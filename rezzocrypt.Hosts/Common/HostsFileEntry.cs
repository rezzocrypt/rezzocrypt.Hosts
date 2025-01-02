using rezzocrypt.Hosts.Common.Base;
using System.Net;
using System.Text.RegularExpressions;

namespace rezzocrypt.Hosts.Common
{
    public class HostsFileEntry
    {
        //chars for trim data
        internal static readonly char[] _charsForTrim = ['\r', '\n', ' ', '\t'];
        //regular expression for check hostname
        internal static readonly Regex _hostNameRegex = new(@"^(([a-z]|[a-z][a-z0-9-]*[a-z0-9]).)*([a-z]|[a-z][a-z0-9-]*[a-z0-9])$");
        //base trim function
        internal static string? Trim(string? data) => data?.Trim(_charsForTrim);
        //raw data
        internal readonly string? _rawData;
        /// <summary>
        /// Entry type
        /// </summary>
        public EntryType Type { get; private set; } = EntryType.Unknown;
        /// <summary>
        /// Comment data if Type is Comment
        /// </summary>
        public string? Comment { get; private set; } = null;
        /// <summary>
        /// Host Entry if Type is Host
        /// </summary>
        public (IPAddress IpAddress, string CanonicalHostname, string[] Aliasses) HostEntry { get; private set; } = default;
        /// <summary>
        /// Result Hosts file string
        /// </summary>
        public override string ToString() => Type switch
        {
            EntryType.Unknown => _rawData ?? "",
            EntryType.Comment => $"# {Comment}",
            EntryType.Host => $"{HostEntry.IpAddress}\t{HostEntry.CanonicalHostname}\t{string.Join(" ", HostEntry.Aliasses)}",
            _ => ""
        };
        /// <summary>
        /// Hosts file parser
        /// </summary>
        /// <param name="rawData">Hosts file line</param>
        public HostsFileEntry(string? rawData = null)
        {
            _rawData = Trim(rawData);
            if (_rawData == null || _rawData.Length == 0)
            {
                Type = EntryType.Empty;
                return;
            }

            if (_rawData.StartsWith('#'))
            {
                Comment = Trim(_rawData[1..]);
                Type = EntryType.Comment;
                return;
            }

            var splittedValues = _rawData
                .Split(['\t', ' '], StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim(_charsForTrim))
                .ToArray();
            if (splittedValues.Length < 2)
                return;
            if (!IPAddress.TryParse(splittedValues[0], out IPAddress? address) || address == null)
                return;
            if (!splittedValues[1..].All(val => _hostNameRegex.IsMatch(val)))
                return;

            HostEntry = (address, splittedValues[1], splittedValues[2..]);
            Type = EntryType.Host;
        }

        /// <summary>
        /// Create Comment entry
        /// </summary>
        /// <param name="comment"></param>
        public static HostsFileEntry SetComment(string comment) => new() { Comment = comment, Type = EntryType.Comment };

        /// <summary>
        /// Create Host entry
        /// </summary>
        /// <param name="address">IP address of the host</param>
        /// <param name="canonicalHostname">Canonical hostname for entry</param>
        /// <param name="aliases">Aliases</param>
        public static HostsFileEntry SetHost(IPAddress address, string canonicalHostname, params string[] aliases)
        {
            if (canonicalHostname.Length == 0 || !_hostNameRegex.IsMatch(canonicalHostname) || !aliases.All(a => _hostNameRegex.IsMatch(a)))
                throw new FormatException("The specified hostname(s) are not valid");

            return new() { Type = EntryType.Host, HostEntry = (address, canonicalHostname, aliases) };
        }

        /// <summary>
        /// Create empty entry
        /// </summary>
        public static HostsFileEntry SetEmpty() => new() { Type = EntryType.Empty };
    }
}
