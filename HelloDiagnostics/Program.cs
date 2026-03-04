using HelloDiagnostics;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        for (int i = 0; i < 2; i++)
        {
            Stopwatch stopwatch = new Stopwatch();
            WorkerManager workerManager = new WorkerManager();

            ThreadMonitoring.mode = 2;
            workerManager.GenerateQ(2);
            stopwatch.Start();
            workerManager.ConsumeQ();
            Console.WriteLine($"Total Q {workerManager.TotalQ}");
            Task.Delay(10).Wait();
            while (ThreadMonitoring.Started && ThreadMonitoring.TotalQ > 0)
            {
                //Console.WriteLine($"Total Q Left: {ThreadMonitoring.TotalQ}");
                Task.Delay(10).Wait();
            }
            stopwatch.Stop();
            Console.WriteLine($"Total Time:{stopwatch.ElapsedMilliseconds}");
            if (i % 5 == 0)
            {
                Task.Delay(2000).Wait();
            }
        }

    }
}