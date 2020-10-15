using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Arex
{
    public enum ARSessionState
    {
        Unsupported,
        Ready,
        Tracking
    }

    public interface IARSession
    {
        bool EnableSession { get; set; }
        IReadOnlyReactiveProperty<ARSessionState> State { get; }
        string LostReason { get; }
    }
}