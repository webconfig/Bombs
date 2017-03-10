using System.Collections.Concurrent;
namespace GameEngine.Network
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
            return null;
        }

        public void Recycle(T hElement)
        {
            m_hElements.Enqueue(hElement);
        }
    }
}
