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

        private static readonly Encoding DefaultEncoding = Encoding.UTF8; // Encoding.GetEncoding("windows-1251");

        static void Main(string[] args)
        {
            ParseArgs(args, out string srcFilePath, out bool binaryCompare, out bool regenerate);

            if (regenerate)
            {
                new FileProvider().Generate(srcFilePath, DefaultEncoding, TotalLinesCount, MaxLineLength);
            }

            string bkFilePath = null;
            if (binaryCompare)
            {
                bkFilePath = srcFilePath + ".bk.txt";
                File.Copy(srcFilePath, bkFilePath, true);
            }

            ExternalSorter.Sort(srcFilePath, DefaultEncoding, Comparator);
            Verifier.Verify(srcFilePath, DefaultEncoding, Comparator);

            if (binaryCompare)
            {
                Verifier.BinaryCompare(srcFilePath, bkFilePath);
                File.Delete(bkFilePath);
            }
        }

        private static void ParseArgs(string[] args, out string srcFilePath, out bool binaryCompare, out bool regenerate)
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

            binaryCompare = false;
            regenerate = false;
            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "--binary_compare":
                        binaryCompare = true;
                        break;
                    case "--regenerate":
                        regenerate = true;
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
