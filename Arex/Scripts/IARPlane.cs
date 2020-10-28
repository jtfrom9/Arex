using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using UniRx;
using UniRx.Triggers;

namespace Arex
{
    [Flags]
    public enum ARPlaneDebugFlag {
        None = 0,
        ShowInfo = 1,
        OutlineOnly = 1 << 1,
    }

    public interface IARPlane
    {
        object internalObject { get; }
        int id { get; }

        Vector3 normal { get; }
        Vector3 center { get; }
        Vector2 extents { get; }
        Vector2 size { get; }
        NativeArray<Vector2> boundary { get; }

        IARPlane subsumedBy { get; }
        bool visible { get; set; }

        string ToShortStrig();

        void SetFlag(ARPlaneDebugFlag flag);
        void ClearFlag(ARPlaneDebugFlag flag);
        ARPlaneDebugFlag Flag { get; }
    }

    public static class IARPlaneExtensions
    {
        public static bool subsumed(this IARPlane plane) {
            return plane.subsumedBy != null;
        }

        public static float CalcArea(this IARPlane plane) {
            Vector2 origin = Vector2.zero;
            Vector2 prev = Vector2.zero;
            float ret = 0;
            foreach(var (p,i) in plane.boundary.Select((p,i)=>(p,i))) {
                if(i==0) {
                    origin = p;
                } else if(i==1) {
                    prev = p;
                } else {
                    var v1 = prev - origin;
                    var v2 = p - origin;
                    ret += Mathf.Abs(v1.x * v2.y - v1.y * v2.x) / 2.0f;
                }
            }
            return ret;
        }
    }
}
