using System.IO;

namespace Comm.Network
{
    /// <summary>
    /// La classe Packet si occupa della gestione della memoria
    /// operazioni di codifica/decodifica binaria
    /// è il building block di quello che sarà poi il protocollo dell'applicazione
    /// </summary>
    public abstract class Packet
    {
        public const int DataSizeIndex = 1;
        public const int CallIdIndex = 3;
        public const int HeaderSize = 5;

        public byte ID { get { return Buffer[0]; } set { Buffer[0] = value; } }
        public ushort DataSize { get; private set; }
        public ushort CallId { get; set; }
        public byte[] Buffer { get; private set; }

        private MemoryStream m_hMs;
        protected BinaryWriter m_hWriter;
        protected Packet(byte bPacketId, int iDataSize)
        {
            Buffer = new byte[iDataSize + HeaderSize];
            m_hMs = new MemoryStream(Buffer);             //Comodo per evitare di copiare i buffer a mano      
            m_hWriter = new BinaryWriter(m_hMs);              //Comodo per effetuare le conversioni in binario
            ID = bPacketId;
        }

        protected void BeginEncode()
        {
            m_hMs.Seek(HeaderSize, SeekOrigin.Begin);
        }

        protected void EndEncode()
        {
            DataSize = (ushort)(m_hMs.Position - HeaderSize);

            m_hMs.Seek(1, SeekOrigin.Begin);
            m_hWriter.Write(DataSize);
            m_hWriter.Write(CallId);
            m_hWriter.Flush();
        }

        public virtual void LoadData(BinaryReader hReader)
        {
            DataSize = hReader.ReadUInt16();
            CallId = hReader.ReadUInt16();
        }

        public override string ToString()
        {
            return string.Format("{0}:Id = {1} DataSize = {2} CallId = {3}", this.GetType().Name, this.ID, this.DataSize, this.CallId);
        }

    }
}
