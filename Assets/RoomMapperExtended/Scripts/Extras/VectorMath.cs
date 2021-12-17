using UnityEngine;

namespace VRoom
{
    public static class VectorMath
    {
        public static Vector3 RemapDistance(float iMin, float iMax, Vector3 a, Vector3 b)
        {
            float dist = Vector3.Distance(a, b);
            float t = Mathf.InverseLerp(iMin, iMax, dist);
            return Vector3.Lerp(a, b, t);
        }
        public static float DistanceSquared(Vector3 a, Vector3 b) => (a.x - b.x).Square() + (a.y - b.y).Square() + (a.z - b.z).Square();
        public static float Square(this float v) => v * v;
        public static float Min(float a, float b, float c, float d) => Min(Min(a, b), Min(c, d));
        public static float Min(float a, float b) => a < b ? a : b;
        public static Vector3 Closest(this Vector3 position, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            float toA = DistanceSquared(position, a);
            float toB = DistanceSquared(position, b);
            float toC = DistanceSquared(position, c);
            float toD = DistanceSquared(position, d);
            float closest = Min(toA, toB, toC, toD);
            return closest == toA ? a : closest == toB ? b : closest == toC ? c : d;
        }
    }
}