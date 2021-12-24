using System;
using System.Linq;
using System.Text;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;

namespace Agile.NET_Deobfuscator_Updated.Modules
{
    public class Strings : IModule
    {
        private byte[] data;
        private TypeDefinition toRemove;

        public override string ModuleName()
        {
            return "String Decryption";
        }

        public override void Process(Context context)
        {
            foreach (var module in context.Assembly.Modules)
            {
                FindData(module);
                foreach (var type in module.GetAllTypes())
                foreach (var method in type.Methods.Where(x => x.HasMethodBody))
                    for (var i = 0; i < method.CilMethodBody.Instructions.Count; i++)
                        if (method.CilMethodBody.Instructions[i].OpCode == CilOpCodes.Ldstr &&
                            method.CilMethodBody.Instructions[i + 1].OpCode == CilOpCodes.Call &&
                            method.CilMethodBody.Instructions[i + 1].Operand is MethodDefinition decryptionMethod)
                        {
                            var operand = (string) method.CilMethodBody.Instructions[i].Operand;
                            var decrypted = DecryptStringAgile6_0_0_12(operand);
                            if (decrypted != null)
                            {
                                method.CilMethodBody.Instructions[i].OpCode = CilOpCodes.Nop;
                                method.CilMethodBody.Instructions[i + 1].OpCode = CilOpCodes.Ldstr;
                                method.CilMethodBody.Instructions[i + 1].Operand = decrypted;
                                context.Resolved.Strings++;
                            }
                        }

                module.TopLevelTypes.Remove(toRemove);
                module.TopLevelTypes.Remove(module.GetAllTypes().Single(x => x.Name == "<AgileDotNetRT>"));
            }
        }

        private void FindData(ModuleDefinition def)
        {
            foreach (var type in def.GetAllTypes().Where(x => x.IsSealed && x.IsNotPublic && x.NestedTypes.Count > 0 && x.Fields.Count > 0))
            {
                var ns = type.NestedTypes.First();
                if (ns.IsSealed && ns.IsValueType &&
                    type.Fields.First().Signature.FieldType.ToString().Contains(ns.Name))
                {
                    toRemove = type;
                    var dataField = type.Fields.First();
                    if (dataField != null)
                    {
                        var reader = ((DataSegment) dataField.FieldRva).CreateReader();
                        data = new byte[reader.Length];
                        reader.ReadBytes(data, 0, data.Length);
                    }
                }
            }
        }

        private string DecryptStringAgile6_0_0_12(string A_0)
        {
            StringBuilder stringBuilder = new();
            for (var i = 0; i < A_0.Length; i++)
                stringBuilder.Append(Convert.ToChar(A_0[i] ^ (char) data[i % data.Length]));
            return stringBuilder.ToString();
        }
    }
}