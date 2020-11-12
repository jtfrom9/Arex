using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.ARFoundation;
using UniRx;

namespace Arex.ARFoundation
{
    public class ArexARFoundationSession : MonoBehaviour, IARSession
    {
        ARSession session;
        ReactiveProperty<ARSessionState> stateProp = new ReactiveProperty<ARSessionState>(ARSessionState.Unsupported);

        bool IARSession.EnableSession
        {
            get => session.enabled;
            set => session.enabled = value;
        }
        IReadOnlyReactiveProperty<ARSessionState> IARSession.State { get => stateProp; }
        string IARSession.LostReason { get => ARSession.notTrackingReason.ToString(); }

        void Awake()
        {
            session = GetComponent<ARSession>();
            Assert.IsNotNull(session);
        }

        void Start()
        {
            Observable.FromEvent<ARSessionStateChangedEventArgs>(h => ARSession.stateChanged += h, h => ARSession.stateChanged -= h)
                .Subscribe(arg =>
                {
                    switch(arg.state) {
                        case UnityEngine.XR.ARFoundation.ARSessionState.Ready:
                        case UnityEngine.XR.ARFoundation.ARSessionState.SessionInitializing:
                            stateProp.Value = ARSessionState.Ready;
                            break;
                        case UnityEngine.XR.ARFoundation.ARSessionState.SessionTracking:
                            stateProp.Value = ARSessionState.Tracking;
                            break;
                        default:
                            stateProp.Value = ARSessionState.Unsupported;
                            break;
                    }
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
