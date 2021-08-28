using System.Security.Cryptography.X509Certificates;

namespace BanYouClient
{
    class CertManager
    {
        internal static void InstallCertificate(string cerFileName, StoreName storeName)
        {
            X509Certificate2 certificate = new X509Certificate2(cerFileName);
            X509Store store = new X509Store(storeName);

            store.Open(OpenFlags.ReadWrite);
            store.Add(certificate);
            store.Close();
        }
    }
}
