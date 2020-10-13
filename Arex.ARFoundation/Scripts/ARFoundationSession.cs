using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.ARFoundation;
using UniRx;

namespace Arex.ARFoundation
{
    public class ARFoundationSession : MonoBehaviour, IARSession
    {
        ARSession session;
        ReactiveProperty<string> debugStatusProp = new ReactiveProperty<string>();

        bool IARSession.EnableSession
        {
            get => session.enabled;
            set => session.enabled = value;
        }
        IReadOnlyReactiveProperty<string> IARSession.DebugStatus { get => debugStatusProp; }

        void Awake()
        {
            ARServiceLocator.Instant.Register(this);

            session = GetComponent<ARSession>();
            Assert.IsNotNull(session);
        }

        void Start()
        {
            Observable.FromEvent<ARSessionStateChangedEventArgs>(h => ARSession.stateChanged += h, h => ARSession.stateChanged -= h)
                .Subscribe(arg =>
                {
                    var msg = $"{arg.state.ToString()}, {ARSession.notTrackingReason.ToString()}";
                    Debug.Log($"<color=red>{msg}</color>");
                    debugStatusProp.Value = msg;
                }).AddTo(this);
        }

        public override string ToString()
        {
            return string.Format("ARFoundationSession: {0}, {1}",
                session.currentTrackingMode,
                session.frameRate ?? 0);
        }
    }
}
