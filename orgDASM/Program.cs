using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace orgDASM
{
    public partial class Disassembler
    {
        public static void Main(string[] args)
        {
            DisplaySplash();
            Disassembler disassembler = new Disassembler();
            bool quick = false;
            string inputFile = null;
            string outputFile = null;
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith("-"))
                {
                    switch (arg)
                    {
                        case "-h":
                        case "/h":
                        case "-?":
                        case "/?":
                        case "-help":
                        case "--help":
                            DisplayHelp();
                            return;
                        case "--quick":
                        case "-q":
                            quick = true;
                            break;
                        case "--output-file":

                            break;
                        default:
                            Console.WriteLine("Invalid parameter.  Use orgDASM --help for help.");
                            return;
                    }
                }
                else
                {
                    if (inputFile == null)
                        inputFile = arg;
                    else if (outputFile == null)
                        outputFile = arg;
                    else
                    {
                        Console.WriteLine("Invalid parameter.  Use orgDASM --help for help.");
                        return;
                    }
                }
            }
            // TODO: Validation
            Stream inputStream = File.OpenRead(inputFile);
            ushort[] data = new ushort[inputStream.Length / 2];
            for (int i = 0; i < inputStream.Length; i++)
            {
                if (i % 2 == 0)
                    data[i / 2] |= (ushort)(inputStream.ReadByte());
                else
                    data[i / 2] |= (ushort)(inputStream.ReadByte() << 8);
            }
            List<CodeEntry> output = disassembler.FastDisassemble(data); // TODO: more stuff
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                foreach (var entry in output)
                {
                    writer.WriteLine(entry.Code);
                }
            }
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("TODO");
        }

        private static void DisplaySplash()
        {
            Console.WriteLine(".orgDASM DCPU-16 Disassembler    Copyright Drew DeVault 2012");
        }
    }
}
