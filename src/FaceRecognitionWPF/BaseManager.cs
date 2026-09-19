using System;
using System.Runtime.Serialization;
using System.Threading;

namespace FaceRecognitionWPF
{
    public abstract class BaseManager
    {
        protected IFormatterConverter _formatterConverter = new FormatterConverter();
        protected StreamingContext _context = new StreamingContext();

        protected readonly CancellationToken _cancellationToken;

        protected BaseManager(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        protected void StartThreads(int threadCount)
        {
            Thread[] threads = new Thread[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                threads[i] = new Thread(ThreadWork);
                threads[i].IsBackground = true;
                threads[i].Priority = ThreadPriority.Lowest;
                threads[i].Start();
            }

            for (int i = 0; i < threadCount; i++)
            {
                threads[i].Join();
            }
        }

        protected abstract void ThreadWork();
    }
}