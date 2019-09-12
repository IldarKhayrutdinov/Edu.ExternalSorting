using System;
using System.Collections.Generic;
using System.Text;

namespace ExternalSort.Net
{
    internal static class Config
    {
        public const int TotalLinesCount = 1000000; // DefaultTotalLinesCount

        public const int MinLineLength = 1;

        public const int MaxLineLength = 50; // DefaultMaxLineLength

        public const int BufferSizeBytes = 1000000; // DefaultBufferSizeBytes

        private const int DefaultTotalLinesCount = 10 * (1024 * 1024 * 1024 / DefaultMaxLineLength);

        private const int DefaultMaxLineLength = 500;

        private const int DefaultBufferSizeBytes = 1024 * 1024;

        private const int ComparingSymbols = 50;

        /// <summary>
        /// The text encoding.
        /// </summary>
        /// <remarks>
        /// 1251	- ANSI Cyrillic
        /// 1252	- ANSI Latin 1
        ///</remarks>
        public static readonly Encoding Encoding =
#if NETCOREAPP2_2
            CodePagesEncodingProvider.Instance.GetEncoding(1252);
#else
            Encoding.GetEncoding("windows-1252");
#endif

        public static readonly IComparer<string> Comparison = new ComparisonComparer<string>(Config.Comparator);

        public static int Comparator(string line1, string line2)
        {
            return string.Compare(line1, 0, line2, 0, ComparingSymbols);
        }

        public static int NullableComparator(string line1, string line2)
        {
            if (line1 == null)
            {
                if (line2 == null)
                {
                    return 0;
                }

                return 1;
            }
            else if (line2 == null)
            {
                return -1;
            }

            return string.Compare(line1, 0, line2, 0, ComparingSymbols);
        }

        private class ComparisonComparer<T> : IComparer<T>
        {
            private readonly Comparison<T> comparison;

            public ComparisonComparer(Comparison<T> comparison)
            {
                this.comparison = comparison ?? throw new ArgumentNullException();
            }

            public int Compare(T x, T y)
            {
                return comparison(x, y);
            }
        }
    }
}
