using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Arex
{
    public interface IARSession
    {
        bool EnableSession { get; set; }
        IReadOnlyReactiveProperty<string> DebugStatus { get; }
    }
}