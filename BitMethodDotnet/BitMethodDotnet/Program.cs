using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using System;
using System.IO;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        ModuleDefMD moduleDefMD = ModuleDefMD.Load(args[0]);
        if (moduleDefMD.HasTypes)
        {
            foreach (TypeDef type in moduleDefMD.Types)
            {
                if (type.HasMethods)
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.HasBody)
                        {
                            Random random = new Random();
                            int randomValueForInsturction = 0;
                            if (method.Body.Instructions.Count >= 3)
                            {
                                randomValueForInsturction = random.Next(0, method.Body.Instructions.Count - 3);
                            }

                            // It could not work sometimes (may will harm to your app) (be careful with that, do some checks before)
                            // e.g try to ignore async methods if you have
                            int randomValue = random.Next(0, 3);
                            Instruction randomlySelectedInstruction = new Instruction();
                            randomlySelectedInstruction.OpCode = randomValue switch
                            {
                                0 => OpCodes.Readonly,
                                1 => OpCodes.Unaligned,
                                2 => OpCodes.Volatile,
                                3 => OpCodes.Constrained,
                                _ => throw new ArgumentOutOfRangeException(),
                            };

                            // Probably bit methods in some decompilers (e.g bit methods on old dnspy versions and a bit ilspy)
                            // this bit wont work on dnspyEx and dotpeek also
                            method.Body.Instructions.Insert(randomValueForInsturction, Instruction.Create(OpCodes.Br_S, method.Body.Instructions[randomValueForInsturction]));
                            method.Body.Instructions.Insert(randomValueForInsturction + 1, randomlySelectedInstruction);
                        }
                    }
                }
            }
        }

        ModuleWriterOptions writerOptions = new ModuleWriterOptions(moduleDefMD);
        writerOptions.MetadataLogger = DummyLogger.NoThrowInstance;
        writerOptions.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack | MetadataFlags.PreserveAll;
        writerOptions.Cor20HeaderOptions.Flags = ComImageFlags.ILOnly;
        string file = new StringBuilder()
                            .Append(Path.GetFileNameWithoutExtension(args[0]))
                            .Append("_protected")
                            .Append(Path.GetExtension(args[0]))
                            .ToString();

        moduleDefMD.Write(file, writerOptions);
    }
}