using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloDiagnostics
{
    internal class MyWorker : ISimpleWorker
    {
        public void Invoke()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int tid = ThreadMonitoring.MapThread();
            stopwatch.Stop();
            //Console.WriteLine($"worker elapsed(Lock):{stopwatch.ElapsedMilliseconds}");
            //int rnd = new Random().Next(0, 1000);
            int rnd = 10;
            ThreadMonitoring.Log($"#{tid} Waiting for {rnd} ");
            Task.Delay(rnd).Wait();
            ThreadMonitoring.UnMapThread(tid);
        }
    }
}
