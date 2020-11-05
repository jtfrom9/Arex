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
using Cysharp.Threading.Tasks;

namespace Arex
{
    public interface IARPlaneManager
    {
        IReadOnlyReactiveProperty<string> DebugStatus { get; }

        bool EnableSearchPlanes { get; set; }
        IObservable<IARPlane> Added { get; }
        IObservable<IARPlane> Updated { get; }
        IObservable<IARPlane> Removed { get; }
        Task<IARPlane> SearchAnchoredPlane();

        IEnumerable<IARPlane> planes { get; }
    }

    public struct RaycastHit
    {
        public Pose pose;
        public IARPlane plane;
    }

    public interface IARPlaneRaycastManager
    {
        bool Raycast(Vector2 pos, out RaycastHit hit);
    }

    public static class ARPlaneManagerExtensions
    {
        public static IEnumerable<IARPlane> ActivePlanes(this IARPlaneManager pm)
        {
            return pm.planes.Where(plane => !plane.subsumed());
        }
    }
}
