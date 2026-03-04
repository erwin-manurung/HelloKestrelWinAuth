using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GeneralTest
{
    [TestClass]
    public class MemoryHeartbeatParser
    {
        private static List<string> MemoryListCounter = new List<string>();
        [TestMethod]
        public void ParseURL()
        {
            var counterLine = 1;
            var logPath = "C:\\Users\\E.Manurung\\Downloads\\Logs_W11PCWFUELUA003_20250307\\server_W11PCWFUELUA003_20250307.log";
            using (StreamReader sr = File.OpenText(logPath))
            {
                string? s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    try
                    {
                        MemHeartBeatProcessor(counterLine, s);
                        counterLine++;
                    }
                    catch (Exception e)
                    {
                        e.ToString();
                    }
                }
            }

            File.WriteAllText(Path.Combine(Path.GetDirectoryName(logPath), "distinct-mem.txt"), ListToString<string>(MemoryListCounter));

        }

        private void MemHeartBeatProcessor(int counter, string s)
        {
            var regexFilter = "^([0-9]{4}-[0-9]{2}-[0-9]{2})(T)([0-9]{2}:[0-9]{2}:[0-9]{2})([\\S ]+)(LogMemoryUsageHeartBeat)([\\S ]+)(PrivateMemory \\: )([0-9]+)( MB)";
            var match = Regex.Match(s, regexFilter);
            if (match.Success)
            {
                //var finalUrl = match.Groups[6].Value;
                //AddToMemoryList(counter+"\t"+finalUrl);
                AddToMemoryList(match.Groups[3].Value + "\t" + match.Groups[8].Value);
            }
        }
        private void AddToMemoryList(string url)
        {
            var lowerurl = url.ToLower();
            MemoryListCounter.Add(lowerurl);
        }
        private string ListToString<T>(List<T> requests)
        {
            StringBuilder sb = new StringBuilder();
            foreach (object r in requests)
            {
                sb.Append(r).AppendLine();
            }
            return sb.ToString();
        }
    }
}
