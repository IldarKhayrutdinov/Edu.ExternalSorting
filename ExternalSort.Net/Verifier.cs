using System;
using System.IO;
using System.Text;

namespace ExternalSort.Net
{
    internal static class Verifier
    {
        public static bool Verify(string filePath, Encoding encoding, Func<string, string, int> comparator)
        {
            using (var fs = new StreamReader(filePath, encoding))
            {
                string last = fs.ReadLine();
                int lines = 1;
                while (!fs.EndOfStream)
                {
                    string line = fs.ReadLine();
                    if (comparator(last, line) < 0)
                    {
                        return false;
                    }

                    lines++;
                    last = line;
                }
            }

            return true;
        }

        public static bool BinaryCompare(string filePath, string etalonPath)
        {
            const int bufLength = 10 * 1024;
            byte[] buf1 = new byte[bufLength];
            byte[] buf2 = new byte[bufLength];

            using (var fs1 = new FileStream(filePath, FileMode.Open))
            using (var fs2 = new FileStream(etalonPath, FileMode.Open))
            {
                if (fs1.Length != fs2.Length)
                {
                    return false;
                }

                long leftBytes = fs2.Length;
                do
                {
                    int readed1 = fs1.Read(buf1, 0, buf1.Length);
                    int readed2 = fs2.Read(buf2, 0, buf2.Length);

                    if (readed1 != readed2)
                    {
                        return false;
                    }

                    if (!Array.Equals(buf1, buf2))
                    {
                        return false;
                    }

                    leftBytes -= readed1;

                } while (leftBytes > 0);
            }

            return true;
        }
    }
}
