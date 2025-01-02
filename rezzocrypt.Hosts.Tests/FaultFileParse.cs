using System.Net;

namespace rezzocrypt.Hosts.Tests
{
    public class Tests
    {
        private readonly string fault_hosts = "_fault_hosts.host";
        private readonly string save_check = "_save_check.host";
        private readonly string valid_save_check = "_valid_save_check.host";

        [SetUp]
        public void Setup()
        {
            if (File.Exists(fault_hosts))
                File.Delete(fault_hosts);
            File.WriteAllText(fault_hosts, @"
                ## Double comment Line
                -1.0.0.0    fault.ip.line
                192.168.0.1 valid_host_url
            ");
        }

        [Test]
        public void SimpleDataTest()
        {
            HostsFile? hosts = null;
            try
            {
                hosts = HostsFile.OpenFile(fault_hosts);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Multiple(() =>
                {
                    Assert.That(hosts == null, Is.False, "Fault parse hosts file!");
                    Assert.That(hosts?.Entries.Count(item => item.Type == Common.Base.EntryType.Empty) == 2, Is.True, "Empty lines not 2!");
                    Assert.That(hosts?.Entries.Count(item => item.Type == Common.Base.EntryType.Comment) == 1, Is.True, "Comment line count not 1!");
                    Assert.That(hosts?.Entries.Count(item => item.Type == Common.Base.EntryType.Host) == 1, Is.True, "Correct host line count not 1!");
                });
            Assert.Pass();
        }

        [Test]
        public void NewFileCreate()
        {
            if (File.Exists(save_check))
                File.Delete(save_check);
            var hosts = HostsFile.OpenFile(save_check);
            Assert.Pass();
        }

        [Test]
        public void CheckValidSave()
        {
            if (File.Exists(valid_save_check))
                File.Delete(valid_save_check);
            var hosts = HostsFile.OpenFile(valid_save_check);
            var commentData = "CommentTest";
            hosts.Entries.Add(Common.HostsFileEntry.SetEmpty());
            hosts.Entries.Add(Common.HostsFileEntry.SetComment("CommentTest"));
            hosts.Entries.Add(Common.HostsFileEntry.SetHost(IPAddress.Parse("127.0.0.1"), "localhost", "local.localhost", "test.localhost"));
            hosts.Save();

            var hostsAgain = HostsFile.OpenFile(valid_save_check);
            Assert.Multiple(() =>
            {
                Assert.That(hostsAgain == null, Is.False, "Fault parse hosts file!");
                Assert.That(hostsAgain?.Entries[0].Type == Common.Base.EntryType.Empty, Is.True, "Empty line not found in first line!");
                Assert.That(hostsAgain?.Entries[1].Type == Common.Base.EntryType.Comment && commentData.Equals(hostsAgain?.Entries[1].Comment), Is.True, "Comment line not found on two line!");
                Assert.That(hostsAgain?.Entries[2].Type == Common.Base.EntryType.Host && hostsAgain?.Entries[2].HostEntry.Aliasses.Length == 2, Is.True, "Host line not found on three line!");
            });


            Assert.Pass();
        }
    }
}