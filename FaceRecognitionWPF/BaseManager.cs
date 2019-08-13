using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FaceRecognitionWPF
{
    public abstract class BaseManager
    {
        protected IFormatterConverter _formatterConverter = new FormatterConverter();
        protected StreamingContext _context = new StreamingContext();

        public void StartThreads(int threadCount)
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

        public abstract void ThreadWork();
    }
}
