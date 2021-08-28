// ripple-server-switcher
// © osu!ripple

using System;
using System.Collections;
using System.IO;

namespace BanYouClient
{
    abstract class BaseHostsEntry
    {
        public abstract override string ToString();
    }

    class HostsEntry : BaseHostsEntry
    {
        public string ip;
        public string domain;

        public override string ToString() => $"{ip}\t{domain}";

        public HostsEntry() { }

        public HostsEntry(string ip, string domain)
        {
            this.ip = ip;
            this.domain = domain;
        }

        public override bool Equals(object obj)
        {
            var other = obj as HostsEntry;
            if (other == null)
                return false;
            return ip.Equals(other.ip) && domain.Equals(other.domain);
        }

        public override int GetHashCode() => ToString().GetHashCode();
    }

    class HostsFile
    {
        private string hostsFilePath;

        public HostsFile(string path = null)
        {
            hostsFilePath = path ?? Environment.GetEnvironmentVariable("windir") + "\\system32\\drivers\\etc\\hosts";
            if (File.Exists(hostsFilePath))
            {
                FileInfo fileInfo = new FileInfo(hostsFilePath);
                if (fileInfo.IsReadOnly)
                    fileInfo.IsReadOnly = false;
            }
        }

        public void Write(HostsEntry[] entrys)
        {
            FileStream fs = new FileStream(hostsFilePath, FileMode.Append, FileAccess.Write, FileShare.None);
            using (StreamWriter writer = new StreamWriter(fs))
            {
                foreach (BaseHostsEntry entry in entrys)
                {
                    writer.WriteLine(entry.ToString());
                }
            }
        }
        public void Remove()
        {
            FileStream fs = new FileStream(hostsFilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
            ArrayList lines = new ArrayList();
            using (StreamReader reader = new StreamReader(fs))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {

                    if (!line.EndsWith(".ppy.sh"))
                    {
                        lines.Add(line);
                    }
                }
            }
            File.WriteAllText(hostsFilePath, string.Join("\n", lines.ToArray()) + "\n");
        }
    }
}
