using System.Collections.Concurrent;

namespace GameEngine
{
    public static class IdGenerator
    {
        private static ConcurrentQueue<ushort> m_hRecycled;
        private static volatile ushort m_uCounter;

        static IdGenerator()
        {
            m_hRecycled = new ConcurrentQueue<ushort>();
        }

        public static ushort Get()
        {

            ushort uId;

            if (m_hRecycled.TryDequeue(out uId))
            {
                return uId;
            }
            else
            {
                checked
                {
                    m_uCounter++;
                    return m_uCounter;
                }

            }
        }

        public static void Recycle(ushort uValue)
        {
            m_hRecycled.Enqueue(uValue);
        }
    }
}
