using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSample
{
    internal abstract class AbstractServicePro : IServicePro
    {
        public abstract string DoAnother();

        public string DoService()
        {
            return "Default Implementation Execution";
        }

        public string GetService()
        {
            return "Default Service";
        }
    }
}
