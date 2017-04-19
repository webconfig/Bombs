using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public struct Mathf
    {
        public const float PI = 3.14159274f;

        public const float Infinity = float.PositiveInfinity;

        public const float NegativeInfinity = float.NegativeInfinity;

        public const float Deg2Rad = 0.0174532924f;

        public const float Rad2Deg = 57.29578f;


        public static float Sin(float f)
        {
            return (float)Math.Sin((double)f);
        }

        public static float Cos(float f)
        {
            return (float)Math.Cos((double)f);
        }

        public static float Tan(float f)
        {
            return (float)Math.Tan((double)f);
        }

        public static float Asin(float f)
        {
            return (float)Math.Asin((double)f);
        }

        public static float Acos(float f)
        {
            return (float)Math.Acos((double)f);
        }

        public static float Atan(float f)
        {
            return (float)Math.Atan((double)f);
        }

        public static float Atan2(float y, float x)
        {
            return (float)Math.Atan2((double)y, (double)x);
        }

        public static float Sqrt(float f)
        {
            return (float)Math.Sqrt((double)f);
        }

        public static float Abs(float f)
        {
            return Math.Abs(f);
        }

        public static int Abs(int value)
        {
            return Math.Abs(value);
        }

        public static float Min(float a, float b)
        {
            return (a >= b) ? b : a;
        }

        public static float Min(params float[] values)
        {
            int num = values.Length;
            float result;
            if (num == 0)
            {
                result = 0f;
            }
            else
            {
                float num2 = values[0];
                for (int i = 1; i < num; i++)
                {
                    if (values[i] < num2)
                    {
                        num2 = values[i];
                    }
                }
                result = num2;
            }
            return result;
        }

        public static int Min(int a, int b)
        {
            return (a >= b) ? b : a;
        }

        public static int Min(params int[] values)
        {
            int num = values.Length;
            int result;
            if (num == 0)
            {
                result = 0;
            }
            else
            {
                int num2 = values[0];
                for (int i = 1; i < num; i++)
                {
                    if (values[i] < num2)
                    {
                        num2 = values[i];
                    }
                }
                result = num2;
            }
            return result;
        }

        public static float Max(float a, float b)
        {
            return (a <= b) ? b : a;
        }

        public static float Max(params float[] values)
        {
            int num = values.Length;
            float result;
            if (num == 0)
            {
                result = 0f;
            }
            else
            {
                float num2 = values[0];
                for (int i = 1; i < num; i++)
                {
                    if (values[i] > num2)
                    {
                        num2 = values[i];
                    }
                }
                result = num2;
            }
            return result;
        }

        public static int Max(int a, int b)
        {
            return (a <= b) ? b : a;
        }

        public static int Max(params int[] values)
        {
            int num = values.Length;
            int result;
            if (num == 0)
            {
                result = 0;
            }
            else
            {
                int num2 = values[0];
                for (int i = 1; i < num; i++)
                {
                    if (values[i] > num2)
                    {
                        num2 = values[i];
                    }
                }
                result = num2;
            }
            return result;
        }

        public static float Pow(float f, float p)
        {
            return (float)Math.Pow((double)f, (double)p);
        }

        public static float Exp(float power)
        {
            return (float)Math.Exp((double)power);
        }

        public static float Log(float f, float p)
        {
            return (float)Math.Log((double)f, (double)p);
        }

        public static float Log(float f)
        {
            return (float)Math.Log((double)f);
        }

        public static float Log10(float f)
        {
            return (float)Math.Log10((double)f);
        }

        public static float Ceil(float f)
        {
            return (float)Math.Ceiling((double)f);
        }

        public static float Floor(float f)
        {
            return (float)Math.Floor((double)f);
        }

        public static float Round(float f)
        {
            return (float)Math.Round((double)f);
        }

        public static int CeilToInt(float f)
        {
            return (int)Math.Ceiling((double)f);
        }

        public static int FloorToInt(float f)
        {
            return (int)Math.Floor((double)f);
        }

        public static int RoundToInt(float f)
        {
            return (int)Math.Round((double)f);
        }

        public static float Sign(float f)
        {
            return (f < 0f) ? -1f : 1f;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }
            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }
            return value;
        }

        public static float Clamp01(float value)
        {
            float result;
            if (value < 0f)
            {
                result = 0f;
            }
            else if (value > 1f)
            {
                result = 1f;
            }
            else
            {
                result = value;
            }
            return result;
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Mathf.Clamp01(t);
        }

        public static float LerpUnclamped(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public static float LerpAngle(float a, float b, float t)
        {
            float num = Mathf.Repeat(b - a, 360f);
            if (num > 180f)
            {
                num -= 360f;
            }
            return a + num * Mathf.Clamp01(t);
        }

        public static float MoveTowards(float current, float target, float maxDelta)
        {
            float result;
            if (Mathf.Abs(target - current) <= maxDelta)
            {
                result = target;
            }
            else
            {
                result = current + Mathf.Sign(target - current) * maxDelta;
            }
            return result;
        }

        public static float MoveTowardsAngle(float current, float target, float maxDelta)
        {
            float num = Mathf.DeltaAngle(current, target);
            float result;
            if (-maxDelta < num && num < maxDelta)
            {
                result = target;
            }
            else
            {
                target = current + num;
                result = Mathf.MoveTowards(current, target, maxDelta);
            }
            return result;
        }

        public static float SmoothStep(float from, float to, float t)
        {
            t = Mathf.Clamp01(t);
            t = -2f * t * t * t + 3f * t * t;
            return to * t + from * (1f - t);
        }

        public static float Gamma(float value, float absmax, float gamma)
        {
            bool flag = false;
            if (value < 0f)
            {
                flag = true;
            }
            float num = Mathf.Abs(value);
            float result;
            if (num > absmax)
            {
                result = ((!flag) ? num : (-num));
            }
            else
            {
                float num2 = Mathf.Pow(num / absmax, gamma) * absmax;
                result = ((!flag) ? num2 : (-num2));
            }
            return result;
        }

 

        public static float Repeat(float t, float length)
        {
            return t - Mathf.Floor(t / length) * length;
        }

        public static float PingPong(float t, float length)
        {
            t = Mathf.Repeat(t, length * 2f);
            return length - Mathf.Abs(t - length);
        }

        public static float InverseLerp(float a, float b, float value)
        {
            float result;
            if (a != b)
            {
                result = Mathf.Clamp01((value - a) / (b - a));
            }
            else
            {
                result = 0f;
            }
            return result;
        }

        public static float DeltaAngle(float current, float target)
        {
            float num = Mathf.Repeat(target - current, 360f);
            if (num > 180f)
            {
                num -= 360f;
            }
            return num;
        }

        internal static long RandomToLong(System.Random r)
        {
            byte[] array = new byte[8];
            r.NextBytes(array);
            return (long)(BitConverter.ToUInt64(array, 0) & 9223372036854775807uL);
        }
    }
}
