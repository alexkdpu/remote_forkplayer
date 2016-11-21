using System;
using System.Linq;
using System.Text.RegularExpressions;
using RemoteFork.Network;

namespace RemoteFork.Requestes {
    internal class ParseCurlRequest : BaseRequest {
        public ParseCurlRequest(string text) : base(text) {
        }

        public override string Execute() {
            string result;

            string url = Regex.Match(text, "(?:\")(.*?)(?=\")").Groups[1].Value;
            var matches = Regex.Matches(text, "(?:-H\\s\")(.*?)(?=\")");
            var header = (
                from Match match in matches
                select match.Groups
                into groups
                where groups.Count > 1
                select groups[1].Value
                into value
                where value.Contains(": ")
                select value).ToDictionary(value => value.Remove(value.IndexOf(": ", StringComparison.Ordinal)),
                    value => value.Substring(value.IndexOf(": ", StringComparison.Ordinal) + 2));
            if (text.Contains("--data")) {
                string dataString = Regex.Match(text, "(?:--data\\s\")(.*?)(?=\")").Groups[1].Value;
                var dataArray = dataString.Split('&');
                var data =
                    dataArray.ToDictionary(value => value.Remove(value.IndexOf("=", StringComparison.Ordinal)),
                        value => value.Substring(value.IndexOf("=", StringComparison.Ordinal) + 1));
                result = HttpUtility.PostRequest(url, data, header);
            } else {
                result = HttpUtility.GetRequest(url, header);
            }

            return result;
        }
    }
}
