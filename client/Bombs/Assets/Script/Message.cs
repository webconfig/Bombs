//using System.Collections.Generic;
//using System.IO;
//using System;

//public class Message
//{
//    public int seqNum;
//}
//public class Input : Message
//{
//    public int keycode;
//    public int entityId;
//    public float lagMs;
//    //=========
//    public DateTime recvTs;
//    public Input() { }
//    public void Serialization(BinaryWriter w)
//    {
//        w.Write(entityId);
//        w.Write(seqNum);
//        w.Write(keycode);
//        w.Write(lagMs);
//    }
//    public void DeSerialization(BinaryReader r)
//    {
//        entityId = r.ReadInt32();
//        seqNum = r.ReadInt32();
//        keycode = r.ReadInt32();
//        lagMs = r.ReadSingle();
//    }
//}

////public class WorldState : Message
////{
////    public List<Entity> entities;
////    public Dictionary<int, int> lastProcessedInputSeqNums;
////    public WorldState() { }
////    public WorldState(int seqNum, List<Entity> entities, Dictionary<int, int> lastProcessedInputSeqNums)
////    {
////        this.seqNum = seqNum;
////        this.entities = entities;
////        this.lastProcessedInputSeqNums = lastProcessedInputSeqNums;
////    }

////    public void Serialization(BinaryWriter w)
////    {
////        w.Write(seqNum);
////        w.Write(entities.Count);
////        foreach (var item in entities)
////        {
////            item.Serialization(w);
////        }
////        w.Write(lastProcessedInputSeqNums.Count);
////        foreach (var item in lastProcessedInputSeqNums)
////        {
////            w.Write(item.Key);
////            w.Write(item.Value);
////        }
////    }
////    public void DeSerialization(BinaryReader r)
////    {
////        seqNum = r.ReadInt32();
////        int count = r.ReadInt32();
////        entities = new List<Entity>();
////        for (int i = 0; i < count; i++)
////        {
////            Entity item = new Entity();
////            item.DeSerialization(r);
////            entities.Add(item);
////        }
////        count = r.ReadInt32();
////        lastProcessedInputSeqNums = new Dictionary<int, int>();
////        for (int i = 0; i < count; i++)
////        {
////            lastProcessedInputSeqNums.Add(r.ReadInt32(), r.ReadInt32());
////        }
////    }
////}

////public class SavedWorldState
////{
////    /**
////     * The time at which the client processed this WorldState message.
////     */
////    public long processedTs;
////    public WorldState value;
////    public SavedWorldState(long processedTs, WorldState value)
////    {
////        this.processedTs = processedTs;
////        this.value = value;
////    }
////}

//public class QueuedMessage
//{
//    public int recvTs;
//    public Message payload;
//}

//public class Entity
//{
//    public int id;
//    public float x = 0;
//    public float z = 0;
//    public float speed = 2;
//    public string anim;
//    public Entity(int id)
//    {
//        this.id = id;
//    }
//    public Entity()
//    {
//    }

//    public void DeSerialization(BinaryReader r)
//    {
//        id = r.ReadInt32();
//        x = r.ReadSingle();
//        z = r.ReadSingle();
//        speed = r.ReadSingle();
//        anim = r.ReadString();
//    }
//}
