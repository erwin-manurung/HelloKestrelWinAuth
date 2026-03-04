using Microsoft.Diagnostics.Runtime;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace GeneralTest
{
    [TestClass]
    public class MixCase
    {

        [TestMethod]
        public void FilterLogs()
        {
            var resultLists = new List<string>();
            string rowPattern = @"^(2024-10-04T06:)";
            //string pattern = @"^(2024-10-04T06:)";
            string excludePattern01 = @"^(2024-10-04T06:)(.*)(Request starting)(.*)";
            string excludePattern02 = @"^(2024-10-04T06:)(.*)(Request finished)(.*)";
            string excludePattern03 = @"^(2024-10-04T06:)(.*)(Allegro.Serialization.Protocol.Soap.AllegroMtomDeserializer)(.*)";
            string excludePattern04 = @"^(2024-10-04T06:)(.*)(Allegro.Core.Server.Mgmt.ServerManager ServerStats)(.*)";
            var dir = @"D:\tmp\filterlog\";
            var path = Path.Combine(dir, "server_20241004.log");
            var pathRes = Path.Combine(dir, "server_20241004-filtered.log");
            var allLines = File.ReadAllLines(path);
            var prevMatchRow = false;
            foreach (var line in allLines)
            {
                //var currMatchRow = Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase);
                var currMatchRow = !Regex.IsMatch(line, excludePattern01, RegexOptions.IgnoreCase)
                    && !Regex.IsMatch(line, excludePattern02, RegexOptions.IgnoreCase)
                    && !Regex.IsMatch(line, excludePattern03, RegexOptions.IgnoreCase)
                    && !Regex.IsMatch(line, excludePattern04, RegexOptions.IgnoreCase);
                var validRow = Regex.IsMatch(line, rowPattern, RegexOptions.IgnoreCase);
                if (currMatchRow && validRow)
                {
                    resultLists.Add(line);
                }

                if (prevMatchRow && !validRow)
                {
                    resultLists.Add(line);
                }
                if (validRow)
                {
                    prevMatchRow = currMatchRow;
                }
            }

            File.WriteAllLines(pathRes, resultLists.ToArray());

        }
        [TestMethod]
        public void TestMethod1()
        {
            using (DataTarget target = DataTarget.AttachToProcess( Process.GetCurrentProcess().Id, false))
            {
                ClrRuntime runtime = target.ClrVersions.First().CreateRuntime();
                foreach (ClrThread thread in runtime.Threads)
                {
                    Console.WriteLine($"Managed Thread ID :{thread.ManagedThreadId}");
                    Console.WriteLine($"OS Thread ID :{thread.OSThreadId}");
                }
            }
        }

        private static List<string> UrlListCounter = new List<string>();
        private static List<string> MemoryListCounter = new List<string>();
        private static List<string> ClearCacheCounter = new List<string>();
        
        private static List<string> AllReqListCounter = new List<string>();

        private static Dictionary<string, int> DistinctWebMethodMap = new Dictionary<string, int>();
        private static List<String> WebMethodList = new List<String>();
        private static Dictionary<string, RequestObjDur> RequestDurationMap = new Dictionary<string, RequestObjDur>();
        private static Dictionary<string, int> LastReqDur = new Dictionary<string, int>();
        private static List<RequestObjDur> RequestFinishedList = new List<RequestObjDur>();
        private static string CurrentMemState = "";
        private static string lastFinalUrl = "";
        [TestMethod]
        public void ParseURL()
        {
            var counterLine = 1;
            var logPath = "";
            //logPath = "C:\\Users\\E.Manurung\\Downloads\\Logs_W11PCWFUELUA003_Incident_Triggered_11_53_AM\\sample.log";
            //logPath = "C:\\Users\\E.Manurung\\Downloads\\Logs_W11PCWFUELUA003_Incident_Triggered_11_53_AM\\server_W11PCWFUELUA003_20250308.log";
            //logPath = "C:\\Users\\E.Manurung\\Downloads\\Logs_W11PCWFUELUA003_20250307\\spike-memory-log-clean.txt";
            //logPath = "D:\\horizon-run\\Horizon_6013\\Horizon-16.145.5.6-rev6013-02\\server\\logs\\server02_20250326.log";
            logPath = "C:\\Users\\E.Manurung\\Downloads\\Logs_W11PCWFUELUA003_20250307\\server_W11PCWFUELUA003_20250307.log";
            //logPath = "D:\\horizon-run\\Horizon_6013\\Horizon-16.145.5.6-rev6013-02\\server\\logs\\server02_20250314-view.log";
            //logPath = "D:\\horizon-run\\Horizon-16.145.5.7-rev5817-Windows\\server\\logs\\server_20240815.log";
            //logPath = "C:\\Users\\E.Manurung\\Downloads\\Logs-jktadcdevapp02\\server_20250326.log";
            using (StreamReader sr = File.OpenText(logPath))
            {
                    string? s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    try
                    {
                        //RequestStartingProcessor(s);
                        AllRequestProcessor(s);
                        ClearCacheProcessor(counterLine, s);
                        WebMethodProcessor(s);
                        RequestDurationProcessor(counterLine, s);
                        MaxRequestDurationProcessor(counterLine, s);
                        MemHeartBeatProcessor(counterLine, s);
                        counterLine++;
                    }
                    catch (Exception e)
                    {
                        e.ToString();
                    }
                }
            }

            File.WriteAllText(Path.Combine(Path.GetDirectoryName(logPath), "webmethod.txt"), ListToString<string>(WebMethodList));
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(logPath), "distinct-mem.txt"), ListToString<string>(MemoryListCounter));
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(logPath), "distinct-req.txt"), ListToString<string>(AllReqListCounter));
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(logPath), "distinct-method.txt"), MapToString(DistinctWebMethodMap));
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(logPath), "duration-url.txt"), ListToString<RequestObjDur>(RequestFinishedList));
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(logPath), "clear-cache.txt"), ListToString<string>(ClearCacheCounter));
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(logPath), "last-dur.txt"), MapToString(LastReqDur));
            
        }

        private void RequestStartingProcessor(string s)
        {
            var regexFilter = "(Request starting HTTP/1.1 POST )(http://)([\\S]+)(asmx)";
            var match = Regex.Match(s, regexFilter);
            if (match.Success)
            {
                var finalUrl = match.Groups[2].Value + match.Groups[3].Value + match.Groups[4].Value;
                AddToList(UrlListCounter, finalUrl);
            }
        }
        private void MemHeartBeatProcessor(int counter,string s)
        {
            var regexFilter = "^([0-9]{4}-[0-9]{2}-[0-9]{2})(T)([0-9]{2}:[0-9]{2}:[0-9]{2})([\\S ]+)(LogMemoryUsageHeartBeat)([\\S ]+)(PrivateMemory \\: )([0-9]+)( MB)";
            var match = Regex.Match(s, regexFilter);
            if (match.Success)
            {
                //var finalUrl = match.Groups[6].Value;
                //AddToMemoryList(counter+"\t"+finalUrl);
                AddToMemoryList(match.Groups[3].Value + "\t" +match.Groups[8].Value);
            }
        }
        private void ClearCacheProcessor(int counter, string s)
        {
            var regexFilter = "(Total # of items to clear = )([0-9]+)";
            var match = Regex.Match(s, regexFilter);
            if (match.Success)
            {
                if (match.Groups[2].Value != "0")
                {
                    var finalUrl = match.Groups[0].Value;
                    AddToClearCache(counter + "\t" + finalUrl);
                }
            }
        }

        //
        private string GetUrl(Match? regexMatch)
        {
            return regexMatch.Groups[2].Value + regexMatch.Groups[3].Value + regexMatch.Groups[4].Value + regexMatch.Groups[5].Value + regexMatch.Groups[6].Value;
        }
        
        private void MaxRequestDurationProcessor(int counter, string s)
        {
            var reqStartRegex = "(Request starting HTTP/1.1 POST )(http://)(\\S+)(\\S+)(asmx)";
            var reqFinishRegex = "(Request finished HTTP/1.1 POST )(http://)(\\S+)(\\S+)(asmx)";
            var threadRegex = "( \\()(\\S+)(\\) )(Request)";
            var timeRegex = "([0-9]{4}-[0-9]{2}-[0-9]{2}T)([0-9]{2}:[0-9]{2}:[0-9]{2})";

            var reqStartMatch = Regex.Match(s, reqStartRegex);
            var reqFinishMatch = Regex.Match(s, reqFinishRegex);

            if (reqStartMatch.Success)
            {
                var reqThreadMatch = Regex.Match(s, threadRegex);
                var reqTimeMatch = Regex.Match(s, timeRegex);
                var finalUrl = GetUrl(reqStartMatch);
                var threadKey = reqThreadMatch.Groups[2].Value;
                var curTime = DateTime.Parse(reqTimeMatch.Groups[0].Value);

                var durObj = new RequestObjDur()
                {
                    url = finalUrl,
                    start = curTime,
                    threadKey = threadKey,
                    startLine = counter
                };
                AddStartedList(durObj);
            }
            if (reqFinishMatch.Success)
            {
                var reqThreadMatch = Regex.Match(s, threadRegex);
                var reqTimeMatch = Regex.Match(s, timeRegex);
                var finalUrl = GetUrl(reqFinishMatch);
                var threadKey = reqThreadMatch.Groups[2].Value;
                var curTime = DateTime.Parse(reqTimeMatch.Groups[0].Value);

                var durObj = new RequestObjDur()
                {
                    url = finalUrl,
                    end = curTime,
                    threadKey = threadKey,
                    endLine = counter
                };
                AddFinishedList(durObj);
            }
        }
        private void RequestDurationProcessor(int counter, string s)
        {
            var reqStartRegex = "(Request starting HTTP/1.1 POST )(http://)(\\S+)(\\S+)(asmx)";
            var reqFinishRegex = "(Request finished HTTP/1.1 POST )(http://)(\\S+)(\\S+)(asmx)";
            var threadRegex = "( \\()(\\S+)(\\) )";
            var timeRegex = "([0-9]{4}-[0-9]{2}-[0-9]{2}T)([0-9]{2}:[0-9]{2}:[0-9]{2})";
            var durRegex = "([0-9.]+)(ms)$";

            var reqStartMatch = Regex.Match(s, reqStartRegex);
            var reqFinishMatch = Regex.Match(s, reqFinishRegex);
            var durMatch = Regex.Match(s, durRegex);
            var reqThreadMatch = Regex.Match(s, threadRegex);
            var threadKey = reqThreadMatch.Groups[2].Value;
            var reqTimeMatch = Regex.Match(s, timeRegex);

            if (reqStartMatch.Success)
            {
                var finalUrl = GetUrl(reqStartMatch);

                var curTime = DateTime.Parse(reqTimeMatch.Groups[0].Value);

                var durObj = new RequestObjDur()
                {
                    url = finalUrl,
                    start = curTime,
                    threadKey = threadKey,
                    startLine = counter
                };
                AddStartedList(durObj);
            }
            if (reqFinishMatch.Success)
            {
                //var reqTimeMatch = Regex.Match(s, timeRegex);
                var finalUrl = GetUrl(reqFinishMatch);
                //var threadKey = reqThreadMatch.Groups[2].Value;
                var curTime = DateTime.Parse(reqTimeMatch.Groups[0].Value);
                var durString = durMatch.Groups[1].Value.Split('.')[0];
                var durObj = new RequestObjDur()
                {
                    url = finalUrl,
                    end = curTime,
                    threadKey = threadKey,
                    endLine = counter,
                    durString = durString
                };
                AddFinishedList(durObj);
            }
        }
        
        private void AllRequestProcessor(string s)
        {
            var regexFilter = "(http://)([\\S]+)(asmx)";
            var match = Regex.Match(s, regexFilter);
            if (match.Success)
            {
                var finalUrl = match.Groups[0].Value;
                AddToDistinctReqList(finalUrl);
            }
        }

        private void WebMethodProcessor(string s)
        {
            var regexMemFilter = "^([0-9]{4}-[0-9]{2}-[0-9]{2})(T)([0-9]{2}:[0-9]{2}:[0-9]{2})([\\S ]+)(LogMemoryUsageHeartBeat)([\\S ]+)(PrivateMemory \\: )([0-9]+)( MB)";
            var matchMem = Regex.Match(s, regexMemFilter);
            if (matchMem.Success)
            {
                CurrentMemState = matchMem.Groups[8].Value;
            }

            var timeRegex = "([0-9]{4}-[0-9]{2}-[0-9]{2}T)([0-9]{2}:[0-9]{2}:[0-9]{2})";
            var matchTime = Regex.Match(s, timeRegex);

            var regexFilter = "(webmethodnameperendpoint=)(\\S+)";
            var match = Regex.Match(s, regexFilter);

            var regexDeserialized = "(begin deserialize soap request for path )(/ClassEvents)(/(\\S+)+(.asmx))";
            var regexDeserializedMatch = Regex.Match(s, regexDeserialized);
            var finalUrl = "";
            var finalValue = "";
            if (regexDeserializedMatch.Success)
            {
                finalUrl = regexDeserializedMatch.Groups[2].Value+""+ regexDeserializedMatch.Groups[3].Value;
                if (!lastFinalUrl.Equals(finalUrl, StringComparison.OrdinalIgnoreCase))
                {
                    lastFinalUrl = finalUrl;
                    finalValue = matchTime.Groups[2].Value + "\t" + finalUrl + "\t" + CurrentMemState;
                }
            }
            if (match.Success)
            {
                finalUrl = match.Groups[2].Value;
                if (!lastFinalUrl.Equals(finalUrl, StringComparison.OrdinalIgnoreCase))
                {
                    lastFinalUrl = finalUrl;
                    finalValue = matchTime.Groups[2].Value + "\t" + finalUrl + "\t" + CurrentMemState;
                }
            }
            if (!string.IsNullOrEmpty(finalUrl))
            {
                AddToWebMethod(finalUrl);
            }
            if (!string.IsNullOrEmpty(finalValue) && !(finalUrl.Contains("HealthCheck") || finalUrl.Contains("PushSync") || finalUrl.Contains("PushNotifier")))
            {
                WebMethodList.Add(finalValue);
            }

        }

        private void AddToWebMethod(string finalUrl)
        {
            int tmpTotal = 0;

            if (!DistinctWebMethodMap.TryGetValue(finalUrl, out tmpTotal))
            {
                DistinctWebMethodMap.Add(finalUrl, 1);
            }
            else
            {
                DistinctWebMethodMap.Remove(finalUrl);
                tmpTotal++;
                DistinctWebMethodMap.Add(finalUrl, tmpTotal);
            }
        }
        private string MapToString(Dictionary<string, int> map)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in map)
            {
                sb.Append(kvp.Key).Append(" > ").Append(kvp.Value).AppendLine();
            }
            return sb.ToString();
        }

        private string ListToString<T>(List<T> requests)
        {
            StringBuilder sb = new StringBuilder();
            foreach (object r in requests)
            {
                if (r is RequestObjDur)
                {
                    RequestObjDur? request = r as RequestObjDur;
                    sb.Append(request.startLine).Append("-").Append(request.endLine).Append("\t")
                        .Append(request.start).Append("-").Append(request.end).Append("\t")
                        .Append(request.threadKey).Append("\t")
                        .Append(request.url).Append("\t").Append(request.duration.TotalSeconds.ToString()).Append("\t")
                        .Append(request.durString).AppendLine();
                }
                else
                {
                    sb.Append(r).AppendLine();
                }
            }
            return sb.ToString();
        }
        private void AddToList(List<string> main, string url)
        {
            var lowerurl = url.ToLower();
            main.Add(lowerurl);
        }
        private void AddToDistinctReqList(string url)
        {
            var lowerurl = url.ToLower();
            if (!AllReqListCounter.Contains(lowerurl))
            {
                AllReqListCounter.Add(lowerurl);
            }
        }
        


        private void AddToMemoryList(string url)
        {
            var lowerurl = url.ToLower();
            MemoryListCounter.Add(lowerurl);
        }
        private void AddToClearCache(string url)
        {
            var lowerurl = url.ToLower();
            ClearCacheCounter.Add(lowerurl);
        }
        private void AddStartedList(RequestObjDur requestObjDur)
        {
            var finalKey = requestObjDur.threadKey+"-"+requestObjDur.url;
            if (!RequestDurationMap.TryGetValue(finalKey, out _))
            {
                RequestDurationMap.Add(finalKey, requestObjDur);
            }
            else
            {
                RequestDurationMap.Remove(finalKey);
                RequestDurationMap.Add(finalKey, requestObjDur);
            }
        }
        private void AddFinishedList(RequestObjDur requestObjDur)
        {
            var finalKey = requestObjDur.threadKey + "-" + requestObjDur.url;
            //var finalKey = requestObjDur.threadKey;
            RequestObjDur? durObj = new RequestObjDur();
            if (RequestDurationMap.TryGetValue(finalKey, out durObj))
            {
                durObj.end = requestObjDur.end;
                durObj.duration = durObj.end - durObj.start;
                durObj.endLine = requestObjDur.endLine;
                durObj.durString = requestObjDur.durString;
                RequestFinishedList.Add(durObj);
                RequestDurationMap.Remove(finalKey);

                // 
                var lastDur = 0;
                if (LastReqDur.TryGetValue(durObj.url.ToLower(), out lastDur))
                {
                    //if (lastDur < durObj.duration.TotalSeconds)
                    if (lastDur < int.Parse(durObj.durString))
                    {
                        LastReqDur.Remove(durObj.url.ToLower());
                        //LastReqDur.Add(durObj.url.ToLower(), int.Parse(durObj.duration.TotalSeconds.ToString()));
                        LastReqDur.Add(durObj.url.ToLower(), int.Parse(durObj.durString));
                    }
                }
                else
                {
                    //LastReqDur.Add(durObj.url.ToLower(), int.Parse(durObj.duration.TotalSeconds.ToString()));
                    LastReqDur.Add(durObj.url.ToLower(), int.Parse(durObj.durString));
                }

            }
            // start checking the last url

        }

        class RequestObjDur
        {
            public string threadKey;
            public string url;
            public DateTime start;
            public DateTime end;
            public TimeSpan duration;
            public int startLine;
            public int endLine;
            public string durString;
        }
    }
}