using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.ARFoundation;
using UniRx;

using Arex;

namespace Arex.ARFoundation
{
    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARRaycastManager))]
    public class ARFoundationPlaneManager : MonoBehaviour, IAREnvironment
    {
        ARSession session;
        ARPlaneManager planeManager;
        ARRaycastManager raycastManager;
        AROcclusionManager occlusionManager;
        int idCount = 0;

        ReactiveProperty<string> debugStatusProp = new ReactiveProperty<string>();
        Subject<IPlane> addedSubject = new Subject<IPlane>();
        Subject<IPlane> removedSubject = new Subject<IPlane>();

        Dictionary<ARPlane, IPlane> planeDicts = new Dictionary<ARPlane, IPlane>();

        void Awake()
        {
            session = FindObjectOfType<ARSession>();
            planeManager = GetComponent<ARPlaneManager>();
            raycastManager = GetComponent<ARRaycastManager>();
            occlusionManager = GetComponentInChildren<AROcclusionManager>();

            Assert.IsNotNull(session);
            Assert.IsNotNull(planeManager);
            Assert.IsNotNull(raycastManager);

            StopSearchPlanes();
        }

        void updateDebugStatus()
        {
            var plist = planes.Where(p => !p.subsumed()).ToList();
            debugStatusProp.Value =
                string.Format("Session: {0}, Planes: {1} ({2})",
                    ARSession.state.ToString(),
                    plist.Count(),
                    string.Join(",", plist.Select(p => $"#{p.id.ToString()}")));
        }

        void onAddedPlane(ARPlane nativePlane)
        {
            if (!planeDicts.ContainsKey(nativePlane))
            {
                var plane = nativePlane.gameObject.GetComponent<ARFoundationPlane>();
                Assert.IsNotNull(plane);
                plane.id = idCount;
                idCount++;
                addedSubject.OnNext(plane);
                planeDicts[nativePlane] = plane;
            }
        }

        void onRemovedSinglePlane(ARPlane nativePlane)
        {
            var plane = planeDicts[nativePlane];
            removedSubject.OnNext(plane);
            planeDicts.Remove(nativePlane);
        }

        // void onRemovedAllPlane()
        // {
        //     foreach (var plane in planes.Values)
        //     {
        //         removedSubject.OnNext(plane);
        //     }
        //     planes.Clear();
        // }

        void Start()
        {
            Observable.FromEvent<ARPlanesChangedEventArgs>(h => planeManager.planesChanged += h, h => planeManager.planesChanged -= h)
                // .Where(_ => search)
                .Subscribe(e =>
                {
                    var (added, updated, removed) = (false, false, false);

                    foreach (var nativePlane in e.added)
                    {
                        onAddedPlane(nativePlane);
                        added = true;
                    }
                    foreach (var nativePlane in e.updated)
                    {
                        updated = true;
                    }
                    foreach (var nativePlane in e.removed)
                    {
                        onRemovedSinglePlane(nativePlane);
                        removed = true;
                    }
                    if (added || removed)
                    {
                        Debug.Log($"<color=green>Planes: {planeDicts.Count} (new: {e.added.Count}, removed: {e.removed.Count})</color>");
                        updateDebugStatus();
                    }
                }).AddTo(this);

            Observable.FromEvent<ARSessionStateChangedEventArgs>(h => ARSession.stateChanged += h, h => ARSession.stateChanged -= h)
                .Subscribe(arg =>
                {
                    Debug.Log($"<color=red>Session: {arg.state.ToString()}</color>");
                    updateDebugStatus();
                }).AddTo(this);
        }

        bool IAREnvironment.EnableSession
        {
            get => session.enabled;
            set => session.enabled = value;
        }
        IReadOnlyReactiveProperty<string> IAREnvironment.DebugStatus { get => debugStatusProp; }

        void StartSearchPlanes()
        {
            foreach (var trackable in planeManager.trackables)
            {
                var nativePlane = trackable as ARPlane;
                onAddedPlane(nativePlane);
            }
            planeManager.enabled = true;
        }

        void StopSearchPlanes()
        {
            planeManager.enabled = false;
            // onRemovedAllPlane();
        }

        bool IAREnvironment.EnableSearchPlanes
        {
            get => planeManager.enabled;
            set
            {
                if (value)
                    StartSearchPlanes();
                else
                    StopSearchPlanes();
            }
        }

        IObservable<IPlane> IAREnvironment.Added { get => addedSubject; }
        IObservable<IPlane> IAREnvironment.Removed { get => removedSubject; }

        async Task<IPlane> IAREnvironment.SearchAnchoredPlane()
        {
            return null;
        }

        public IEnumerable<IPlane> planes { get => planeDicts.Values; }

        public bool EnableOcculusion {
            get => (occlusionManager != null) ? occlusionManager.enabled : false;
            set
            {
                if (occlusionManager != null)
                    occlusionManager.enabled = value;
            }
        }
    }
}
