﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UniRx;
using Cysharp.Threading.Tasks;
using Arex;

namespace Arex.ARFoundation
{
    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARRaycastManager))]
    public class ARFoundationPlaneManager : MonoBehaviour, IARPlaneManager, IARPlaneRaycastManager
    {
        [SerializeField] bool inspectorDebug = false;

        ARPlaneManager planeManager;
        ARRaycastManager raycastManager;
        AROcclusionManager occlusionManager;
        int idCount = 0;

        ReactiveProperty<string> debugStatusProp = new ReactiveProperty<string>();
        int lastNumPlanes = 0;
        Subject<IARPlane> addedSubject = new Subject<IARPlane>();
        Subject<IARPlane> updatedSubject = new Subject<IARPlane>();
        Subject<IARPlane> removedSubject = new Subject<IARPlane>();

        Dictionary<ARPlane, IARPlane> planeDicts = new Dictionary<ARPlane, IARPlane>();

        void Awake()
        {
            ARServiceLocator.Instant.Register(this as IARPlaneManager);
            ARServiceLocator.Instant.Register(this as IARPlaneRaycastManager);

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
                // Assert.IsNotNull(plane, "No ARFoundationPlane");
                if(plane==null) {
                    plane = nativePlane.gameObject.AddComponent<ARFoundationPlane>();
                }
                if(inspectorDebug) {
                    // add debug component
                    if (nativePlane.gameObject.GetComponent<ARPlaneDebug>() == null)
                    {
                        nativePlane.gameObject.AddComponent<ARPlaneDebug>();
                    }
                }
                plane.id = idCount;
                idCount++;

                planeDicts[nativePlane] = plane; // hold by planeDicts first
                addedSubject.OnNext(plane);  // then notify
            }
        }

        void onUpdatedPlane(ARPlane nativePlane)
        {
            if (!planeDicts.ContainsKey(nativePlane))
            {
                return;
            }
            updatedSubject.OnNext(planeDicts[nativePlane]);
        }

        void onRemovedPlane(ARPlane nativePlane)
        {
            var plane = planeDicts[nativePlane];
            planeDicts.Remove(nativePlane);
            removedSubject.OnNext(plane);
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
                            onUpdatedPlane(nativePlane);
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
                // foreach (var trackable in planeManager.trackables)
                // {
                //     var nativePlane = trackable as ARPlane;
                //     onAddedPlane(nativePlane);
                // }
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
        IObservable<IARPlane> IARPlaneManager.Updated { get => updatedSubject; }
        IObservable<IARPlane> IARPlaneManager.Removed { get => removedSubject; }

        async Task<IARPlane> IARPlaneManager.SearchAnchoredPlane()
        {
            return null;
        }

        public IEnumerable<IARPlane> planes { get => planeDicts.Values; }

        // public void Remove(IARPlane plane) {
        //     if (planeManager.enabled)
        //     {
        //         Debug.LogError("failed to Remove");
        //         return;
        //     }
        //     if(planeDicts.ContainsValue(plane)) {
        //         var nativePlane = planeDicts.First(e => e.Value == plane).Key;
        //         planeDicts.Remove(nativePlane);
        //         Destroy(nativePlane.gameObject);
        //     }
        // }

        // public async UniTask RemoveAll()
        // {
        //     Debug.Log("RemoveAll");
        //     if (planeManager.enabled)
        //     {
        //         Debug.LogError("failed to RemoveAll");
        //         return;
        //     }
        //     // foreach(var nativePlane in planeDicts.Keys) {
        //     //     Debug.Log($"removed #{planeDicts[nativePlane].id}");
        //     //     Destroy(nativePlane.gameObject);
        //     // }
        //     var bakup = planeManager.currentDetectionMode;
        //     planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;
        //     await UniTask.DelayFrame(10);
        //     planeManager.requestedDetectionMode = bakup;
        //     await UniTask.DelayFrame(10);
        //     planeDicts.Clear();
        // }

        bool IARPlaneRaycastManager.Raycast(Vector2 pos, out RaycastHit hit)
        {
            hit = new RaycastHit();
            var hits = new List<ARRaycastHit>();
            if (!this.raycastManager.Raycast(pos, hits, TrackableType.Planes))
            {
                return false;
            }
            Debug.Log($"hits={hits.Count}");
            foreach (var h in hits)
            {
                Debug.Log($"{h}");
                var nativePlane = h.trackable as ARPlane;
                if (planeDicts.ContainsKey(nativePlane))
                {
                    hit.pose = h.pose;
                    hit.plane = planeDicts[nativePlane];
                    return true;
                }
            }
            return false;
        }
    }
}
