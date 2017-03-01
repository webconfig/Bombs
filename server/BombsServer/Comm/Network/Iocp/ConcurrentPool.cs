using System.Collections.Concurrent;
namespace Comm.Network.Iocp
{
    internal class ConcurrentPool<T> where
        T : class, new()
    {
        private ConcurrentQueue<T> m_hElements;

        public ConcurrentPool()
        {
            m_hElements = new ConcurrentQueue<T>();
        }

        public T Get()
        {
            T hElem;

            if (m_hElements.TryDequeue(out hElem))
            {
                return hElem;
            }
            else
            {
                return new T();
            }
        }

        public void Recycle(T hElement)
        {
            m_hElements.Enqueue(hElement);
        }
    }
}
