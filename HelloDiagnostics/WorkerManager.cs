using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloDiagnostics
{
    internal class WorkerManager
    {
        private List<ISimpleWorker> workers = new List<ISimpleWorker>();
        public void GenerateQ(int total)
        {
            for (int a = 1; a < total; a++)
            {
                workers.Add(new MyWorker());
            }
        }

        public void ConsumeQ()
        {
            foreach (ISimpleWorker worker in workers)
            {
                var t = new Thread(worker.Invoke);
                t.Start();
            }
        }
        public void RemoveQ(ISimpleWorker q)
        {
            workers.Remove(q);
        }
        public int TotalQ => workers.Count;
    }
}