using System.Collections.Generic;
using UnityEngine;
using System;
namespace Util{
    public static class Vector{
        public static Vector2 V32(Vector3 v3) {
            return new Vector2(v3.x, v3.y);
        }
        public static Vector3 V23(Vector2 v2, float z = 0) {
            return new Vector3(v2.x, v2.y, z);
        }
        public static Vector4 V24(Vector2 v2, float z = 0, float w = 0) {
            return new Vector4(v2.x, v2.y, z, w);
        }
        public static Vector3 Multiply(Vector3 v1, Vector3 v2) {
            return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        }
        public static Vector2 Multiply(Vector2 v1, Vector2 v2) {
            return new Vector2(v1.x * v2.x, v1.y * v2.y);
        }
        public static Vector3 Division(Vector3 v1, Vector3 v2) {
            return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
        }
        public static Vector2 Division(Vector2 v1, Vector2 v2) {
            return new Vector2(v1.x / v2.x, v1.y / v2.y);
        }
        public static Vector2 Do(Vector2 v1, Vector2 v2, Func<float, float, float> f) {
            return new Vector2(f(v1.x, v2.x), f(v1.y, v2.y));
        }
        public static Vector2 Do(Vector2 v1, Vector2 v2, Vector2 v3, Func<float, float, float, float> f) {
            return new Vector2(f(v1.x, v2.x, v3.x), f(v1.y, v2.y, v3.y));
        }
        public static Vector2 Do(Vector2 v1, Func<float, float> f) {
            return new Vector2(f(v1.x), f(v1.y));
        }
        public static Vector4 Do(Vector4 v1, Func<float, float> f) {
            return new Vector4(f(v1.x), f(v1.y), f(v1.z), f(v1.w));
        }
        public static Vector3 Do(Vector3 v1, Func<float, float> f) {
            return new Vector3(f(v1.x), f(v1.y), f(v1.z));
        }
        public static Vector2 DoComponent(Vector2 v, Vector2 component, Func<float, float> f) {
            Vector2 pureComponent = Do(component, _f => _f != 0 ? 1 : 0);
            return Do(v, pureComponent, (_f, c) => c == 0 ? _f : f(_f));
        }
        public static Vector3 DoComponent(Vector3 v, Vector3 component, Func<float, float> f) {
            var pureComponent = Do(component, (_f) => _f != 0 ? 1 : 0);
            return Do(v, pureComponent, (_f, c) => c == 0 ? _f : f(_f));
        }

        public static Vector2 ReplaceComponent(Vector2 v, Vector2 component , float f) {
            return DoComponent(v, component, (_f) => f);
        }
        public static Vector3 ReplaceComponent(Vector3 v, Vector3 component , float f) { 
            return DoComponent(v, component, (_f) => f);
        }
        public static void ReplaceComponent(ref Vector2 v, Vector2 component, float f) {
            v = ReplaceComponent(v, component, f);
        }
        public static Vector2 XComponent2 {
            get {
                return Vector2.right;
            }
        }
        public static Vector2 YComponent2 {
            get {
                return Vector2.up;
            }
        }
        public static Vector2 ChangeX(Vector2 v1, float value) {
            v1.x = value;
            return v1;
        }
        public static Vector2 ChangeY(Vector2 v1, float value) {
            v1.y = value;
            return v1;
        }
        /// <summary>
        /// Calculate a angle in normalized math angle axis.
        /// Counterclockwise is positive.
        /// </summary>
        /// <param name="v">the input vector</param>
        /// <returns>In [-180, 180]</returns>
        public static float Angle(Vector2 v) {
            float angleAbs = Vector2.Angle(Vector2.right, v);
            return (v.y < 0 ? -1 : 1) * angleAbs;
        }
        public static Vector2 NormalAxisRad(float radians) {
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }
        public static Vector2 NormalAxisDegree(float degree) {
            return NormalAxisRad(degree * Mathf.Deg2Rad);
        }
        public static Vector2 Sum(IEnumerable<Vector2> vs) {
            return new FunctionalCollection<Vector2>(vs).Foldl(Vector2.zero, (v1, v2) => v1 + v2);
        }
        public static Vector2 Average(IEnumerable<Vector2> vs) {
            return Sum(vs) / (float)(new FunctionalCollection<Vector2>(vs).Length);
        }
        public class V32Reference {
            private readonly Func<Vector3> getV3;
            private readonly Action<Vector3> setV3;

            public V32Reference(Func<Vector3> getV3, Action<Vector3> setV3)
            {
                this.getV3 = getV3;
                this.setV3 = setV3;
            }
            public Vector2 V2 {
                get {
                    return V32(getV3());
                }
                set {
                    setV3(V23(value, getV3().z));
                }
            }
        }

    }
}
