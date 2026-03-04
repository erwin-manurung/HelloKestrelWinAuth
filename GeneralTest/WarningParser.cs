using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GeneralTest
{
    [TestClass]
    public class WarningParser

    {
        [TestMethod]
        public void TestNewLine()
        {
            string s = "Hello\r\nworld!";
            int idx = s.IndexOf("\n");
            Assert.AreEqual(6, idx);
        }

        [TestMethod]
        public void FindWarningInLog()
        {

            //first regex
            var regex = new Regex("(\\: warning )([a-zA-Z0-9]{4,15})(:)");
            IEnumerable<string> logFiles = Directory.EnumerateFiles(@"D:\build-log-net8-dev");
            Dictionary<string, List<string>> fileWarningMap = new Dictionary<string, List<string>>();
            Dictionary<string, Dictionary<string, int>> WarningCounter = new Dictionary<string, Dictionary<string, int>>();

            foreach (string logFile in logFiles)
            {
                var fileName = Path.GetFileName(logFile);
                WarningCounter.TryAdd(fileName, new Dictionary<string, int>());
                string[] logLines = File.ReadAllLines(logFile);
                int lineCounter = 0;
                foreach (string line in logLines)
                {
                    lineCounter++;
                    var matches = regex.Matches(line);
                    foreach (Match match in matches)
                    {
                        if (match.Success)
                        {
                            var warningCode = match.Groups[2].Value;
                            //warningCode += $"({lineCounter})";
                            if (!fileWarningMap.TryGetValue(fileName, out var innerList))
                            {
                                List<string> newInnerList = new List<string>();
                                newInnerList.Add(warningCode);
                                fileWarningMap[fileName] = newInnerList;
                            }
                            else
                            {

                                if (!fileWarningMap[fileName].Contains(warningCode))
                                {
                                    fileWarningMap[fileName].Add(warningCode);
                                }
                            }
                            if (WarningCounter.TryGetValue(fileName, out var fileWarnings))
                            {
                                if (fileWarnings.TryGetValue(warningCode, out var counter))
                                {
                                    counter++;
                                    fileWarnings[warningCode] = counter;
                                }
                                else
                                {
                                    counter = 1;
                                    fileWarnings.TryAdd(warningCode, counter);
                                }


                            }
                        }
                    }

                }
                // show all
                Console.WriteLine($"Log: {fileName}: {string.Join(",", fileWarningMap[fileName].Select(a => a + ":" + WarningCounter[fileName][a]).ToArray())}");
                // show only syslib
                //Console.WriteLine($"Log: {fileName}: {string.Join(",", fileWarningMap[fileName].Where(a => a.StartsWith("SYS")).Select(a => a+":"+WarningCounter[fileName][a]).ToArray())}");

            }
        }
    }
}