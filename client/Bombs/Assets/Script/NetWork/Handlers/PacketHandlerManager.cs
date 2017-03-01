using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
/// <summary>
/// 处理数据
/// </summary>
/// <typeparam name="TClient"></typeparam>
public class PacketHandlerManager
{
    public delegate void PacketHandlerFunc(Client client, byte[] datas, ushort start_index, ushort msg_length);
    private Dictionary<byte, PacketHandlerFunc> _handlers;

    public PacketHandlerManager()
    {
        _handlers = new Dictionary<byte, PacketHandlerFunc>();
    }

    /// <summary>
    /// 添加一个处理数据的模块
    /// </summary>
    /// <param name="op"></param>
    /// <param name="handler"></param>
    public void Add(byte op, PacketHandlerFunc handler)
    {
        _handlers[op] = handler;
    }

    /// <summary>
    /// 加载所有处理数据的模块
    /// </summary>
    public void AutoLoad()
    {
        foreach (var method in this.GetType().GetMethods())
        {
            foreach (PacketHandlerAttribute attr in method.GetCustomAttributes(typeof(PacketHandlerAttribute), false))
            {
                var del = (PacketHandlerFunc)Delegate.CreateDelegate(typeof(PacketHandlerFunc), this, method);
                foreach (var op in attr.Ops)
                    this.Add(op, del);
            }
        }
    }

    /// <summary>
    /// 处理数据
    /// </summary>
    /// <param name="client"></param>
    /// <param name="command"></param>
    /// <param name="datas"></param>
    public virtual void Handle(Client client, byte command, byte[] datas, ushort start_index, ushort msg_length)
    {
        PacketHandlerFunc handler;
        if (!_handlers.TryGetValue(command, out handler))
        {
            this.UnknownPacket(client, command);
            return;
        }

        try
        {
            handler(client, datas, start_index, msg_length);
        }
        catch (PacketElementTypeException ex)
        {
            UnityEngine.Debug.Log(
                "PacketElementTypeException: " + ex.Message + Environment.NewLine +
                ex.StackTrace + Environment.NewLine +
                "Packet: " + Environment.NewLine +
                command.ToString()
            );
        }
    }

    public virtual void UnknownPacket(Client client, int command)
    {
        UnityEngine.Debug.Log(string.Format("PacketHandlerManager: Handler for '{0:X4}', '{1}'.", command, Op.GetName(command)));
    }
    public void RecvData<T>(byte[] data, out T t, ushort start, ushort length)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            ms.Write(data, start, length);
            ms.Position = 0;
            t = Serializer.Deserialize<T>(ms);
        }
    }
}

public class PacketHandlerAttribute : Attribute
{
    public byte[] Ops { get; protected set; }

    public PacketHandlerAttribute(params byte[] ops)
    {
        this.Ops = ops;
    }
}

public enum PacketElementType : byte
{
    None = 0,
    Byte = 1,
    Short = 2,
    Int = 3,
    Long = 4,
    Float = 5,
    String = 6,
    Bin = 7,
}

public class PacketElementTypeException : Exception
{
    public PacketElementTypeException(PacketElementType expected, PacketElementType actual)
        : base(string.Format("Expected {0}, got {1}.", expected, actual))
    {
    }
}