using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile.NET_Deobfuscator_Updated
{
    public abstract class IModule
    {
        public abstract string ModuleName();
        public abstract void Process(Context context);
    }
}
