using System;
using System.Collections.Generic;
using System.IO;
namespace GameEngine
{
    public class Message
    {
        public int seqNum;
    }
    public class Input : Message
    {
        public float pressTime;
        public int entityId;
        public Input() { }
        public Input(int seqNum, float pressTime, int entityId)
        {
            this.seqNum = seqNum;
            this.pressTime = pressTime;
            this.entityId = entityId;
        }
        public void Serialization(BinaryWriter w)
        {
            w.Write(seqNum);
            w.Write(pressTime);
            w.Write(entityId);
        }
        public void DeSerialization(BinaryReader r)
        {
            seqNum = r.ReadInt32();
            pressTime = r.ReadSingle();
            entityId = r.ReadInt32();
        }
    }

    public class WorldState : Message
    {
        public List<Entity> entities;
        public Dictionary<int, int> lastProcessedInputSeqNums;

        public WorldState(int seqNum, List<Entity> entities, Dictionary<int, int> lastProcessedInputSeqNums)
        {
            this.seqNum = seqNum;
            this.entities = entities;
            this.lastProcessedInputSeqNums = lastProcessedInputSeqNums;
        }

        public void Serialization(BinaryWriter w)
        {
            w.Write(seqNum);
            w.Write(entities.Count);
            foreach (var item in entities)
            {
                item.Serialization(w);
            }
            w.Write(lastProcessedInputSeqNums.Count);
            foreach (var item in lastProcessedInputSeqNums)
            {
                w.Write(item.Key);
                w.Write(item.Value);
            }
        }
        public void DeSerialization(BinaryReader r)
        {
            seqNum = r.ReadInt32();
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                Entity item = new Entity();
                item.DeSerialization(r);
                entities.Add(item);
            }
            count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                lastProcessedInputSeqNums.Add(r.ReadInt32(),r.ReadInt32());
            }
        }
    }

    //public class SavedWorldState
    //{
    //    /**
    //     * The time at which the client processed this WorldState message.
    //     */
    //    public int processedTs;
    //    public WorldState value;
    //    public SavedWorldState(int processedTs, WorldState value)
    //    {
    //        this.processedTs = processedTs;
    //        this.value = value;
    //    }
    //}

    //public class QueuedMessage
    //{
    //    public long recvTs;
    //    public Message payload;
    //}
}
