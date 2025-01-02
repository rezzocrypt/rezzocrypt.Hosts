namespace rezzocrypt.Hosts.Tests
{
    public class Tests
    {
        private readonly string fileName = "fault_hosts";

        [SetUp]
        public void Setup()
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
            File.WriteAllText(fileName, @"
                ## Double comment Line
                -1.0.0.0    fault.ip.line
                192.168.0.1 valid_host_url
            ");
        }

        [Test]
        public void Test1()
        {
            HostsFile? hosts = null;
            try
            {
                hosts = HostsFile.OpenFile(fileName);
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
    }
}