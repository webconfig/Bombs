namespace UnityEngine
{
    public class Transform: MonoBehaviour
    {
        public Vector3 position;
        public Quaternion rotation;
        public void Start()
        {
            
        }


        public Vector3 right
        {
            get
            {
                return this.rotation * Vector3.right;
            }
        }

        public Vector3 up
        {
            get
            {
                return this.rotation * Vector3.up;
            }
        }

        public Vector3 forward
        {
            get
            {
                return this.rotation * Vector3.forward;
            }
        }
    }
}
