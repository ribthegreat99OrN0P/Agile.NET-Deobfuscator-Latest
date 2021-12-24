using System.Linq;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Agile.NET_Deobfuscator_Updated
{
    public static class Extensions
    {
        /// <summary>
        ///     Redirects all branches that point to nop instructions, to real instructions
        /// </summary>
        /// <param name="body"></param>
        public static void RedirectBranches(this CilMethodBody body)
        {
            for (var i = 0; i < body.Instructions.Count; i++)
                if (body.Instructions[i].IsBranch() &&
                    body.Instructions[i].OpCode != CilOpCodes.Switch)
                {
                    var targetLabel = body.Instructions[i].Operand as ICilLabel;
                    if (targetLabel != null)
                    {
                        var targetInstruction = body.Instructions.GetByOffset(targetLabel.Offset);
                        if (targetInstruction.OpCode == CilOpCodes.Nop)
                        {
                            var targetInstructionIndex = body.Instructions.IndexOf(targetInstruction);
                            for (var j = targetInstructionIndex; j < body.Instructions.Count; j++)
                                if (body.Instructions[j].OpCode != CilOpCodes.Nop)
                                {
                                    var newLabel = body.Instructions[j].CreateLabel();
                                    body.Instructions[i].Operand = newLabel;
                                    break;
                                }
                        }
                    }
                }
        }

        /// <summary>
        ///     Removes all useless nop instructions (please call FixBranches before using)
        /// </summary>
        /// <param name="body"></param>
        public static void ClearUselessNops(this CilMethodBody body)
        {
            for (var i = 0; i < body.Instructions.Count; i++)
                if (body.Instructions[i].OpCode == CilOpCodes.Nop)
                    body.Instructions.Remove(body.Instructions[i]);
            body.Instructions.CalculateOffsets();
            body.VerifyLabels();
        }

        /// <summary>
        ///     Checks to see if the provided body contains any instructions that contain the opcode provided
        /// </summary>
        /// <param name="body"></param>
        /// <param name="opcode"></param>
        /// <returns></returns>
        public static bool ContainsCertain(this CilMethodBody body, CilOpCode opcode, out CilInstruction instruction)
        {
            foreach (var il in body.Instructions)
                if (il.OpCode == opcode)
                {
                    instruction = il;
                    return true;
                }

            instruction = null;
            return false;
        }
    }
}