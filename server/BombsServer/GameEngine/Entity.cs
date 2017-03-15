using System.IO;
namespace GameEngine
{
    public class Entity
    {
        public int id;
        public float x = 0;
        public float y = 0;
        public float z = 0;
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
            switch ((KeyCode)input.keycode)
            {
                case KeyCode.D:
                    this.x += input.lagMs * this.speed;
                    break;
                case KeyCode.A:
                    this.x -= input.lagMs * this.speed;
                    break;
                case KeyCode.W:
                    this.z += input.lagMs * this.speed;
                    break;
                case KeyCode.S:
                    this.z -= input.lagMs * this.speed;
                    break;
            }
        }

        public void Serialization(BinaryWriter w)
        {
            w.Write(id);
            w.Write(x);
            w.Write(y);
            w.Write(z);
            w.Write(speed);
        }
        public void DeSerialization(BinaryReader r)
        {
            id = r.ReadInt32();
            x = r.ReadSingle();
            y = r.ReadSingle();
            z = r.ReadSingle();
            speed = r.ReadSingle();
        }
    }
}
