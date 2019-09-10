using System;
using System.IO;
using System.Text;

namespace ExternalSort.Net
{
    internal static class Verifier
    {
        public static bool Verify(string filePath, Encoding encoding, Comparison<string> comparator)
        {
            using (var fs = new StreamReader(filePath, encoding))
            {
                string last = fs.ReadLine();
                int lines = 1;
                while (!fs.EndOfStream)
                {
                    string line = fs.ReadLine();
                    if (comparator(last, line) > 0)
                    {
                        return false;
                    }

                    lines++;
                    last = line;
                }
            }

            return true;
        }

        public static bool ReferenceCompare(string filePath, string srcPath, Encoding encoding, Comparison<string> comparator)
        {
            string[] lines = File.ReadAllLines(filePath, encoding);
            string[] srcLines = File.ReadAllLines(srcPath, encoding);

            Array.Sort(srcLines, comparator);

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
