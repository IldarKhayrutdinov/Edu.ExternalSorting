using System;
using System.IO;

namespace ExternalSort.Net
{
    class Program
    {
        static void Main(string[] args)
        {
            ParseArgs(args, out string srcFilePath, out bool regenerate, out bool debugMode, out int nWayValue, out Config config);

            Console.WriteLine($"External sorter {DateTime.Now}");
            ////Console.WriteLine($"CLR version = {Environment.Version}");
            Console.WriteLine($"Arguments: buffer size = {config.BufferSize}, N = {nWayValue}, regenerate = {regenerate}, debug = {debugMode}");
            
            if (regenerate)
            {
                new FileProvider().Generate(srcFilePath, config);
            }

            Console.WriteLine($"FilePath = {srcFilePath}, size = {new FileInfo(srcFilePath).Length} \n");
            
            string outFilePath = srcFilePath + ".out.txt";
            File.Copy(srcFilePath, outFilePath, true);

            //// NaturalMergeSorter.Sort(outFilePath, Consts.Comparator);
            var sortedFilePath = new NWayMergeSorter(nWayValue, config).Sort(srcFilePath);
            File.Replace(sortedFilePath, outFilePath, null);

            Console.WriteLine("\nVerify = " + Verifier.Verify(outFilePath));

            if (debugMode)
            {
                bool res = Verifier.ReferenceCompare(outFilePath, srcFilePath);
                Console.WriteLine("ReferenceCompare = " + res);
            }

            Console.WriteLine();
        }

        private static void ParseArgs(string[] args, out string srcFilePath, out bool regenerate, out bool debugMode, out int nWayValue, out Config config)
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
            config = new Config();
            foreach (var arg in args)
            {
                var s = arg.Split('=');
                string key = s[0];
                switch (key)
                {
                    case "--regenerate":
                        regenerate = bool.TryParse(s[1], out bool b) ? b : true;

                        break;
                    case "--generate_file_size":
                        if (long.TryParse(s[1], out long fileSize))
                        {
                            config.FileSize = fileSize;
                        }

                        break;
                    case "--debug":
                        debugMode  = bool.TryParse(s[1], out bool d) ? d : true;

                        break;
                    case "--n":
                        if (int.TryParse(s[1], out int n))
                        {
                            const int MaxAllowWays = 20;
                            n = Math.Max(n, 2);
                            n = Math.Min(n, MaxAllowWays);

                            nWayValue = n;
                        }

                        break;
                    case "--buffer_size":
                        if (int.TryParse(s[1], out int size))
                        {
                            config.BufferSize = size;
                        }

                        break;
                }
            }
        }

    }
}
