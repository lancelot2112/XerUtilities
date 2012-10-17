using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XerUtilities.Rendering
{
    public static class MathLib
    {
        public static Random Random = new Random(1000);//new Random((int)DateTime.Now.Ticks);

        public static float NextRandom()
        {
            return (float)Random.NextDouble();
        }
        public static float RandomInRange(float min, float max)
        {
            return min + (float)Random.NextDouble() * (max - min);
        }
        public static float Sqrt(float value)
        {
            //unsafe
            //{
            //    uint i = *(uint*)&value;

            //    //adjust bias
            //    i += 127 << 23;
            //    //approximation of sqrt
            //    i >>= 1;

            //    return *(float*)i;
            //}

            return (float)Math.Sqrt(value);
        }

        public static float Acos(float value)
        {
            return (float)Math.Acos(value);
        }

        public static float Cos(float value)
        {
            return (float)Math.Cos(value);
        }

        public static float Sin(float value)
        {
            return (float)Math.Sin(value);
        }

        public static bool QuaternionEqual(Quaternion quat1, Quaternion quat2,float allowedError)
        {
            return Abs(quat1.X-quat2.X) < allowedError && Abs(quat1.Y-quat2.Y) < allowedError && Abs(quat1.Z-quat2.Z) < allowedError && Abs(quat1.W-quat2.W) < allowedError;
        }

        public static float YAngle(Quaternion quat)
        {
            float num = 2*quat.Y*quat.W;
            float den = 1-2*quat.Y*quat.Y;
            return (float)Math.Atan2(num, den);
        }

        public static float YAngle(Vector3 direction)
        {
            return (float)Math.Atan2(direction.Z, direction.X);
        }

        public static bool VectorsEqual(Vector3 vec1, Vector3 vec2, float allowedError)
        {
            return Abs(vec1.X - vec2.X) < allowedError && Abs(vec1.Y - vec2.Y) < allowedError && Abs(vec1.Z - vec2.Z) < allowedError;
        }

        /// <summary>
        /// Maps an angle to the set of reals [-pi,pi]
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
                radians += MathHelper.TwoPi;
            while (radians > MathHelper.Pi)
                radians -= MathHelper.TwoPi;
            return radians;
        }

        public static float WrapAngleToNearest(float ang1, float ang2)
        {
            float dist1 = MathHelper.Distance(ang1,ang2);
            float ang1wrap = (ang1 > 0 ? ang1 - MathHelper.TwoPi : ang1 + MathHelper.TwoPi);
            float dist2 = MathHelper.Distance(ang1wrap,ang2);

            return dist1 < dist2 ? ang1 : ang1wrap;
        }
        public static bool VectorsDirectionEqual(Vector3 vec1, Vector3 vec2, float allowedError)
        {
            float len1 = vec1.LengthSquared();
            float len2 = vec2.LengthSquared();
            float dot = Vector3.Dot(vec1, vec2);
            if(!(dot<0.0f))
                dot *= dot / (len1 * len2);
            return dot < 0.0f ? false : 1.0f - dot < allowedError;
        }

        public static bool CompareVectorElements(Vector3 vec1, Vector3 vec2, Func<float, float, bool> elementComparer )
        {
            bool ans = elementComparer(vec1.X, vec2.X);
            ans &= elementComparer(vec1.Y, vec2.Y);
            ans &= elementComparer(vec1.Z, vec2.Z);
            return ans;
        }

        public static float Abs(float val)
        {
            return val < 0 ? -val : val;
        }

        public static int Abs(int val)
        {
            return val < 0 ? -val : val;
        }

        public static float Sgn(float val)
        {
            return val < 0 ? -1f : 1f;
        }
    }
}
