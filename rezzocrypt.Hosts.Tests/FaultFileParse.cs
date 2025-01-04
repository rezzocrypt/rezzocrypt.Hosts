using rezzocrypt.Hosts.Common;
using System.Net;

namespace rezzocrypt.Hosts.Tests
{
    public class Tests
    {
        private readonly string save_check = "_save_check.host";
        private readonly string valid_save_check = "_valid_save_check.host";

        [SetUp]
        public void Setup() { }

        [Test]
        public void SimpleDataTest()
        {
            var hosts = new HostsFile();
            hosts.Entries.Add(new HostsFileEntry("## Double comment Line"));
            hosts.Entries.Add(new HostsFileEntry("-1.0.0.0    fault.ip.line"));
            hosts.Entries.Add(new HostsFileEntry("192.168.0.1 valid_host_url"));
            Assert.Multiple(() =>
                {
                    Assert.That(hosts.Entries[0].Type == Common.Base.EntryType.Comment, Is.True, "First line is not Comment!");
                    Assert.That(hosts.Entries[1].Type == Common.Base.EntryType.Unknown, Is.True, "Second line is not Unknown. Check parse format!");
                    Assert.That(hosts.Entries[2].Type == Common.Base.EntryType.Host, Is.True, "Third line is not Host!");
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
            var commentData = "CommentTest";
            var hosts = new HostsFile();
            hosts.Entries.Add(Common.HostsFileEntry.GetEmpty());
            hosts.Entries.Add(Common.HostsFileEntry.GetComment("CommentTest"));
            hosts.Entries.Add(Common.HostsFileEntry.GetHost(IPAddress.Parse("127.0.0.1"), "localhost", "local.localhost", "test.localhost"));
            hosts.SaveAs(valid_save_check);

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

        [Test]
        public void CheckCommentUncomment()
        {
            var line = new Common.HostsFileEntry("127.0.0.1 test.ru comment.ru");
            Assert.IsTrue(line.Type == Common.Base.EntryType.Host, "Not parse host data");
            line.CommentLine();
            Assert.IsTrue(line.Type == Common.Base.EntryType.Comment, "Data not set as comment");
            line.UncommentLine();
            Assert.IsTrue(line.Type == Common.Base.EntryType.Host, "Data not host data after change from comment!");
            Assert.Pass();
        }
    }
}