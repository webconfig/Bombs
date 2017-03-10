using System.IO;
namespace GameEngine
{
    public class Entity
    {
        public int id;
        public float x = 0;
        public float speed = 2;
        public int lastProcessedInputSeqNum = -1;
        public Entity(int id)
        {
            this.id = id;
        }
        public Entity()
        {
        }
        public void applyInput(Input input)
        {
            this.x += input.pressTime * this.speed;
        }

        /** Return a copy of this entity. */
        public Entity copy()
        {
            Entity e = new Entity(this.id);
            e.x = this.x;
            e.speed = this.speed;
            return e;
        }

        public void Serialization(BinaryWriter w)
        {
            w.Write(id);
            w.Write(x);
            w.Write(speed);
        }
        public void DeSerialization(BinaryReader r)
        {
            id = r.ReadInt32();
            x = r.ReadSingle();
            speed = r.ReadSingle();
        }
    }
}
