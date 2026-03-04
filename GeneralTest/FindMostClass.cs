using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralTest
{
    [TestClass]
    internal class FindMostClass
    {
        [TestMethod]
        public void GetClassStatInLog()
        {
            var logPath = "C:\\Users\\E.Manurung\\Downloads\\Logs_W11PCWFUELUA003_20250307\\server_W11PCWFUELUA003_20250307.log";
            using (StreamReader sr = File.OpenText(logPath))
            {
                string? s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    Console.WriteLine(s);
                }
            }
}
