﻿using System;
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
    public class ARFoundationPlaneManager : MonoBehaviour, IARPlaneManager
    {
        ARPlaneManager planeManager;
        ARRaycastManager raycastManager;
        AROcclusionManager occlusionManager;
        int idCount = 0;

        ReactiveProperty<string> debugStatusProp = new ReactiveProperty<string>();
        int lastNumPlanes = 0;
        Subject<IARPlane> addedSubject = new Subject<IARPlane>();
        Subject<IARPlane> removedSubject = new Subject<IARPlane>();

        Dictionary<ARPlane, IARPlane> planeDicts = new Dictionary<ARPlane, IARPlane>();

        void Awake()
        {
            ARServiceLocator.Instant.Register(this);

            planeManager = GetComponent<ARPlaneManager>();
            raycastManager = GetComponent<ARRaycastManager>();
            occlusionManager = GetComponentInChildren<AROcclusionManager>();
            StopSearchPlanes();
        }

        void updateDebugStatus()
        {
            var plist = planes.Where(p => !p.subsumed()).ToList();
            var count = plist.Count();
            debugStatusProp.Value =
                string.Format("Planes: {0}({1}) ({2})",
                    count,
                    count - lastNumPlanes,
                    string.Join(",", plist.Select(p => $"#{p.id.ToString()}")));
            lastNumPlanes = count;
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

        void onRemovedPlane(ARPlane nativePlane)
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
            if (planeManager != null)
            {
                Observable.FromEvent<ARPlanesChangedEventArgs>(h => planeManager.planesChanged += h, h => planeManager.planesChanged -= h)
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
                            onRemovedPlane(nativePlane);
                            removed = true;
                        }
                        if (added || removed)
                        {
                            Debug.Log($"<color=green>Planes: {planeDicts.Count} (new: {e.added.Count}, removed: {e.removed.Count})</color>");
                            updateDebugStatus();
                        }
                    }).AddTo(this);
            }
        }

        IReadOnlyReactiveProperty<string> IARPlaneManager.DebugStatus { get => debugStatusProp; }

        void StartSearchPlanes()
        {
            if (planeManager != null)
            {
                foreach (var trackable in planeManager.trackables)
                {
                    var nativePlane = trackable as ARPlane;
                    onAddedPlane(nativePlane);
                }
                planeManager.enabled = true;
            }
        }

        void StopSearchPlanes()
        {
            if (planeManager != null)
            {
                planeManager.enabled = false;
                // onRemovedAllPlane();
            }
        }

        bool IARPlaneManager.EnableSearchPlanes
        {
            get => (planeManager != null) ? planeManager.enabled : false;
            set
            {
                if (value)
                    StartSearchPlanes();
                else
                    StopSearchPlanes();
            }
        }

        IObservable<IARPlane> IARPlaneManager.Added { get => addedSubject; }
        IObservable<IARPlane> IARPlaneManager.Removed { get => removedSubject; }

        async Task<IARPlane> IARPlaneManager.SearchAnchoredPlane()
        {
            return null;
        }

        public IEnumerable<IARPlane> planes { get => planeDicts.Values; }

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
