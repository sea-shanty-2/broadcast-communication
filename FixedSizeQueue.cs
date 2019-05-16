using System.Collections.Concurrent;

namespace BroadcastCommunication
{
    // Credit: https://stackoverflow.com/questions/5852863/fixed-size-queue-which-automatically-dequeues-old-values-upon-new-enques
    public class FixedSizedQueue<T> : ConcurrentQueue<T>
    {
        private readonly object syncObject = new object();

        public int Size { get; }

        public FixedSizedQueue(int size)
        {
            Size = size;
        }

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);
            lock (syncObject)
            {
                while (Count > Size)
                {
                    T outObj;
                    base.TryDequeue(out outObj);
                }
            }
        }
    }
}