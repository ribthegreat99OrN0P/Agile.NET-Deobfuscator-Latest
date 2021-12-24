// See https://aka.ms/new-console-template for more information

using System;
using System.IO;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Builder;
using AsmResolver.DotNet.Code.Cil;
using System.Reflection.PortableExecutable;

namespace Agile.NET_Deobfuscator_Updated
{
    class Program
    {
        static void Main(string[] file)
        {
            Context ctx;//= new Context() { Assembly = AssemblyDefinition.FromFile("UnPackMeConsoleProt.exe") };
                
            
            if(file != null)
            {
                ctx = new Context() { Assembly = AssemblyDefinition.FromFile(file[0]) };
            }
            else
            {
                Console.Write("File >");
                var read = Console.ReadLine();
                if(read != null)
                {
                    ctx = new Context() { Assembly = AssemblyDefinition.FromFile(read) };
                }
                else
                {
                    return;
                }
            }
            
            var runtimeVersion = ctx.Assembly.ManifestModule?.OriginalTargetRuntime.Version;
            Console.WriteLine($"resolving assemblies from framework version : {runtimeVersion}");
            var resolver = new DotNetCoreAssemblyResolver(runtimeVersion);
            foreach(var reference in ctx.Assembly.ManifestModule.AssemblyReferences)
            {
                resolver.Resolve(reference);
            }
            
            foreach (var module in new IModule[] { new Modules.Calls(), new Modules.Strings(),new Modules.Flow() , /*new Modules.Resources(),*/})//, new Modules.Flow()
            {
                Console.WriteLine($"started {module.ModuleName()}");
                module.Process(ctx);
            }
                

            var imageBuilder = new ManagedPEImageBuilder();
            var factory = new DotNetDirectoryFactory(MetadataBuilderFlags.PreserveAll);
            imageBuilder.DotNetDirectoryFactory = factory;
            factory.MethodBodySerializer = new CilMethodBodySerializer()
            {
                ComputeMaxStackOnBuildOverride = false
            };
            ctx.Assembly.ManifestModule.Write(Path.GetFileNameWithoutExtension(ctx.Path) + "-deob" +(ctx.Assembly.ManifestModule.IsILLibrary ? ".dll" : ".exe"), imageBuilder);
            Console.WriteLine(
                $" Summary: \n Resolved {ctx.Resolved.Delegates} calls \n Decrypted {ctx.Resolved.Strings} strings \n Solved {ctx.Resolved.AbsSolved} arithmetic calls \n");
            
            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}
