using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Laba1_cs
{
    class Program
    {
        public struct GeneticData
        {
            public string protein;
            public string organism;
            public string amino_acids;
        }

        struct Command
        {
            public string name;
            public string parameter1;
            public string parameter2;
        }

        static void Main(string[] args)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string seqFile = Path.Combine(baseDir, "sequences.txt");
            string cmdFile = Path.Combine(baseDir, "commands.txt");
            string outFile = Path.Combine(baseDir, "genedata.txt");

            List<GeneticData> database = ReadData(seqFile);
            List<Command> commands = ReadCommands(cmdFile);

            if (database.Count == 0 || commands.Count == 0)
            {
                Console.WriteLine("Ошибка: Убедитесь, что файлы sequences.txt и commands.txt лежат по пути:");
                Console.WriteLine(baseDir);
                return;
            }

            CommandHandler(database, commands, outFile);
            Console.WriteLine("Программа успешно выполнена!");
            Console.WriteLine($"Результаты автоматически сохранены в файл:\n{outFile}");
        }

        static List<Command> ReadCommands(string filename)
        {
            List<Command> commands = new List<Command>();
            if (!File.Exists(filename))
            {
                return commands;
            }

            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string[] parts = line.Split('\t');
                    Command command;
                    command.name = String.Empty;
                    command.parameter1 = String.Empty;
                    command.parameter2 = String.Empty;

                    if (parts.Length == 2)
                    {
                        command.name = parts[0].Trim();
                        command.parameter1 = parts[1].Trim();
                    }
                    else if (parts.Length >= 3)
                    {
                        command.name = parts[0].Trim();
                        command.parameter1 = parts[1].Trim();
                        command.parameter2 = parts[2].Trim();
                    }

                    commands.Add(command);
                }
            }
            return commands;
        }

        static List<GeneticData> ReadData(string filename)
        {
            List<GeneticData> data = new List<GeneticData>();
            if (!File.Exists(filename))
            {
                return data;
            }

            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string[] parts = line.Split('\t');
                    if (parts.Length >= 3)
                    {
                        GeneticData protein;
                        protein.protein = parts[0].Trim();
                        protein.organism = parts[1].Trim();
                        protein.amino_acids = RLDecoding(parts[2].Trim());
                        data.Add(protein);
                    }
                }
            }
            return data;
        }

        static string RLDecoding(string amino_acids)
        {
            if (string.IsNullOrEmpty(amino_acids))
            {
                return "";
            }

            StringBuilder decoded = new StringBuilder();
            int count = 0;

            for (int i = 0; i < amino_acids.Length; i++)
            {
                char ch = amino_acids[i];
                if (char.IsDigit(ch))
                {
                    count = count * 10 + (ch - '0');
                }
                else
                {
                    int repeat = 1;
                    if (count != 0)
                    {
                        repeat = count;
                    }
                    decoded.Append(ch, repeat);
                    count = 0;
                }
            }
            return decoded.ToString();
        }

        static string RLEncoding(string amino_acids)
        {
            if (string.IsNullOrEmpty(amino_acids))
            {
                return "";
            }

            StringBuilder encoded = new StringBuilder();
            int count = 1;

            for (int i = 1; i <= amino_acids.Length; i++)
            {
                if (i < amino_acids.Length && amino_acids[i] == amino_acids[i - 1])
                {
                    count++;
                }
                else
                {
                    if (count > 2)
                    {
                        encoded.Append(count);
                    }
                    else if (count == 2)
                    {
                        encoded.Append(amino_acids[i - 1]);
                    }
                    encoded.Append(amino_acids[i - 1]);
                    count = 1;
                }
            }
            return encoded.ToString();
        }

        static void CommandHandler(List<GeneticData> proteins, List<Command> commands, string outputFilename)
        {
            using (StreamWriter writer = new StreamWriter(outputFilename, false, Encoding.UTF8))
            {
                writer.WriteLine("Dwight Barnette");
                writer.WriteLine("Genetic Searching");
                writer.WriteLine(new string('-', 72));

                for (int i = 0; i < commands.Count; i++)
                {
                    int opIndex = i + 1;
                    Command cmd = commands[i];

                    if (cmd.name == "search")
                    {
                        ExecuteSearch(proteins, cmd.parameter1, opIndex, writer);
                    }
                    else if (cmd.name == "diff")
                    {
                        ExecuteDiff(proteins, cmd.parameter1, cmd.parameter2, opIndex, writer);
                    }
                    else if (cmd.name == "mode")
                    {
                        ExecuteMode(proteins, cmd.parameter1, opIndex, writer);
                    }
                }
            }
        }

        static void ExecuteSearch(List<GeneticData> database, string query, int opIndex, StreamWriter writer)
        {
            writer.WriteLine($"{opIndex:D3}  search  {query}");
            writer.WriteLine("organism                        protein");

            string decodedQuery = RLDecoding(query);
            bool found = false;

            foreach (var data in database)
            {
                if (data.amino_acids.Contains(decodedQuery))
                {
                    writer.WriteLine($"{data.organism,-32}{data.protein}");
                    found = true;
                }
            }

            if (!found)
            {
                writer.WriteLine("NOT FOUND");
            }
            writer.WriteLine(new string('-', 72));
        }

        static void ExecuteDiff(List<GeneticData> database, string protein1, string protein2, int opIndex, StreamWriter writer)
        {
            writer.WriteLine($"{opIndex:D3}  diff  {protein1}  {protein2}");
            writer.WriteLine("amino-acids difference:");

            GeneticData? p1 = database.Find(x => x.protein == protein1);
            GeneticData? p2 = database.Find(x => x.protein == protein2);

            if (p1 == null || p2 == null)
            {
                writer.Write("MISSING: ");
                List<string> missing = new List<string>();

                if (p1 == null) missing.Add(protein1);
                if (p2 == null) missing.Add(protein2);

                writer.WriteLine(string.Join(" ", missing));
                writer.WriteLine(new string('-', 72));
                return;
            }

            string s1 = p1.Value.amino_acids;
            string s2 = p2.Value.amino_acids;

            int minLen = Math.Min(s1.Length, s2.Length);
            int diffCount = Math.Abs(s1.Length - s2.Length);

            for (int i = 0; i < minLen; i++)
            {
                if (s1[i] != s2[i])
                {
                    diffCount++;
                }
            }

            writer.WriteLine(diffCount);
            writer.WriteLine(new string('-', 72));
        }

        static void ExecuteMode(List<GeneticData> database, string proteinName, int opIndex, StreamWriter writer)
        {
            writer.WriteLine($"{opIndex:D3}  mode  {proteinName}");
            writer.WriteLine("amino-acid occurs:");

            GeneticData? target = database.Find(x => x.protein == proteinName);
            if (target == null)
            {
                writer.WriteLine($"MISSING: {proteinName}");
                writer.WriteLine(new string('-', 72));
                return;
            }

            string seq = target.Value.amino_acids;
            int[] counts = new int[256];

            foreach (char c in seq)
            {
                counts[c]++;
            }

            char maxChar = 'A';
            int maxCount = -1;

            char[] aminoAlphabet = { 'A', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'Y' };

            foreach (char c in aminoAlphabet)
            {
                if (counts[c] > maxCount)
                {
                    maxCount = counts[c];
                    maxChar = c;
                }
            }
            writer.WriteLine(maxChar);
            writer.WriteLine(maxCount);
            writer.WriteLine(new string('-', 72));
        }
    }
}