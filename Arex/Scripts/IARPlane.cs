using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.ARFoundation;
using Unity.Collections;
using UniRx;
using UniRx.Triggers;

namespace Arex
{
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
    }

    public static class IARPlaneExtensions
    {
        public static bool subsumed(this IARPlane plane) {
            return plane.subsumedBy != null;
        }
    }
}
