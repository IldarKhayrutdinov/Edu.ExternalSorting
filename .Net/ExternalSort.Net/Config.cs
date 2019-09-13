using System;
using System.Collections.Generic;
using System.Text;

namespace ExternalSort.Net
{
    internal class Config
    {        
        public const int DefaultBufferSize = 1024 * 1024;

        public const long DefaultFileSize = 10L * 1024 * 1024 * 1024;

        public const int DefaultMaxLineLength = 500;

        public const int ComparingSymbols = 50;

        /// <summary>
        /// The text encoding.
        /// </summary>
        /// <remarks>
        /// 1251	- ANSI Cyrillic
        /// 1252	- ANSI Latin 1
        ///</remarks>
        public static Encoding Encoding { get; } =
#if NETCOREAPP2_2
            CodePagesEncodingProvider.Instance.GetEncoding(1252);
#else
            Encoding.GetEncoding("windows-1252");
#endif

        public static IComparer<string> Comparison { get; } = new ComparisonComparer<string>(Config.Comparator);

        /// <summary>
        /// The buffer size in bytes for sorters.
        /// </summary>
        public int BufferSize { get; set; } = DefaultBufferSize;

        /// <summary>
        /// The max file size in bytes.
        /// </summary>
        /// <remarks>For test file generator.</remarks>
        public long FileSize { get; set; } = DefaultFileSize;

        /// <summary>
        /// The min symbols count in line.
        /// </summary>
        /// <remarks>For test file generator.</remarks>
        public int MinLineLength = 1;

        /// <summary>
        /// The max symbols count in line.
        /// </summary>
        /// <remarks>For test file generator.</remarks>
        public int MaxLineLength { get; set; } = DefaultMaxLineLength;

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