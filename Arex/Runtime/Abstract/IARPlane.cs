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

        Transform transform { get; }
        IARPlane subsumedBy { get; }
        bool visible { get; set; }
        Material material { get; set; }

        string ToShortStrig();
        float GetArea();

        void SetFlag(ARPlaneDebugFlag flag);
        void ClearFlag(ARPlaneDebugFlag flag);
        ARPlaneDebugFlag Flag { get; }

        Transform GetAnchor(Pose pose);
        void RemoveAnchor(Transform anchor);
    }

    public static class IARPlaneExtensions
    {
        public static bool subsumed(this IARPlane plane) {
            return plane.subsumedBy != null;
        }
    }
}
