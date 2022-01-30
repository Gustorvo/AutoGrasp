using UnityEngine;

namespace SoftHand
{
    public static class VectorExtentions
    {
        /// <summary>
        /// Returns absolute values for all 3 components of the Vector3
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(
                Mathf.Abs(v.x),
                Mathf.Abs(v.y),
                Mathf.Abs(v.z)
                );
        }

        /// <summary>
        /// Returns square distance to target
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b">target Vector3</param>
        /// <returns></returns>
        public static float DistanceSquared(this Vector3 a, Vector3 b) => (a.x - b.x).Square() + (a.y - b.y).Square() + (a.z - b.z).Square();
        public static float Square(this float v) => v * v;

        public static float Sum(this Vector3 v)
        {
            return v.x + v.y + v.z;
        }
    }
}