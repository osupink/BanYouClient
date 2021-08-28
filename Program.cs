using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace BanYouClient
{
    class Program
    {
        public delegate bool ConsoleCtrlDelegate(int dwCtrlType);
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
        static ConsoleCtrlDelegate exitHandler = new ConsoleCtrlDelegate(ExitHandler);
        static HostsFile hostsFile = new HostsFile();
        static ProxyServer proxyServer = new ProxyServer();
        static string CurBanYouClientVer = "b20210828.1";
        static string ProgramTitle = string.Format("BanYou 客户端 ({0})", CurBanYouClientVer);

        private static bool ExitHandler(int CtrlType)
        {
            proxyServer.Stop();
            hostsFile.Remove();
            return false;
        }
        private static async Task OnRequest(object sender, SessionEventArgs e)
        {
            if (!e.HttpClient.Request.Url.Contains("ppy.sh")) return;
            Uri requestUri = e.HttpClient.Request.RequestUri;
            switch (e.HttpClient.Request.Host)
            {
                case "c.ppy.sh":
                case "c1.ppy.sh":
                    e.HttpClient.Request.Host = "server.b.osu.pink";
                    break;
                case "osu.ppy.sh":
                    switch (requestUri.AbsolutePath)
                    {
                        case "/web/coins.php":
                        case "/web/lastfm.php":
                        case "/web/osu-rate.php":
                        case "/web/osu-markasread.php":
                        case "/web/osu-session.php":
                        case "/web/osu-getfriends.php":
                        case "/web/osu-checktweets.php":
                        case "/web/osu-addfavourite.php":
                            e.GenericResponse("", HttpStatusCode.OK);
                            return;
                        case "/web/osu-error.php":
                        case "/web/osu-comment.php":
                        case "/web/osu-getreplay.php":
                        case "/web/osu-osz2-getscores.php":
                        case "/web/osu-submit-modular.php":
                        case "/web/osu-submit-modular-selector.php":
                        case "/web/osu-screenshot.php":
                        case "/web/osu-getbeatmapinfo.php":
                        case "/web/bancho_connect.php":
                            break;
                        default:
                            return;
                    }
                    e.HttpClient.Request.Host = "score.b.osu.pink";
                    break;
            }
        }
        private static void Main(string[] args)
        {
            SetConsoleCtrlHandler(exitHandler, true);
            Console.Title = ProgramTitle;
            Console.WriteLine("BanYou 客户端初始化...");
            CertManager.InstallCertificate("cert/ca.crt", System.Security.Cryptography.X509Certificates.StoreName.Root);
            CertManager.InstallCertificate("cert/osu.crt", System.Security.Cryptography.X509Certificates.StoreName.CertificateAuthority);
            proxyServer.BeforeRequest += OnRequest;
            TransparentProxyEndPoint httpEndPoint = new TransparentProxyEndPoint(IPAddress.Any, 80, false);
            TransparentProxyEndPoint httpsEndPoint = new TransparentProxyEndPoint(IPAddress.Any, 443, true)
            {
                GenericCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2("cert/ppy.sh.pfx")
            };
            proxyServer.AddEndPoint(httpEndPoint);
            proxyServer.AddEndPoint(httpsEndPoint);

            HostsEntry[] hostsEntry = new HostsEntry[]  {
                new HostsEntry { domain="osu.ppy.sh", ip="127.0.0.1" },
                new HostsEntry { domain="c.ppy.sh", ip="127.0.0.1" },
                new HostsEntry { domain="ce.ppy.sh", ip="127.0.0.1" }
            }.Concat((from x in Enumerable.Range(1, 6) select new HostsEntry { domain = String.Format("c{0}.ppy.sh", x), ip = "127.0.0.1" }).ToArray()).ToArray();
            try
            {
                hostsFile.Write(hostsEntry);
            } catch {
                Console.WriteLine("访问被拒绝，请关闭你的杀毒软件然后再试一次.");
                Console.ReadKey(true);
                return;
            }

            Console.WriteLine("正在启动 BanYou 客户端...");
            proxyServer.Start();
            
            Console.WriteLine("启动完成!");
            while (true)
            {
                Console.ReadKey(true);
            }
        }
    }
}
