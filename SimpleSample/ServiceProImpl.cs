using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSample
{
    internal class ServiceProImpl : AbstractServicePro
    {
        public override string DoAnother()
        {
            return "DoAnother using ServicePro";
        }

        public new string DoService()
        {
            return "ServiceProImpl Implementation Execution";
        }
    }
}
