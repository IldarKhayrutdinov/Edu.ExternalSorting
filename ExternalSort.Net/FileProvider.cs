﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExternalSort.Net
{
    internal class FileProvider
    {
        private readonly Random random = new Random();

        public void Generate(string filePath, Config config)
        {
            double magicRandomAverageNumber = 0.5;
            int totalLinesCount = (int)(config.FileSize / config.MaxLineLength / magicRandomAverageNumber);
            var charset = GenerateCharset().ToArray();
            using (var fs = new StreamWriter(filePath, false, Config.Encoding))
            {
                for (int i = 0; i < totalLinesCount; i++)
                {
                    int lineLength = random.Next(config.MaxLineLength - config.MinLineLength) + config.MinLineLength;
                    char[] buffer = BuildRandomLine(charset, lineLength);
                    fs.WriteLine(buffer);
                }
            }
        }

        private static IEnumerable<char> GenerateCharset()
        {
            for (char c = char.MinValue; c < char.MaxValue; c++)
            {
                if (('0' <= c && c <= '9')
#if !DEBUG
                    || ('A' <= c && c <= 'Z')
                    || ('a' <= c && c <= 'z')
#endif
                    )
                {
                    yield return c;
                }
            }
        }

        private char[] BuildRandomLine(char[] charset, int length)
        {
            return Enumerable.Repeat(charset, length)
              .Select(s => s[random.Next(charset.Length)]).ToArray();
        }
    }
}
