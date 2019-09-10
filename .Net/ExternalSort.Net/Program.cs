using System;
using System.IO;
using System.Text;

namespace ExternalSort.Net
{
    class Program
    {
        private const int TotalLinesCount = 10;

        private const int MaxLineLength = 500;

        private const int ComparingSymbols = 50;

        private static readonly Encoding DefaultEncoding = CodePagesEncodingProvider.Instance.GetEncoding(1251); /* Encoding.GetEncoding("windows-1251"); */

        static void Main(string[] args)
        {
            ParseArgs(args, out string srcFilePath, out bool regenerate, out bool referenceCompare);

            Console.WriteLine("FilePath = " + srcFilePath);
            Console.WriteLine($"Args: regenerate={regenerate}, referenceCompare={referenceCompare}");

            if (regenerate)
            {
                new FileProvider().Generate(srcFilePath, DefaultEncoding, TotalLinesCount, MaxLineLength);
            }

            string outFilePath = srcFilePath + ".out.txt";
            File.Copy(srcFilePath, outFilePath, true);

            ExternalSorter.Sort(outFilePath, DefaultEncoding, Comparator);
            Verifier.Verify(outFilePath, DefaultEncoding, Comparator);

            Console.WriteLine("Complete");

            if (referenceCompare)
            {
                bool res = Verifier.ReferenceCompare(outFilePath, srcFilePath, DefaultEncoding, Comparator);
                Console.WriteLine("ReferenceCompare=" + res);
                //// File.Delete(outFilePath);
            }
        }

        private static void ParseArgs(string[] args, out string srcFilePath, out bool regenerate, out bool referenceCompare)
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

            referenceCompare = false;
            regenerate = false;
            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "--regenerate":
                        regenerate = true;
                        break;
                    case "--reference_compare":
                        referenceCompare = true;
                        break;
                }
            }
        }

        private static int Comparator(string line1, string line2)
        {
            return string.Compare(line1, 0, line2, 0, ComparingSymbols);
        }
    }
}
