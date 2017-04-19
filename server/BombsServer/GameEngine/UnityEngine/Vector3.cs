namespace UnityEngine
{
    public struct Vector3
    {
        public  float x;
        public  float y;
        public  float z;


        public Vector3(float value) { x = y = z = value; }
        public Vector3(float _x, float _y, float _z) { x = _x; y = _y; z = _z; }
        public Vector3(double _x, double _y, double _z) { x = (float)_x; y = (float)_y; z = (float)_z; }
        public Vector3(Vector3 v) { x = v.x; y = v.y; z = v.z; }


        /// <summary>
        /// Converts this Vector3 to a string.
        /// </summary>
        public override string ToString() => $"x: {x}, y: {y}, z: {z}";

        #region Mathf

        public static Vector3 Floor(Vector3 value) => new Vector3(Mathf.Floor(value.x), Mathf.Floor(value.y), Mathf.Floor(value.z));
        public Vector3 Floor() => Floor(this);
        private static float Square(float num) => num * num;
        public static float DistanceTo(Vector3 a, Vector3 b) => a.DistanceTo(b);
        public float DistanceTo(Vector3 other) => (float)Mathf.Sqrt(Square(other.x - x) + Square(other.y - y) + Square(other.z - z));

        /// <summary>
        /// Finds the distance of this vector from Vector3.Zero
        /// </summary>
        public float Distance() => DistanceTo(zero);

        public static Vector3 Min(Vector3 a, Vector3 b) => new Vector3(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));
        public Vector3 Min(Vector3 other) => new Vector3(Mathf.Min(x, other.x), Mathf.Min(y, other.y), Mathf.Min(z, other.z));

        public static Vector3 Max(Vector3 a, Vector3 b) => new Vector3(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));
        public Vector3 Max(Vector3 other) => new Vector3(Mathf.Max(x, other.x), Mathf.Max(y, other.y), Mathf.Max(z, other.z));

        public static Vector3 Delta(Vector3 a, Vector3 b) => a - b;
        public Vector3 Delta(Vector3 other) => this - other;

        public static Vector3 Normalize(Vector3 value)
        {
            var factor = 1f / DistanceTo(value, zero);
            return value * factor;
        }
        public Vector3 Normalize() => Normalize(this);

        #endregion

        #region Operators

        public static Vector3 operator -(Vector3 a) => new Vector3(-a.x, -a.y, -a.z);
        public static Vector3 operator ++(Vector3 a) => new Vector3(a.x, a.y, a.z) + one;
        public static Vector3 operator --(Vector3 a) => new Vector3(a.x, a.y, a.z) - one;

        public static bool operator !=(Vector3 a, Vector3 b) => !a.Equals(b);
        public static bool operator ==(Vector3 a, Vector3 b) => a.Equals(b);
        public static bool operator >(Vector3 a, Vector3 b) => a.x > b.x && a.y > b.y && a.z > b.z;
        public static bool operator <(Vector3 a, Vector3 b) => a.x < b.x && a.y < b.y && a.z < b.z;
        public static bool operator >=(Vector3 a, Vector3 b) => a.x >= b.x && a.y >= b.y && a.z >= b.z;
        public static bool operator <=(Vector3 a, Vector3 b) => a.x <= b.x && a.y <= b.y && a.z <= b.z;

        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator *(Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector3 operator /(Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        public static Vector3 operator %(Vector3 a, Vector3 b) => new Vector3(a.x % b.x, a.y % b.y, a.z % b.z);

        public static Vector3 operator +(Vector3 a, float b) => new Vector3(a.x + b, a.y + b, a.z + b);
        public static Vector3 operator -(Vector3 a, float b) => new Vector3(a.x - b, a.y - b, a.z - b);
        public static Vector3 operator *(Vector3 a, float b) => new Vector3(a.x * b, a.y * b, a.z * b);
        public static Vector3 operator /(Vector3 a, float b) => new Vector3(a.x / b, a.y / b, a.z / b);
        public static Vector3 operator %(Vector3 a, float b) => new Vector3(a.x % b, a.y % b, a.z % b);

        public static Vector3 operator +(float a, Vector3 b) => new Vector3(a + b.x, a + b.y, a + b.z);
        public static Vector3 operator -(float a, Vector3 b) => new Vector3(a - b.x, a - b.y, a - b.z);
        public static Vector3 operator *(float a, Vector3 b) => new Vector3(a * b.x, a * b.y, a * b.z);
        public static Vector3 operator /(float a, Vector3 b) => new Vector3(a / b.x, a / b.y, a / b.z);
        public static Vector3 operator %(float a, Vector3 b) => new Vector3(a % b.x, a % b.y, a % b.z);

        #endregion

        #region Constants

        public static readonly Vector3 zero = new Vector3(0, 0, 0);
        public static readonly Vector3 one = new Vector3(1, 1, 1);

        public static readonly Vector3 up = new Vector3(0, 1, 0);
        public static readonly Vector3 down = new Vector3(0, -1, 0);
        public static readonly Vector3 left = new Vector3(-1, 0, 0);
        public static readonly Vector3 right = new Vector3(1, 0, 0);
        public static readonly Vector3 Backwards = new Vector3(0, 0, -1);
        public static readonly Vector3 forward = new Vector3(0, 0, 1);

        public static readonly Vector3 UnitX = new Vector3(1f, 0f, 0f);
        public static readonly Vector3 UnitY = new Vector3(0f, 1f, 0f);
        public static readonly Vector3 UnitZ = new Vector3(0f, 0f, 1f);

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if (obj is float)
                return Equals((float)obj);

            if (obj is Vector3)
                return Equals((Vector3)obj);

            return false;
        }
        public bool Equals(float other) => other.Equals(x) && other.Equals(y) && other.Equals(z);
        public bool Equals(Vector3 other) => other.x.Equals(x) && other.y.Equals(y) && other.z.Equals(z);

        public override int GetHashCode() => x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
    }
}
