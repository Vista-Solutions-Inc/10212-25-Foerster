using Jobs.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foerster.Models.JobManagement
{
    public class JobPart : Part
    {
        public override string ConnectorName => throw new NotImplementedException();

        public override void ConnectModuleToBus()
        {
            throw new NotImplementedException();
        }
        public override string? Reset()
        {
            return null;
        }
        public override string? UnLoadForOperation()
        {
            return null;
        }
    }
}
