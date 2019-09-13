using System;
using System.IO;

namespace ExternalSort.Net
{
    internal static class Verifier
    {
        public static bool Verify(string filePath)
        {
            using (var fs = new StreamReader(filePath, Config.Encoding))
            {
                string last = fs.ReadLine();
                while (!fs.EndOfStream)
                {
                    string line = fs.ReadLine();
                    if (Config.Comparator(last, line) > 0)
                    {
                        return false;
                    }

                    last = line;
                }
            }

            return true;
        }

        public static bool ReferenceCompare(string filePath, string srcPath)
        {
            string[] lines = File.ReadAllLines(filePath, Config.Encoding);
            string[] srcLines = File.ReadAllLines(srcPath, Config.Encoding);

            Array.Sort(srcLines, Config.Comparator);

            return Compare(lines, srcLines);
        }

        private static bool Compare<T>(T[] lines, T[] srcLines)
            where T : IEquatable<T>
        {
            if (lines.Length != srcLines.Length)
            {
                return false;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].Equals(srcLines[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
