using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Globalization;

namespace orgDASM
{
    public partial class Disassembler
    {
        #region Constructor and runtime values

        private int CodeNumber, DataNumber;

        private Dictionary<string, byte> OpcodeTable;
        private Dictionary<string, byte> NonBasicOpcodeTable;
        private Dictionary<string, byte> ValueTable;

        public Dictionary<string, ushort> DataLabels;
        public Dictionary<string, ushort> CodeLabels;

        public Disassembler()
        {
            OpcodeTable = new Dictionary<string, byte>();
            NonBasicOpcodeTable = new Dictionary<string, byte>();
            ValueTable = new Dictionary<string, byte>();
            CodeNumber = 0;
            DataNumber = 0;
            DataLabels = new Dictionary<string, ushort>();
            CodeLabels = new Dictionary<string, ushort>();
            LoadTable();
        }

        private void LoadTable()
        {
            StreamReader sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("orgDASM.DCPUtable.txt"));
            string[] lines = sr.ReadToEnd().Replace("\r", "").Split('\n');
            sr.Close();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;
                string[] parts = line.Split(' ');
                string contents = line.Substring(line.IndexOf(' ') + 1);
                contents = contents.Substring(contents.IndexOf(' ') + 1);
                if (parts[0] == "o")
                    OpcodeTable.Add(contents, byte.Parse(parts[1], NumberStyles.HexNumber));
                else if (parts[0] == "n")
                    NonBasicOpcodeTable.Add(contents, byte.Parse(parts[1], NumberStyles.HexNumber));
                else if (parts[0] == "a,b")
                    ValueTable.Add(contents, byte.Parse(parts[1], NumberStyles.HexNumber));
            }
        }

        #endregion

        /// <summary>
        /// Disassembles a small snippet of data without finding labels.
        /// </summary>
        /// <param name="Data"></param>
        public List<CodeEntry> FastDisassemble(ushort[] Data)
        {
            List<CodeEntry> output = new List<CodeEntry>();

            for (int i = 0; i < Data.Length; i++)
            {
                CodeEntry entry = new CodeEntry();
                entry.Opcode = (byte)(Data[i] & 0x1F);
                entry.ValueB = (byte)((Data[i] & 0x3E0) >> 5);
                entry.ValueA = (byte)((Data[i] & 0xFC00) >> 10);

                KeyValuePair<string, byte> opcodeMatch = new KeyValuePair<string,byte>(),
                    valueAMatch = new KeyValuePair<string,byte>(),
                    valueBMatch = new KeyValuePair<string,byte>();
                bool matchFound = false, nonBasic = false;

                if (entry.Opcode != 0)
                {
                    foreach (var code in OpcodeTable)
                    {
                        if (code.Value == entry.Opcode)
                        {
                            matchFound = true;
                            opcodeMatch = code;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var code in NonBasicOpcodeTable)
                    {
                        if (code.Value == entry.ValueB)
                        {
                            matchFound = nonBasic = true;
                            entry.Opcode = entry.ValueB;
                            opcodeMatch = code;
                            break;
                        }
                    }
                }
                foreach (var value in ValueTable)
                {
                    if (value.Value == entry.ValueA)
                        valueAMatch = value;
                    if (value.Value == entry.ValueB)
                        valueBMatch = value;
                }
                if (!matchFound)
                {
                    // Treat as data
                    entry.Code = "DAT 0x" + Data[i].ToString("x");
                }
                else
                {
                    if (valueAMatch.Key == null)
                    {
                        short signedValue = (short)(entry.ValueA - 0x21);
                        valueAMatch = new KeyValuePair<string,byte>(signedValue.ToString(), valueAMatch.Value);
                    }

                    entry.Code = opcodeMatch.Key;
                    entry.Code = entry.Code.Replace("%a", valueAMatch.Key);
                    entry.Code = entry.Code.Replace("%b", valueBMatch.Key);
                    if (entry.Code.Contains("$0"))
                    {
                        string test1 = entry.Code.Remove(entry.Code.LastIndexOf("$0"));
                        string test2 = entry.Code.Substring(entry.Code.LastIndexOf("$0") + 2);
                        entry.Code = entry.Code.Remove(entry.Code.LastIndexOf("$0"))
                            + "0x" + Data[++i].ToString("x") +
                            entry.Code.Substring(entry.Code.LastIndexOf("$0") + 2);
                        if (Data[i] < 0x1F || (short)Data[i] == -1) // TODO: Remove
                        {
                            entry.Code = "DAT 0x" + Data[--i].ToString("x");
                            output.Add(entry);
                            continue;
                        }
                    }
                    if (entry.Code.Contains("$0"))
                    {
                        entry.Code = entry.Code.Replace("$0", "0x" + Data[++i].ToString("x"));
                        if (Data[i] < 0x1F || (short)Data[i] == -1) // TODO: Remove
                        {
                            entry.Code = "DAT 0x" + Data[--i].ToString("x");
                            output.Add(entry);
                            continue;
                        }
                    }
                }
                output.Add(entry);
            }

            return output;
        }
    }
}
