using System;
using System.Net.Sockets;
using Comm.Util;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ProtoBuf;
using System.Linq;
namespace Comm.Network.Iocp
{
    public  class Session : IDisposable
    {
        private const int BufferSize = 1024;
        public ushort Id;
        public Socket Socket { get; set; }
        private ConcurrentPool<SocketAsyncEventArgs> m_hSendOps;
        private ConcurrentPool<SocketAsyncEventArgs> m_hRecvOps;
        public CallBack<Session, byte, byte[],ushort, ushort> Handle;
        private byte[] RecvBuffer;
        private int CurrentOffset=0;

        public Session()
        {
            //TODO: optimize SocketAsyncEventArgs distribution
            m_hSendOps = new ConcurrentPool<SocketAsyncEventArgs>();
            m_hRecvOps = new ConcurrentPool<SocketAsyncEventArgs>();

            for (int i = 0; i < 10; i++)
            {
                SocketAsyncEventArgs hSendOp = new SocketAsyncEventArgs();
                hSendOp.Completed += OnSendCompleted;
                hSendOp.SetBuffer(new byte[BufferSize], 0, BufferSize);
                m_hSendOps.Recycle(hSendOp);

                SocketAsyncEventArgs hRecvOp = new SocketAsyncEventArgs();
                hRecvOp.Completed += OnRecvCompleted;
                hRecvOp.SetBuffer(new byte[BufferSize], 0, BufferSize);
                m_hRecvOps.Recycle(hRecvOp);
            }
            RecvBuffer = new byte[BufferSize];
        }

        public void Dispose()
        {
            //Todo: dispose pools
            //m_hSendOp.Dispose();
            //m_hRecvOp.Dispose();
        }

        public void Close()
        {
            try
            {
                this.Socket.Shutdown(SocketShutdown.Both);
                this.Socket.Close();
                this.Socket = null;
            }
            catch (Exception)
            {
                //TODO: better check if something went wrong during shutdown
            }
        }

        public void Start()
        {
            this.StartRecv();
        }

        private void StartRecv()
        {
            SocketAsyncEventArgs hRecv = m_hRecvOps.Get();
            if (!Socket.ReceiveAsync(hRecv))
                OnRecvCompleted(this, hRecv);
        }

        private void OnRecvCompleted(object hSender, SocketAsyncEventArgs hE)
        {
            try
            {
                if (hE.BytesTransferred == 0 || hE.SocketError != SocketError.Success)
                {//IsDisconnect
                    m_hRecvOps.Recycle(hE);
                    throw new SocketException();
                }
                
                //===拷贝数据到缓存===
                Buffer.BlockCopy(hE.Buffer, 0, RecvBuffer, CurrentOffset, hE.BytesTransferred);
                CurrentOffset += hE.BytesTransferred;
                m_hRecvOps.Recycle(hE);

                ushort DataSize,TotalSize,MsgSize;
                byte command;
                while (CurrentOffset> 3)
                {
                    DataSize = BitConverter.ToUInt16(RecvBuffer,0);
                    TotalSize = DataSize;
                    if (TotalSize <= CurrentOffset)
                    {
                        command= RecvBuffer[2];//命令
                        MsgSize = (ushort)(DataSize - 3);
                        Handle(this, command, RecvBuffer,3, MsgSize);
                        //---删除----
                        Array.Clear(RecvBuffer, 0, TotalSize);
                        CurrentOffset-= TotalSize;
                    }
                    else
                    {
                        break;
                    }
                }
                this.StartRecv();
            }
            catch (Exception hEx)
            {
                Console.WriteLine(hEx.Message);
                //Service.Recycle(this);
            }
        }

        public void Send(Packet hPacket)
        {
            try
            {
                SocketAsyncEventArgs hSendOp = m_hSendOps.Get();

                Buffer.BlockCopy(hPacket.Buffer, 0, hSendOp.Buffer, 0, Packet.HeaderSize + hPacket.DataSize);
                hSendOp.SetBuffer(0, Packet.HeaderSize + hPacket.DataSize);

                if (!Socket.SendAsync(hSendOp))
                    this.OnSendCompleted(this, hSendOp);
            }
            catch (Exception hEx)
            {
                Console.WriteLine(hEx);
            }
        }
        public void Send<T>(byte type, T t)
        {
            try
            {
                SocketAsyncEventArgs hSendOp = m_hSendOps.Get();
                //============生成数据============
                byte[] msg;
                using (MemoryStream ms = new MemoryStream())
                {
                    Serializer.Serialize<T>(ms, t);
                    msg = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(msg, 0, msg.Length);
                }

                ushort total_length = (ushort)(msg.Length + 3);
                byte[] total_length_bytes = BitConverter.GetBytes(total_length);
                //消息体结构：消息体长度+消息体
                byte[] data = new byte[total_length];
                total_length_bytes.CopyTo(data, 0);
                data[2] = type;
                msg.CopyTo(data, 3);
                //============发送数据============
                Buffer.BlockCopy(data, 0, hSendOp.Buffer, 0, data.Length);
                hSendOp.SetBuffer(0, data.Length);
                if (!Socket.SendAsync(hSendOp))//投递发送请求，这个函数有可能同步发送出去，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件
                {
                    m_hSendOps.Recycle(hSendOp);
                }
            }
            catch (Exception hEx)
            {
                Console.WriteLine(hEx);
            }

        }

        private void OnSendCompleted(object hSender, SocketAsyncEventArgs hE)
        {
            m_hSendOps.Recycle(hE);
        }
    }
    //public static class SomeExtensions
    //{
    //    public static bool IsDisconnect(this SocketAsyncEventArgs hThis)
    //    {
    //        if (hThis.BytesTransferred == 0 || hThis.SocketError != SocketError.Success)
    //            return true;
    //        else
    //            return false;
    //    }

    //    public static int CopyTo(this SocketAsyncEventArgs hThis, byte[] hBuffer, ref int iOffset)
    //    {
    //        Buffer.BlockCopy(hThis.Buffer, 0, hBuffer, iOffset, hThis.BytesTransferred);
    //        iOffset += hThis.BytesTransferred;
    //        return hThis.BytesTransferred;
    //    }

    //    public static bool ContainsPacket(this byte[] hThis, int iOffset, int iBufferDataOffset, int iDataToConsume, out byte bId, out ushort uDataSize)
    //    {
    //        bId = hThis[iOffset];
    //        uDataSize = BitConverter.ToUInt16(hThis, iOffset + 1);

    //        if (Packet.HeaderSize + uDataSize <= iBufferDataOffset && iDataToConsume > 0)
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }

    //    public static void Reorder(this byte[] hThis, int iCurrentIndex, ref int iDataOffset)
    //    {
    //        Buffer.BlockCopy(hThis, iCurrentIndex, hThis, 0, iDataOffset - iCurrentIndex);
    //        Array.Clear(hThis, iDataOffset - iCurrentIndex, iDataOffset);
    //        iDataOffset = 0;
    //    }
    //}
}
