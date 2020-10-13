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
    public interface IARPlaneManager
    {
        IReadOnlyReactiveProperty<string> DebugStatus { get; }

        bool EnableSearchPlanes { get; set; }
        IObservable<IARPlane> Added { get; }
        IObservable<IARPlane> Removed { get; }
        Task<IARPlane> SearchAnchoredPlane();

        IEnumerable<IARPlane> planes { get; }
        bool EnableOcculusion { get; set; }
    }
}
