using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ExternalSort
{
    internal class FileProvider
    {
        private readonly Random random = new Random();

        public void Generate(string filePath, Encoding encoding, int totalLinesCount, int maxLineLength)
        {
            var charset = GenerateCharset().ToArray();
            using (var fs = new StreamWriter(filePath, false, encoding))
            {
                for (int i = 0; i < totalLinesCount; i++)
                {
                    int lineLength = random.Next(maxLineLength);
                    char[] buffer = GetRandomLine(charset, lineLength);
                    fs.WriteLine(buffer);
                }
            }
        }

        private static IEnumerable<char> GenerateCharset()
        {
            for (char c = char.MinValue; c < char.MaxValue; c++)
            {
                if (('0' <= c && c <= '9')
                    || ('A' <= c && c <= 'Z')
                    || ('a' <= c && c <= 'z'))
                {
                    yield return c;
                }
            }
        }

        private char[] GetRandomLine(char[] charset, int length)
        {
            return Enumerable.Repeat(charset, length)
              .Select(s => s[random.Next(charset.Length)]).ToArray();
        }
    }
}
