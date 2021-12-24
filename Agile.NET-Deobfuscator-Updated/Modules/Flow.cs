using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Echo.ControlFlow.Blocks;
using Echo.ControlFlow.Serialization.Blocks;
using Echo.Platforms.AsmResolver;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Agile.NET_Deobfuscator_Updated.Modules
{
    public class Flow : IModule
    {
        public override string ModuleName()
        {
            return "ControlFlow";
        }

        public override void Process(Context context)
        {
            // TO-DO* implement emulation to solve flow
            SolveMath(context);
            //CleanFlow(context);
        }

        private void SolveMath(Context context)
        {
            foreach (var module in context.Assembly.Modules)
            foreach (var type in module.GetAllTypes())
            foreach (var method in type.Methods.Where(x => x.HasMethodBody))
                for (var i = 0; i < method.CilMethodBody.Instructions.Count; i++)
                    if (method.CilMethodBody.Instructions[i].IsLdcI4() &&
                        method.CilMethodBody.Instructions[i + 1].OpCode == CilOpCodes.Call && method.CilMethodBody
                            .Instructions[i + 1].Operand.ToString().Contains("Math::Abs"))
                    {
                        var value = method.CilMethodBody.Instructions[i].GetLdcI4Constant();
                        var solved = Math.Abs(value);
                        method.CilMethodBody.Instructions[i].OpCode = CilOpCodes.Nop;
                        method.CilMethodBody.Instructions[i + 1].OpCode = CilOpCodes.Ldc_I4;
                        method.CilMethodBody.Instructions[i + 1].Operand = solved;
                        context.Resolved.AbsSolved++;
                    }
        }


        private void CleanFlow(Context context)
        {
            var method = context.Assembly.ManifestModule?.ManagedEntrypointMethod;
            method?.CilMethodBody?.RedirectBranches();
            method?.CilMethodBody?.ClearUselessNops();

            var vm = new CilVirtualMachine(method?.CilMethodBody, true);
            var executionContext = new CilExecutionContext(vm, vm.CurrentState, CancellationToken.None);
            var resolvedBody = new List<CilInstruction>();

            if (method.CilMethodBody.ContainsCertain(CilOpCodes.Switch, out var targetSwitch))
            {
                var switchTargets = targetSwitch.Operand as List<ICilLabel>;
                Console.WriteLine($"loops to go through {switchTargets.Count}");


                var targetLocal =
                    method.CilMethodBody.Instructions[method.CilMethodBody.Instructions.IndexOf(targetSwitch) - 1]
                        .GetLocalVariable(method.CilMethodBody.LocalVariables); //ldloc of main local
                var allBlocks = method.CilMethodBody.ConstructStaticFlowGraph().ConstructBlocks().Blocks;
                IBlockVisitor<CilInstruction> visitor = null;
                foreach (var b in allBlocks)
                {
                    var e = b.GetAllBlocks().ToList();
                    for (var i = 0; i < e.Count(); i++)
                    {
                        Console.WriteLine(i);
                        foreach (var ins in e[i].Instructions)
                            Console.WriteLine(ins);
                        Console.WriteLine("---------------------");

                    }

                    
                }
                foreach (var b in allBlocks)
                {
                    var e = b.GetAllBlocks().ToList();
                    var dict = new Dictionary<int, CilInstruction[]>();
                    sort(method.CilMethodBody, e, dict);
                }

                /*
                var allBlocks = method.CilMethodBody.ConstructStaticFlowGraph().ConstructBlocks().GetAllBlocks()
                    .ToList();
                for (var i = 0; i < allBlocks.Count(); i++)
                {
                    Console.WriteLine(i);
                    foreach (var ins in allBlocks[i].Instructions)
                        Console.WriteLine(ins);
                    Console.WriteLine("---------------------");
                }
                var setterValues = new List<int>();
                GetSetters(method.CilMethodBody, targetLocal, setterValues, switchTargets.Count);
                setterValues.Reverse();
                for (var i = 0; i < setterValues.Count; i++)
                {
                    vm.CurrentState.Stack.Clear();
                    vm.CurrentState.Stack.Push(new I4Value(setterValues[i]));
                    var emulationResult = vm.Dispatcher.Execute(executionContext, targetSwitch);
                    if (emulationResult.IsSuccess)
                    {
                        var jumpedBlock = GetInstructionBlockParent(allBlocks,
                            vm.Instructions.GetInstructionAtOffset(vm.CurrentState.ProgramCounter));
                        if (jumpedBlock != null)
                        {
                            var resolved = GetResolvedInstructions(jumpedBlock, targetLocal);
                            if (resolved.Count > 0)
                            {
                                resolvedBody.AddRange(resolved);
                              
                            }
                        }
                    }
                }
              */
            }
            
            foreach (var ins in resolvedBody)
            {
                Console.WriteLine(ins);
            }
            //method.CilMethodBody.Instructions.Clear();
            //method.CilMethodBody.Instructions.AddRange(resolvedBody);
        }

        private void sort(CilMethodBody body, List<BasicBlock<CilInstruction>> blocks,
            Dictionary<int, CilInstruction[]> dict)
        {
            for (var i = 0; i < blocks.Count; i++)
            {
                for (var j = 0; j < blocks[i].Instructions.Count; j++)
                {
                    //if()
                }
            }
        }
        private void GetSetters(CilMethodBody body, CilLocalVariable mainlocal, List<int> values, int max)
        {
            for (var i = 0; i < body.Instructions.Count; i++)
            {
                if (body.Instructions[i].IsLdcI4() && body.Instructions[i + 1].IsStloc() &&
                    body.Instructions[i + 1].Operand == mainlocal)
                {
                    var v = body.Instructions[i].GetLdcI4Constant();
                    if (v < max)
                    {
                        values.Add(v);
                    }
                }
            }
        }
        private BasicBlock<CilInstruction> GetInstructionBlockParent(List<BasicBlock<CilInstruction>> blocks,
            CilInstruction instruction)
        {
            foreach (var block in blocks)
                if (block.Instructions.Contains(instruction))
                    return block;
            return null;
        }

        private List<CilInstruction> GetResolvedInstructions(BasicBlock<CilInstruction> block, CilLocalVariable mainlocal)
        {
            var instructions = new List<CilInstruction>();
            for (var i = 0; i < block.Instructions.Count; i++)
            {
                if (block.Instructions[i].IsLdcI4() && block.Instructions[i + 1].IsStloc() &&
                    block.Instructions[i + 1].Operand == mainlocal) break;
                instructions.Add(block.Instructions[i]);
            }

            return instructions;
        }

        private BasicBlock<CilInstruction> GetFirstSetterBlock(List<BasicBlock<CilInstruction>> allBlocks,
            CilLocalVariable mainLocal)
        {
            foreach (var block in allBlocks)
                for (var i = 0; i < block.Instructions.Count; i++)
                    if (block.Instructions[i].IsLdcI4() && block.Instructions[i + 1].IsStloc() &&
                        block.Instructions[i + 1].Operand == mainLocal)
                        return block;

            return null;
        }
    }
}