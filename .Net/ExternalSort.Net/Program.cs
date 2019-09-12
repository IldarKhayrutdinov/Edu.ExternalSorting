using System;
using System.IO;

namespace ExternalSort.Net
{
    class Program
    {
        static void Main(string[] args)
        {
            ParseArgs(args, out string srcFilePath, out bool regenerate, out bool debugMode, out int nWayValue);

            Console.WriteLine($"Start sorter, CLR version = {Environment.Version}, Date = { DateTime.Now}");
            Console.WriteLine($"FilePath = {srcFilePath}, size = {new FileInfo(srcFilePath).Length}");
            Console.WriteLine($"Arguments: n = {nWayValue}, regenerate = {regenerate}, debug = {debugMode}\n");

            if (regenerate)
            {
                new FileProvider().Generate(srcFilePath);
            }

            string outFilePath = srcFilePath + ".out.txt";
            File.Copy(srcFilePath, outFilePath, true);

            //// NaturalMergeSorter.Sort(outFilePath, Consts.Comparator);
            var sortedFilePath = new NWayMergeSorter(nWayValue).Sort(srcFilePath);
            File.Replace(sortedFilePath, outFilePath, null);

            Console.WriteLine("\nVerify = " + Verifier.Verify(outFilePath));

            if (debugMode)
            {
                bool res = Verifier.ReferenceCompare(outFilePath, srcFilePath);
                Console.WriteLine("ReferenceCompare = " + res);
            }

            Console.WriteLine();
        }

        private static void ParseArgs(string[] args, out string srcFilePath, out bool regenerate, out bool debugMode, out int nWayValue)
        {
            if (args == null || args.Length < 1)
            {
                throw new ArgumentException();
            }

            srcFilePath = args[0];
            if (!File.Exists(srcFilePath))
            {
                throw new ArgumentOutOfRangeException();
            }

            debugMode = false;
            regenerate = false;
            nWayValue = Environment.ProcessorCount;
            foreach (var arg in args)
            {
                var s = arg.Split('=');
                string key = s[0];
                switch (key)
                {
                    case "--regenerate":
                        regenerate = true;
                        break;
                    case "--debug":
                        debugMode = true;
                        break;
                    case "--n":
                        if (!int.TryParse(s[1], out nWayValue))
                        {
                            nWayValue = Environment.ProcessorCount;
                        }

                        break;
                }
            }
        }

    }
}
