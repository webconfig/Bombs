
namespace UnityEngine
{
    public struct Quaternion
    {
        public float x;

        public float y;

        public float z;

        public float w;

        public const float kEpsilon = 1E-06f;


        public float this[int index]
        {
            get
            {
                float result=-1;
                switch (index)
                {
                    case 0:
                        result = this.x;
                        break;
                    case 1:
                        result = this.y;
                        break;
                    case 2:
                        result = this.z;
                        break;
                    case 3:
                        result = this.w;
                        break;
                }
                return result;
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.x = value;
                        break;
                    case 1:
                        this.y = value;
                        break;
                    case 2:
                        this.z = value;
                        break;
                    case 3:
                        this.w = value;
                        break;
                }
            }
        }

        public static Quaternion identity
        {
            get
            {
                return new Quaternion(0f, 0f, 0f, 1f);
            }
        }

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public void Set(float new_x, float new_y, float new_z, float new_w)
        {
            this.x = new_x;
            this.y = new_y;
            this.z = new_z;
            this.w = new_w;
        }

        public static Quaternion operator *(Quaternion lhs, Quaternion rhs)
        {
            return new Quaternion(lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y, lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z, lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x, lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);
        }

        public static Vector3 operator *(Quaternion rotation, Vector3 point)
        {
            float num = rotation.x * 2f;
            float num2 = rotation.y * 2f;
            float num3 = rotation.z * 2f;
            float num4 = rotation.x * num;
            float num5 = rotation.y * num2;
            float num6 = rotation.z * num3;
            float num7 = rotation.x * num2;
            float num8 = rotation.x * num3;
            float num9 = rotation.y * num3;
            float num10 = rotation.w * num;
            float num11 = rotation.w * num2;
            float num12 = rotation.w * num3;
            Vector3 result;
            result.x = (1f - (num5 + num6)) * point.x + (num7 - num12) * point.y + (num8 + num11) * point.z;
            result.y = (num7 + num12) * point.x + (1f - (num4 + num6)) * point.y + (num9 - num10) * point.z;
            result.z = (num8 - num11) * point.x + (num9 + num10) * point.y + (1f - (num4 + num5)) * point.z;
            return result;
        }

        public static bool operator ==(Quaternion lhs, Quaternion rhs)
        {
            return Quaternion.Dot(lhs, rhs) > 0.999999f;
        }

        public static bool operator !=(Quaternion lhs, Quaternion rhs)
        {
            return !(lhs == rhs);
        }

        public static float Dot(Quaternion a, Quaternion b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
        }


        public static float Angle(Quaternion a, Quaternion b)
        {
            float f = Quaternion.Dot(a, b);
            return Mathf.Acos(Mathf.Min(Mathf.Abs(f), 1f)) * 2f * 57.29578f;
        }

        private static Vector3 Internal_MakePositive(Vector3 euler)
        {
            float num = -0.005729578f;
            float num2 = 360f + num;
            if (euler.x < num)
            {
                euler.x += 360f;
            }
            else if (euler.x > num2)
            {
                euler.x -= 360f;
            }
            if (euler.y < num)
            {
                euler.y += 360f;
            }
            else if (euler.y > num2)
            {
                euler.y -= 360f;
            }
            if (euler.z < num)
            {
                euler.z += 360f;
            }
            else if (euler.z > num2)
            {
                euler.z -= 360f;
            }
            return euler;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2 ^ this.w.GetHashCode() >> 1;
        }

        public override bool Equals(object other)
        {
            bool result;
            if (!(other is Quaternion))
            {
                result = false;
            }
            else
            {
                Quaternion quaternion = (Quaternion)other;
                result = (this.x.Equals(quaternion.x) && this.y.Equals(quaternion.y) && this.z.Equals(quaternion.z) && this.w.Equals(quaternion.w));
            }
            return result;
        }

        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1}, {2:F1}, {3:F1})", new object[]
            {
                this.x,
                this.y,
                this.z,
                this.w
            });
        }

        public string ToString(string format)
        {
            return string.Format("({0}, {1}, {2}, {3})", new object[]
            {
                this.x.ToString(format),
                this.y.ToString(format),
                this.z.ToString(format),
                this.w.ToString(format)
            });
        }
    }
}
