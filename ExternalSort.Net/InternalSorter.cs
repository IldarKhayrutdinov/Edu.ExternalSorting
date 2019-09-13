using System.Threading.Tasks;

namespace ExternalSort.Net
{
    internal static class InternalSorter
    {
        public static void QuickSort(string[] array, int length)
        {
            Quick(array, 0, length - 1);
        }

        private static void Quick(string[] array, int leftIndex, int rightIndex)
        {
            if (array.Length == 0)
            {
                return;
            }

            if (leftIndex >= rightIndex)
            {
                return;
            }

            int midIndex = leftIndex + (rightIndex - leftIndex) / 2;
            string mid = array[midIndex];

            int i = leftIndex;
            int j = rightIndex;
            while (i <= j)
            {
                while (Config.Comparator(array[i], mid) < 0)
                {
                    i++;
                }

                while (Config.Comparator(array[j], mid) > 0)
                {
                    j--;
                }

                if (i <= j)
                {
                    string temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;

                    i++;
                    j--;
                }
            }

            /*
            if (leftIndex < j)
            {
                Quick(array, leftIndex, j);
            }

            if (rightIndex > i)
            {
                Quick(array, i, rightIndex);
            }
            /* */

            /* */
            Task leftTask = null;
            if (leftIndex < j)
            {
                leftTask = Task.Factory.StartNew(() => Quick(array, leftIndex, j));
            }

            Task rightTask = null;
            if (rightIndex > i)
            {
                rightTask = Task.Factory.StartNew(() => Quick(array, i, rightIndex));
            }

            leftTask?.Wait();
            rightTask?.Wait();
            /* */
        }
    }
}
