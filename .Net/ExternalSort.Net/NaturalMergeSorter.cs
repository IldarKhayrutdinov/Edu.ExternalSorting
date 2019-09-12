using System;
using System.IO;

namespace ExternalSort.Net
{
    /// <summary>
    /// External natural merge sort algorithm.
    /// </summary>
    internal class NaturalMergeSorter
    {
        public static void Sort(string filePath, Comparison<string> comparator)
        {
            string tempFile1 = filePath + "__temp1.txt";
            string tempFile2 = filePath + "__temp2.txt";

            bool doProcess = true;
            while (doProcess)
            {
                // split src file
                using (var fs = new StreamReader(filePath, Config.Encoding))
                using (var fs1 = new StreamWriter(tempFile1, false, Config.Encoding))
                using (var fs2 = new StreamWriter(tempFile2, false, Config.Encoding))
                {
#if DEBUG
                    fs1.AutoFlush = true;
                    fs2.AutoFlush = true;
#endif
                    var context = new SplitterContext(fs1, fs2);
                    var writer = context.Writer;

                    string last = fs.ReadLine();
                    string line = fs.ReadLine();

                    writer.WriteLine(last);
                    while (!fs.EndOfStream)
                    {
                        if (comparator(last, line) > 0)
                        {
                            writer = context.Switch();
                        }

                        writer.WriteLine(line);

                        last = line;
                        line = fs.ReadLine();
                    }

                    // flush buffer
                    writer.WriteLine(line);

                    //// calc doProcess
                    //fs1.Flush();
                    //fs2.Flush();
                    //doProcess = fs1.BaseStream.Length != 0 && fs2.BaseStream.Length != 0;

                    // add synthetic end of block markers
                    fs1.WriteLine(string.Empty);
                    fs2.WriteLine(string.Empty);
                }

                if (!doProcess)
                {
#if !DEBUG
                    File.Delete(tempFile1);
                    File.Delete(tempFile2);
#endif
                    break;
                }

                // merge splitted files
                using (var fs = new StreamWriter(filePath, false, Config.Encoding))
                using (var fs1 = new StreamReader(tempFile1, Config.Encoding))
                using (var fs2 = new StreamReader(tempFile2, Config.Encoding))
                {
#if DEBUG
                    fs.AutoFlush = true;
#endif
                    var reader1 = new MergerBlockContext(fs1, comparator);
                    var reader2 = new MergerBlockContext(fs2, comparator);

                    string line1 = reader1.BeginRead();
                    string line2 = reader2.BeginRead();

                    long destBlocksCount = 0;
                    while (!fs1.EndOfStream && !fs2.EndOfStream)
                    {
                        destBlocksCount++;
                        reader1.BeginBlock();
                        reader2.BeginBlock();

                        // merge current blocks
                        while (!reader1.IsBlockComplete && !reader2.IsBlockComplete)
                        {
                            if (comparator(line1, line2) < 0)
                            {
                                fs.WriteLine(line1);
                                line1 = reader1.ReadLine();
                            }
                            else
                            {
                                fs.WriteLine(line2);
                                line2 = reader2.ReadLine();
                            }
                        }

                        // flush block for file1
                        while (!reader1.IsBlockComplete)
                        {
                            fs.WriteLine(line1);
                            line1 = reader1.ReadLine();
                        }

                        // flush block for file2
                        while (!reader2.IsBlockComplete)
                        {
                            fs.WriteLine(line2);
                            line2 = reader2.ReadLine();
                        }
                    }

                    bool hasExtBlock = false;

                    // flush remained block for file1
                    reader1.BeginBlock();
                    while (!fs1.EndOfStream && !reader1.IsBlockComplete)
                    {
                        fs.WriteLine(line1);
                        line1 = reader1.ReadLine();
                        hasExtBlock = true;
                    }

                    // flush remained block for file2
                    reader2.BeginBlock();
                    while (!fs2.EndOfStream && !reader2.IsBlockComplete)
                    {
                        fs.WriteLine(line2);
                        line2 = reader2.ReadLine();
                        hasExtBlock = true;
                    }

                    doProcess = destBlocksCount > 1 || hasExtBlock;
                }
            }
        }

        private class SplitterContext
        {
            private readonly StreamWriter fs1;

            private readonly StreamWriter fs2;

            private int index;

            public SplitterContext(StreamWriter fs1, StreamWriter fs2)
            {
                this.fs1 = fs1;
                this.fs2 = fs2;

                index = 0;
            }

            public StreamWriter Switch()
            {
                index = index == 0 ? 1 : 0;
                return Writer;
            }

            public StreamWriter Writer => index == 0 ? fs1 : fs2;
        }

        private class MergerBlockContext
        {
            private readonly StreamReader fs;

            private readonly Comparison<string> comparator;

            private string last;

            public MergerBlockContext(StreamReader fs, Comparison<string> comparator)
            {
                this.fs = fs;
                this.comparator = comparator;
            }

            public bool IsBlockComplete { get; private set; }

            public string BeginRead()
            {
                last = fs.ReadLine();
                IsBlockComplete = false;
                return last;
            }

            public string ReadLine()
            {
                string line = fs.ReadLine();
                IsBlockComplete = comparator(last, line) > 0;
                last = line;
                return line;
            }

            public void BeginBlock()
            {
                IsBlockComplete = false;
            }
        }
    }
}
