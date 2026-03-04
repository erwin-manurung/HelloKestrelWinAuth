using Microsoft.Diagnostics.Runtime.Utilities;
using Microsoft.Management.Infrastructure;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace GeneralTest
{
    [TestClass]
    public class WindowsServerInfoTests
    {
        private float GetCpuUsageForProcess()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = 0d;
            startCpuUsage = Process.GetProcesses().Sum((a) =>
            {
                try
                {
                    return a.TotalProcessorTime.TotalMilliseconds;
                }
                catch { return 0; }
            });
            Task.Delay(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetProcesses().Sum((a) =>
            {
                try
                {
                    return a.TotalProcessorTime.TotalMilliseconds;
                }
                catch { return 0; }
            });

            var cpuUsedMs = endCpuUsage - startCpuUsage;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            float cpuFinalUsage = (float)cpuUsageTotal * 100;
            return cpuFinalUsage;
        }

        [TestMethod]
        public void GetCPUUsage()
        {
            GetCpuUsageForProcess();
        }
        [TestMethod]
        public void WinOS()
        {
            ManagementObjectCollection moc = new ManagementClass("Win32_OperatingSystem").GetInstances();
            foreach (ManagementBaseObject o in moc)
            {
                ManagementObject mo = (ManagementObject)o;
                string name = mo.Properties["Name"].Value.ToString().Trim();
                Console.WriteLine(name);
                //while (name.IndexOf("   ", StringComparison.Ordinal) > 0) name = name.Replace("   ", " ");
                //while (name.IndexOf("  ", StringComparison.Ordinal) > 0) name = name.Replace("  ", " ");
            }
        }
        [TestMethod]
        public void WinPerfTest_WMI()
        {
            System.Diagnostics.PerformanceCounter cpuCounter;
            cpuCounter = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total");
            var val = cpuCounter.NextValue();
            val.ToString();
        }
        [TestMethod]
        public void WinProcessor()
        {
            ManagementObjectCollection moc = new ManagementClass("Win32_Processor").GetInstances();
            foreach (ManagementBaseObject o in moc)
            {
                ManagementObject mo = (ManagementObject)o;
                string name = mo.Properties["Name"].Value.ToString().Trim();
                Console.WriteLine(name);
                //while (name.IndexOf("   ", StringComparison.Ordinal) > 0) name = name.Replace("   ", " ");
                //while (name.IndexOf("  ", StringComparison.Ordinal) > 0) name = name.Replace("  ", " ");
            }

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                {
                    foreach (var item in searcher.Get())
                    {
                        string name = item["Name"].ToString().Trim();
                        Console.WriteLine(name);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
