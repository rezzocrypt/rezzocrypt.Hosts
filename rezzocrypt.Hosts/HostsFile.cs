using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using rezzocrypt.Hosts.Common;

namespace rezzocrypt.Hosts
{
    public class HostsFile
    {
        public string Path { get; private set; }
        public readonly List<HostsFileEntry> Entries = [];

        /// <summary>
        /// Rade hosts file
        /// </summary>
        /// <param name="path"></param>
        internal HostsFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                throw new FileNotFoundException(null, path);
            Path = path;
            using var reader = new StreamReader(File.OpenRead(Path));
            while (!reader.EndOfStream)
                Entries.Add(new(reader.ReadLine()));
        }

        /// <summary>
        /// Writes the hosts file to disk
        /// </summary>
        public void Save()
        {
            var lines = Entries.Select(item => item.ToString()).ToArray();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var fi = new FileInfo(Path);
                // windows security moment
                FileSecurity fileS = fi.GetAccessControl();
                fi.Attributes &= ~FileAttributes.ReadOnly;
                SecurityIdentifier cu = WindowsIdentity.GetCurrent().User;
                var rule = new FileSystemAccessRule(cu, FileSystemRights.Write, AccessControlType.Allow);
                fileS.SetAccessRule(rule);
                fi.SetAccessControl(fileS);
                using var sw = new StreamWriter(Path);
                foreach (var line in lines)
                    sw.WriteLine(line);
                // restore old rules
                fi.Attributes |= FileAttributes.ReadOnly;
                fileS.RemoveAccessRule(rule);
                fi.SetAccessControl(fileS);
            }
            else
            {
                using var sw = new StreamWriter(Path);
                foreach (var line in lines)
                    sw.WriteLine(line);
            }

        }

        /// <summary>
        /// Open hosts file
        /// </summary>
        /// <param name="filePath">hosts file path</param>
        public static HostsFile OpenFile(string? filePath = default)
        {
            filePath = filePath ?? Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\drivers\etc\hosts";
            return new HostsFile(filePath);
        }
    }
}
