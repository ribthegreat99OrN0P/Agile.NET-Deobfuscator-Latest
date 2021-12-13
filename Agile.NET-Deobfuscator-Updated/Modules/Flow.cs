using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile.NET_Deobfuscator_Updated.Modules
{
    public class Flow : IModule
    {
        public override string ModuleName() => "ControlFlow";

        public override void Process(Context context)
        {
            // TO-DO* implement emulation to solve flow
            throw new NotImplementedException();
        }
    }
}
