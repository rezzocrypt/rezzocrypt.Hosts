using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using rezzocrypt.Hosts.Common;

namespace rezzocrypt.Hosts
{
    public class HostsFile
    {
        /// <summary>
        /// File path
        /// </summary>
        public string Path { get; private set; } = string.Empty;
        /// <summary>
        /// File lines
        /// </summary>
        public readonly List<HostsFileEntry> Entries = [];

        /// <summary>
        /// Create host file container
        /// </summary>
        public HostsFile() { }
        /// <summary>
        /// Open hosts file
        /// </summary>
        /// <param name="path">File path</param>
        public HostsFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new FileNotFoundException(null, path);
            if (!File.Exists(path))
                File.Create(path).Close();
            Path = path;
            using var reader = new StreamReader(File.OpenRead(Path));
            while (!reader.EndOfStream)
                Entries.Add(new(reader.ReadLine()));
        }

        /// <summary>
        /// Write hosts file to disk
        /// </summary>
        public void Save() => SaveAs(Path);

        /// <summary>
        /// Save hosts to new file
        /// </summary>
        /// <param name="path">Path to new file</param>
        /// <exception cref="FileNotFoundException"></exception>
        public HostsFile SaveAs(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new FileNotFoundException("Use SaveAs and not empty Path for new save");

            var lines = Entries.Select(item => item.ToString()).ToArray();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var fi = new FileInfo(path);
                // windows security moment
                FileSecurity fileS = fi.GetAccessControl();
                fi.Attributes &= ~FileAttributes.ReadOnly;
                SecurityIdentifier cu = WindowsIdentity.GetCurrent().User;
                var rule = new FileSystemAccessRule(cu, FileSystemRights.Write, AccessControlType.Allow);
                fileS.SetAccessRule(rule);
                fi.SetAccessControl(fileS);
                using var sw = new StreamWriter(path);
                foreach (var line in lines)
                    sw.WriteLine(line);
                // restore old rules
                fi.Attributes |= FileAttributes.ReadOnly;
                fileS.RemoveAccessRule(rule);
                fi.SetAccessControl(fileS);
            }
            else
            {
                using var sw = new StreamWriter(path);
                foreach (var line in lines)
                    sw.WriteLine(line);
            }
            return path == Path
                ? this
                : new HostsFile(path);
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
