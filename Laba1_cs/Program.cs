using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Laba1_cs
{
    public struct GeneticData
    {
        public string protein;     // название белка
        public string organism;    // название организма
        public string amino_acids; // цепочка аминокислот
    }

    class Program
    {
        static void Main(string[] args)
        {
            string seqFile = "sequences.txt";
            string cmdFile = "commands.txt";
            string outFile = "genedata.txt";

            if (!File.Exists(seqFile) || !File.Exists(cmdFile))
            {
                Console.WriteLine("Ошибка: отсутствуют входные файлы sequences.txt или commands.txt в папке приложения!");
                return;
            }

            // 1. Чтение базы белков
            List<GeneticData> database = new List<GeneticData>();
            string[] seqLines = File.ReadAllLines(seqFile);

            foreach (var line in seqLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] parts = line.Split('\t');
                if (parts.Length >= 3)
                {
                    GeneticData data;
                    data.protein = parts[0].Trim();
                    data.organism = parts[1].Trim();
                    data.amino_acids = RLDecoding(parts[2].Trim());

                    database.Add(data);
                }
            }

            // 2. Потоковое чтение команд и запись результатов
            using (StreamReader reader = new StreamReader(cmdFile))
            using (StreamWriter writer = new StreamWriter(outFile, false, Encoding.UTF8))
            {
                writer.WriteLine("Иван Иванов"); // <-- Замените на ваши имя и фамилию
                writer.WriteLine("Genetic Searching");
                writer.WriteLine(new string('-', 72));

                string cmdLine;
                int opIndex = 1;

                while ((cmdLine = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(cmdLine))
                    {
                        continue;
                    }

                    string[] parts = cmdLine.Split('\t');
                    string command = parts[0].Trim();

                    if (command == "search" && parts.Length >= 2)
                    {
                        ExecuteSearch(database, parts[1].Trim(), opIndex, writer);
                    }
                    else if (command == "diff" && parts.Length >= 3)
                    {
                        ExecuteDiff(database, parts[1].Trim(), parts[2].Trim(), opIndex, writer);
                    }
                    else if (command == "mode" && parts.Length >= 2)
                    {
                        ExecuteMode(database, parts[1].Trim(), opIndex, writer);
                    }

                    opIndex++;
                }
            }

            Console.WriteLine("Вычисления завершены! Результаты записаны в genedata.txt");
        }

        public static string RLDecoding(string amino_acids)
        {
            if (string.IsNullOrEmpty(amino_acids))
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
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
                    sb.Append(ch, repeat);
                    count = 0;
                }
            }
            return sb.ToString();
        }

        public static string RLEncoding(string amino_acids)
        {
            if (string.IsNullOrEmpty(amino_acids))
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
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
                        sb.Append(count);
                    }
                    else if (count == 2)
                    {
                        sb.Append(amino_acids[i - 1]);
                    }
                    sb.Append(amino_acids[i - 1]);
                    count = 1;
                }
            }
            return sb.ToString();
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

            GeneticData? p1 = database.Find(x => x.protein == protein1);
            GeneticData? p2 = database.Find(x => x.protein == protein2);

            if (p1 == null || p2 == null)
            {
                writer.WriteLine("amino-acids difference:");
                writer.Write("MISSING: ");
                List<string> missing = new List<string>();

                if (p1 == null)
                {
                    missing.Add(protein1);
                }
                if (p2 == null)
                {
                    missing.Add(protein2);
                }

                writer.WriteLine(string.Join(", ", missing));
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

            writer.WriteLine("amino-acids difference:");
            writer.WriteLine(diffCount);
            writer.WriteLine(new string('-', 72));
        }

        static void ExecuteMode(List<GeneticData> database, string proteinName, int opIndex, StreamWriter writer)
        {
            writer.WriteLine($"{opIndex:D3}  mode  {proteinName}");

            GeneticData? target = database.Find(x => x.protein == proteinName);
            if (target == null)
            {
                writer.WriteLine("amino-acid occurs:");
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

            writer.WriteLine("amino-acid occurs:");
            writer.WriteLine(maxChar);
            writer.WriteLine(maxCount);
            writer.WriteLine(new string('-', 72));
        }
    }
}