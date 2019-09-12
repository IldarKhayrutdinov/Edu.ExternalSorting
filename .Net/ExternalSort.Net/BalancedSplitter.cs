using System.IO;
using System.Threading.Tasks;

namespace ExternalSort.Net
{
    internal class BalancedSplitter
    {
        private readonly StreamReader source;

        private readonly StreamWriter[] destinations;

        private readonly string[][] consumerBuffers;

        public BalancedSplitter(StreamReader source, StreamWriter[] destinations)
        {
            this.source = source;
            this.destinations = destinations;

            int consumersCount = destinations.Length;
            int totalBufferLines = (Config.BufferSizeBytes / Config.MaxLineLength) / consumersCount;
            int bufLines = totalBufferLines / consumersCount;

            consumerBuffers = new string[consumersCount][];
            for (int i = 0; i < consumersCount; i++)
            {
                consumerBuffers[i] = new string[bufLines];
            }

            BlockLength = bufLines;
        }

        public int BlockLength { get; }

        public void Split()
        {

            Task[] tasks = new Task[consumerBuffers.Length];
            int activeConsumers = 0;
            do
            {
                activeConsumers = 0;
                for (int i = 0; i < consumerBuffers.Length; i++)
                {
                    int readedLines = ReadLines(source, consumerBuffers[i]);
                    if (readedLines == 0)
                    {
                        break;
                    }

                    activeConsumers++;
                    int localIndex = i;
                    tasks[i] = Task.Factory.StartNew(() => ConsumerThreadFunc(localIndex, readedLines));
                }

                for (int i = 0; i < activeConsumers; i++)
                {
                    tasks[i].Wait();
                }
            }
            while (activeConsumers > 0);
        }

        private void ConsumerThreadFunc(int index, int length)
        {
            //Array.Sort(consumerBuffers[index], 0, length, Config.Comparison);
            InternalSorter.QuickSort(consumerBuffers[index], length);
            for (int i = 0; i < length; i++)
            {
                destinations[index].WriteLine(consumerBuffers[index][i]);
            }
        }

        private static int ReadLines(StreamReader fs, string[] buf)
        {
            int i = 0;
            while (!fs.EndOfStream && i < buf.Length)
            {
                buf[i++] = fs.ReadLine();
            }

            return i;
        }
    }
}
