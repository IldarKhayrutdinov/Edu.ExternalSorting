using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ExternalSort.Net
{
    /// <summary>
    /// External N-way balanced merge sort algorithm.
    /// </summary>
    internal class NWayMergeSorter
    {
        private readonly int nWayValue;

        private readonly Config config;

        public NWayMergeSorter(int nWayValue, Config config)
        {
            this.nWayValue = nWayValue;
            this.config = config;
        }

        public string Sort(string filePath)
        {
            Console.WriteLine($"N-Way merge sorter start...\n");

            using (var context = new NWayMergerContext(filePath, nWayValue))
            {
                int srcLength;
                long blockLength;

                var totalSw = Stopwatch.StartNew();

                // initial split
                using (var source = new StreamReader(filePath, Config.Encoding))
                {
                    var destinations = context.CreateWriters();
                    var splitter = new BalancedSplitter(source, destinations, config);
                    splitter.Split();
                    context.ReleaseStreams();

                    srcLength = destinations.Length;
                    blockLength = splitter.BlockLength;

                    Console.WriteLine($"splitting elapsed = {totalSw.Elapsed}");
                }

                // merging
                uint passNo = 0;
                bool doWork = true;
                do
                {
                    context.SwitchStreams();

                    var sources = context.CreateReaders();
                    var destinations = context.CreateWriters();

                    var sw = Stopwatch.StartNew();
                    Merge(sources, srcLength, blockLength, destinations, out int destLength);
                    Console.WriteLine($"pass #{passNo++}, block length = {blockLength}, elapsed = {sw.Elapsed}");

                    srcLength = destLength;
                    blockLength *= nWayValue;

                    context.ReleaseStreams();
                    doWork = destLength > 1;
                }
                while (doWork);

                Console.WriteLine($"\nN-Way merge sorter completed, total elapsed = {totalSw.Elapsed}");

                return context.ClearPaths();
            }
        }

        private void Merge(StreamReader[] sources, int srcLength, long srcBlockLength, StreamWriter[] destinations, out int destLength)
        {
            string[] buffer = new string[srcLength];
            long[] srcBlockskReaded = null;

            destLength = -1;
            int destIndex = -1;
            int srcCompletedBlocks = srcLength;

            do
            {
                // new block
                if (srcLength - srcCompletedBlocks <= 0)
                {
                    // fill buffer
                    Task[] tasks = new Task[srcLength];
                    for (int i = 0; i < srcLength; i++)
                    {
                        if (sources[i].EndOfStream)
                        {
                            srcLength = i;
                            break;
                        }

                        int localIndex = i;
                        tasks[i] = Task.Factory.StartNew(() => buffer[localIndex] = sources[localIndex].ReadLine());
                    }

                    if (srcLength == 0)
                    {
                        // complete merging
                        break;
                    }

                    for (int i = 0; i < srcLength; i++)
                    {
                        tasks[i].Wait();
                    }

                    // seek dest stream
                    destIndex++;
                    if (destIndex == destinations.Length)
                    {
                        destIndex = 0;
                        destLength = destinations.Length;
                    }

                    // initialize variables
                    srcCompletedBlocks = 0;
                    srcBlockskReaded = new long[srcLength];
                }

                // output current min value
                int minIndex = MinIndex(buffer, srcLength);
                string min = buffer[minIndex];
                Task readingTask = null;

                srcBlockskReaded[minIndex]++;
                if (srcBlockskReaded[minIndex] == srcBlockLength || sources[minIndex].EndOfStream)
                {
                    // simulate completed block/stream
                    buffer[minIndex] = null;
                    srcCompletedBlocks++;
                }
                else
                {
                    // read next value to the buffer from stream
                    readingTask = Task.Factory.StartNew(() => buffer[minIndex] = sources[minIndex].ReadLine());
                }

                Task writingTask = Task.Factory.StartNew(() => destinations[destIndex].WriteLine(min));

                readingTask?.Wait();
                writingTask.Wait();
            }
            while (srcLength > 0);

            if (destLength == -1)
            {
                destLength = destIndex + 1;
            }
        }

        private static int MinIndex(string[] buffer, int length)
        {
            int min = 0;
            for (int i = min + 1; i < length; i++)
            {
                // null strings was ignore
                if (Config.NullableComparator(buffer[i], buffer[min]) < 0)
                {
                    min = i;
                }
            }

            return min;
        }

        private class NWayMergerContext : IDisposable
        {
            private readonly int nWayValue;

            private readonly string[] templatesPath;

            private readonly IDisposable[] streams;

            private int index;

            public NWayMergerContext(string sourcePath, int nWayValue)
            {
                var templates = new string[nWayValue << 1];
                for (int i = 0; i < templates.Length; i++)
                {
                    templates[i] = sourcePath + $"__temp_{i}.txt";
                }

                this.templatesPath = templates;
                this.streams = new IDisposable[templates.Length];
                this.nWayValue = nWayValue;
                this.index = nWayValue;
            }

            private int NextIndex => index == 0 ? nWayValue : 0;

            public void SwitchStreams()
            {
                index = NextIndex;
            }

            public StreamReader[] CreateReaders()
            {
                int shift = index;
                var res = new StreamReader[nWayValue];
                for (int i = 0; i < nWayValue; i++)
                {
                    res[i] = new StreamReader(templatesPath[shift + i], Config.Encoding);
                    streams[shift + i] = res[i];
                }

                return res;
            }

            public StreamWriter[] CreateWriters()
            {
                int shift = NextIndex;
                var res = new StreamWriter[nWayValue];
                for (int i = 0; i < nWayValue; i++)
                {
                    res[i] = new StreamWriter(templatesPath[shift + i], false, Config.Encoding);
                    streams[shift + i] = res[i];
#if DEBUG
                    res[i].AutoFlush = true;
#endif
                }

                return res;
            }

            public string ClearPaths()
            {
                int resultIndex = NextIndex;
                for (int i = 0; i < templatesPath.Length; i++)
                {
                    if (i == resultIndex)
                    {
                        continue;
                    }

                    File.Delete(templatesPath[i]);
                }

                return templatesPath[resultIndex];
            }

            public void ReleaseStreams()
            {
                foreach (var stream in streams)
                {
                    stream?.Dispose();
                }
            }

            public void Dispose()
            {
                ReleaseStreams();
            }
        }
    }
}
