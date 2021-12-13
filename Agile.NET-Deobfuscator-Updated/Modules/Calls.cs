using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile.NET_Deobfuscator_Updated.Modules
{
    public class Calls : IModule
    {
        private static bool flag = false;
        private static int removed = 0;
        public override string ModuleName() => "Call Resolver";

        public override void Process(Context context)
        {
            foreach(var module in context.Assembly.Modules)
            {
                foreach(var type in module.GetAllTypes())
                {
                    foreach(var method in type.Methods.Where(x => x.HasMethodBody))
                    {
                        for(var i = 0; i < method.CilMethodBody?.Instructions.Count; i++)
                        {
                            if (method.CilMethodBody.Instructions[i].OpCode == CilOpCodes.Call &&
                               method.CilMethodBody.Instructions[i].Operand.ToString().Contains("::Invoke"))
                            {

                                var op = method.CilMethodBody.Instructions[i].Operand;
                                if (op is MethodDefinition mm)
                                {
                                    var solved = gettt(mm);

                                    method.CilMethodBody.Instructions[i].Operand = solved;
                                    context.Resolved.Delegates++;
                                }
                            }
                            if (method.CilMethodBody.Instructions[i].OpCode == CilOpCodes.Ldsfld)
                            {
                                var op2 = method.CilMethodBody.Instructions[i].Operand;
                                if (op2 is FieldDefinition ff && ff.DeclaringType?.BaseType?.FullName == "System.MulticastDelegate")
                                {
                                    method.CilMethodBody.Instructions[i].OpCode = CilOpCodes.Nop;
                                }
                            }
                        }
                    }
                }
                foreach(var delegateType in module.GetAllTypes().Where(x => x.IsDelegate))
                {
                    module.TopLevelTypes.Remove(delegateType);
                }
            }
        }
        private static MemberReference gettt(MethodDefinition met)
        {
            TypeDefinition decType = met.DeclaringType;
            if (decType?.BaseType?.FullName == "System.MulticastDelegate")
            {
                foreach (var method in decType.Methods)
                {
                    if (method.HasMethodBody)
                    {
                        for (int i = 0; i < method.CilMethodBody.Instructions.Count; i++)
                        {
                            if (method.CilMethodBody.Instructions[i].IsLdcI4())
                            {

                                int value = method.CilMethodBody.Instructions[i].GetLdcI4Constant();
                                FieldDefinition fieldDele = getfieldofdelegate(decType);

                                string name = fieldDele.Name;
                                if (name.EndsWith("%"))
                                {
                                    flag = true;
                                    name = name.TrimEnd(new char[] { '%' });
                                }
                                uint num = BitConverter.ToUInt32(Convert.FromBase64String(name), 0);

                                var token = new MetadataToken((uint)((long)num + 167772161L));

                                var resolved = met.Module.LookupMember(token) as MemberReference;
                                return resolved;
                            }
                        }
                    }
                }
            }
            return null;
        }
        private static FieldDefinition getfieldofdelegate(TypeDefinition type)
        {
            foreach (var item in type.Fields)
            {
                return item;
            }
            return null;
        }
    }
}
