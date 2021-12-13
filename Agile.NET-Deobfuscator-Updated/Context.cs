using AsmResolver.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile.NET_Deobfuscator_Updated
{
    public class Context
    {
        public AssemblyDefinition Assembly { get; init; }
        public string Path => Assembly.ManifestModule.FilePath;
        public ResolvedValues Resolved { get; init; } = new ResolvedValues();
    }
}
