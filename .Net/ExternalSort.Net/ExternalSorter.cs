using System;
using System.IO;
using System.Text;

namespace ExternalSort
{
    internal class ExternalSorter
    {
        public static void Sort(string filePath, Encoding encoding, Func<string, string, int> comparator)
        {
            string tempFile1 = filePath + "__temp1.txt";
            string tempFile2 = filePath + "__temp2.txt";

            bool t1 = true;
            bool t2 = true;
            while (t1 && t2)
            {
                using (var fs = new StreamReader(filePath, encoding))
                using (var fs1 = new StreamWriter(tempFile1, false, encoding))
                using (var fs2 = new StreamWriter(tempFile2, false, encoding))
                {
                    string prev = fs.ReadLine();
                    string line = fs.ReadLine();

                    fs1.WriteLine(prev);
                    bool is1 = true;
                    t1 = false;
                    t2 = false;
                    while (!fs.EndOfStream)
                    {
                        if (comparator(line, prev) < 0)
                        {
                            if (is1)
                            {
                                fs1.WriteLine();
                                t1 = true;
                            }
                            else
                            {
                                fs2.WriteLine();
                                t2 = true;
                            }

                            is1 = !is1;
                        }

                        if (is1)
                        {
                            fs1.WriteLine(line);
                            t1 = true;
                        }
                        else
                        {
                            fs2.WriteLine(line);
                            t2 = true;
                        }

                        prev = line;
                        line = fs.ReadLine();
                    }

                    if (is1)
                    {
                        fs1.WriteLine(line);
                    }
                    else
                    {
                        fs2.WriteLine(line);
                    }

                    fs1.Flush();
                    fs2.Flush();

                    if (fs1.BaseStream.Length == 0 && fs2.BaseStream.Length == 0)
                    {
                        break;
                    }
                }

                if (!t1 || !t2)
                {
                    break;
                }

                //
                using (var fs = new StreamWriter(filePath, false, encoding))
                using (var fs1 = new StreamReader(tempFile1, encoding))
                using (var fs2 = new StreamReader(tempFile2, encoding))
                {
                    string line1 = !fs1.EndOfStream ? fs1.ReadLine() : null;
                    string line2 = !fs2.EndOfStream ? fs2.ReadLine() : null;

                    bool blockComplete1 = false;
                    bool blockComplete2 = false;
                    while (!fs1.EndOfStream && !fs2.EndOfStream)
                    {
                        blockComplete1 = false;
                        blockComplete2 = false;
                        while (!blockComplete1 && !blockComplete2)
                        {
                            if (comparator(line1, line2) < 0)
                            {
                                fs.WriteLine(line1);
                                line1 = fs1.ReadLine();
                                blockComplete1 = string.IsNullOrEmpty(line1);
                            }
                            else
                            {
                                fs.WriteLine(line2);
                                line2 = fs2.ReadLine();
                                blockComplete2 = string.IsNullOrEmpty(line2);
                            }
                        }

                        while (!blockComplete1)
                        {
                            fs.WriteLine(line1);
                            line1 = fs1.ReadLine();
                            blockComplete1 = string.IsNullOrEmpty(line1);
                        }

                        while (!blockComplete2)
                        {
                            fs.WriteLine(line2);
                            line2 = fs2.ReadLine();
                            blockComplete2 = string.IsNullOrEmpty(line2);
                        }
                    }

                    blockComplete1 = false;
                    blockComplete2 = false;
                    while (!fs1.EndOfStream && !blockComplete1)
                    {
                        fs.WriteLine(line1);
                        line1 = fs1.ReadLine();
                        blockComplete1 = string.IsNullOrEmpty(line1);
                    }

                    while (!fs2.EndOfStream && !blockComplete2)
                    {
                        fs.WriteLine(line2);
                        line2 = fs2.ReadLine();
                        blockComplete2 = string.IsNullOrEmpty(line2);
                    }
                }
            }
        }

    }
}
