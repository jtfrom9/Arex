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
    public interface IAREnvironment
    {
        bool EnableSession { get; set; }
        IReadOnlyReactiveProperty<string> DebugStatus { get; }

        bool EnableSearchPlanes { get; set; }
        IObservable<IPlane> Added { get; }
        IObservable<IPlane> Removed { get; }
        IEnumerable<IPlane> planes { get; }
        Task<IPlane> SearchAnchoredPlane();
    }
}
