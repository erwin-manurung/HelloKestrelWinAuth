using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HelloDiagnostics
{
    internal class ThreadMonitoring
    {
        private static int _threadCount = 0;
        private static bool _started = false;
        internal static int mode = 1;
        public static int TotalQ => _threadCount;
        public static bool Started => _started;
        // mapped of managedid and osthread id
        //private static Dictionary<int/*managedid*/, int/*threadid*/> ManagedThreadIDMap = new Dictionary<int, int>();
        private static ConcurrentDictionary<int/*managedid*/, int/*threadid*/> ManagedThreadIDMap = new ConcurrentDictionary<int, int>();
        private static readonly object _lock = new object();
        static int GetCurrentManagedThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }
        public static void Print()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in ManagedThreadIDMap.Keys)
            {
                sb.Append(item).Append(" ");
            }
            Log(sb.ToString());
        }
        internal static void Log(object o)
        {
            //Console.WriteLine(o);
        }
        public static int MapThread()
        {
            //lock (_lock)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                var mapid = MapThreadImpl();
                stopwatch.Stop();
                //Console.WriteLine($"worker elapsed(ori):{stopwatch.ElapsedMilliseconds}");
                return mapid;
            }
        }
        internal static int MapThreadImpl()
        {
            int currentManThreadId = GetCurrentManagedThreadId();
            ProcessThreadCollection ptcs = Process.GetCurrentProcess().Threads;
            try
            {
                if (mode == 1)
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    Microsoft.Diagnostics.Runtime.DataTarget target = Microsoft.Diagnostics.Runtime.DataTarget.CreateSnapshotAndAttach(Process.GetCurrentProcess().Id);
                    sw.Stop();
                    Console.WriteLine($"Create Snapshot:{sw.ElapsedMilliseconds}");
                    //using (Microsoft.Diagnostics.Runtime.DataTarget target = Microsoft.Diagnostics.Runtime.DataTarget.CreateSnapshotAndAttach(Process.GetCurrentProcess().Id))
                    try
                    {
                        Microsoft.Diagnostics.Runtime.ClrRuntime runtime = target.ClrVersions.First().CreateRuntime();
                        foreach (Microsoft.Diagnostics.Runtime.ClrThread thread in runtime.Threads)
                        {
                            var mtid = thread.ManagedThreadId;
                            int ptid = (int)thread.OSThreadId;
                            ProcessThread pt = default;
                            if (mtid == currentManThreadId)
                            {
                                foreach (ProcessThread ptc in ptcs)
                                {
                                    if (ptid == ptc.Id)
                                    {
                                        pt = ptc;
                                        break;
                                    }
                                }
                                //Console.WriteLine($"Adding into map managedid:osthread -> {currentManThreadId}:{pt?.Id}");
                                if (ManagedThreadIDMap.TryAdd(currentManThreadId,
                                    pt.Id))
                                {
                                    Log($"Adding Q {currentManThreadId}");
                                    _threadCount++;
                                }

                                break;
                            }
                        }
                    }
                    finally
                    {
                        if (target != null)
                        {
                            target.Dispose();
                        }
                    }
                }
                else if (mode == 2)
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    Microsoft.Diagnostics.Runtime.DataTarget target = Microsoft.Diagnostics.Runtime.DataTarget.CreateSnapshotAndAttach(Process.GetCurrentProcess().Id);
                    sw.Stop();
                    Console.WriteLine($"Create Snapshot:{sw.ElapsedMilliseconds}");

                    ImmutableArray< Microsoft.Diagnostics.Runtime.ClrThread> ClrThreads = new ImmutableArray<Microsoft.Diagnostics.Runtime.ClrThread> ();
                    //using (Microsoft.Diagnostics.Runtime.DataTarget target = Microsoft.Diagnostics.Runtime.DataTarget.CreateSnapshotAndAttach(Process.GetCurrentProcess().Id))
                    try{
                        Microsoft.Diagnostics.Runtime.ClrRuntime runtime = target.ClrVersions.First().CreateRuntime();
                        ClrThreads = runtime.Threads;
                    }
                    finally
                    {
                        if (target != null)
                        {
                            target.Dispose();
                        }
                    }

                    using (Process p = new Process())
                    {
                        string scmd = "ps";
                        var pAssigned = p != null;
                        var pStartInfo = p?.StartInfo != null;

                        p.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.FileName = "dotnet-stack";
                        p.StartInfo.Arguments = $"{scmd}";
                        p.Start();
                        var outResult = p.StandardOutput.ReadToEnd();
                        File.WriteAllText("D:\\tmp\\dotnet-stack.txt", outResult);
                        string[] allLines = outResult.Split("\\r\\n");
                        foreach (string line in allLines)
                        {
                            var cleanLine = line.Trim();
                            var lineRows = cleanLine.Split(" ");
                            Console.WriteLine(lineRows[0]+","+ lineRows[1]);
                        }
                        p.WaitForExit();
                    }
                    foreach (Microsoft.Diagnostics.Runtime.ClrThread thread in ClrThreads)
                    {
                        var mtid = thread.ManagedThreadId;
                        int ptid = (int)thread.OSThreadId;
                        Console.WriteLine($"--{ptid}");
                        ProcessThread pt = default;
                        if (mtid == currentManThreadId)
                        {
                            foreach (ProcessThread ptc in ptcs)
                            {
                                if (ptid == ptc.Id)
                                {
                                    pt = ptc;
                                    break;
                                }
                            }
                            //Console.WriteLine($"Adding into map managedid:osthread -> {currentManThreadId}:{pt?.Id}");
                            if (ManagedThreadIDMap.TryAdd(currentManThreadId,
                                pt.Id))
                            {
                                Log($"Adding Q {currentManThreadId}");
                                _threadCount++;
                            }
                            break;
                        }
                    }
                }
                return currentManThreadId;
            }
            catch (Exception ex)
            {
                Log($"MapThread Err: {ex.Message}");
            }
            return currentManThreadId;
        }

        public static void UnMapThread(int currentManThreadId)
        {
            lock (_lock)
            {
                if (ManagedThreadIDMap.TryGetValue(currentManThreadId, out var ptid))
                {
                    _threadCount--;
                    Log($"Resolve and Remove Q {currentManThreadId}");
                    ManagedThreadIDMap.TryRemove(currentManThreadId, out _);
                }
            }
        }
    }
}
