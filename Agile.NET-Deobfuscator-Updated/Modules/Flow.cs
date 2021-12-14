using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Echo.ControlFlow;
using Echo.Platforms.AsmResolver;
using Echo.ControlFlow.Blocks;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Serialization.Blocks;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Agile.NET_Deobfuscator_Updated.Modules
{
    public class Flow : IModule
    {
        public override string ModuleName() => "ControlFlow";

        public override void Process(Context context)
        {
            // TO-DO* implement emulation to solve flow
            SolveMath(context);
            //CleanNops(context);
            CleanFlow(context);
        }
        private void SolveMath(Context context)
        {
            foreach(var module in context.Assembly.Modules)
            {
                foreach(var type in module.GetAllTypes())
                {
                    foreach(var method in type.Methods.Where(x => x.HasMethodBody))
                    {
                        for(var i = 0; i < method.CilMethodBody.Instructions.Count; i++)
                        {
                            if(method.CilMethodBody.Instructions[i].IsLdcI4() && method.CilMethodBody.Instructions[i+1].OpCode == CilOpCodes.Call && method.CilMethodBody.Instructions[i + 1].Operand.ToString().Contains("Math::Abs"))
                            {
                                var value = method.CilMethodBody.Instructions[i].GetLdcI4Constant();
                                var solved = Math.Abs(value);
                                method.CilMethodBody.Instructions[i].OpCode = CilOpCodes.Nop;
                                method.CilMethodBody.Instructions[i + 1].OpCode = CilOpCodes.Ldc_I4;
                                method.CilMethodBody.Instructions[i + 1].Operand = solved;
                                context.Resolved.AbsSolved++;
                            }
                        }
                    }
                }
            }
        }
        private void CleanNops(Context context)
        {
            foreach (var module in context.Assembly.Modules)
            {
                foreach (var type in module.GetAllTypes())
                {
                    foreach (var method in type.Methods.Where(x => x.HasMethodBody))
                    {
                        for (var i = 0; i < method.CilMethodBody.Instructions.Count; i++)
                        {
                            if (method.CilMethodBody.Instructions[i].OpCode == CilOpCodes.Nop)
                            {
                                method.CilMethodBody.Instructions.Remove(method.CilMethodBody.Instructions[i]);
                            }
                        }
                        
                    }
                }
            }
        }
        private void CleanFlow(Context context)
        {
            foreach (var module in context.Assembly.Modules)
            {
                foreach (var type in module.GetAllTypes())
                {
                    foreach (var method in type.Methods.Where(x => x.HasMethodBody && ContainsSwitch(x)))
                    {
                        method.CilMethodBody.VerifyLabels();
                        var switchCode = method.CilMethodBody.Instructions.Single(x => x.OpCode == CilOpCodes.Switch);
                        var labels = switchCode.Operand as List<ICilLabel>;
                        var emulator = new CilVirtualMachine(method.CilMethodBody, true);
                        emulator.CurrentState.Stack.Push(new I4Value(4));
                        var res = emulator.Dispatcher.Execute(null, switchCode);
                        if (res.IsSuccess)
                        {
                            Console.WriteLine("success");
                         
                        }
                        var graph = method.CilMethodBody.ConstructStaticFlowGraph();
                        var blocks = graph.ConstructBlocks();
                        var allBlocks = blocks.GetAllBlocks().ToList();
                        for(var i = 0; i < allBlocks.Count(); i++)
                        {
                            Console.WriteLine(i);
                            foreach (var ins in allBlocks[i].Instructions)
                                Console.WriteLine(ins);
                            Console.WriteLine("---------------------");
                        }
                      


                        Console.WriteLine("-----------------------------------");
                        break;
                    }
                }
            }
        }
        private bool ContainsSwitch(MethodDefinition method)
        {
            foreach(var il in method.CilMethodBody.Instructions)
            {
                if (il.OpCode == CilOpCodes.Switch) return true;
            }
            return false;
        }
    }
}
