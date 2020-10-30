using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;

namespace Arex.Examples
{
    [Serializable]
    class GroudFindCondition
    {
        public float largeArea = 5.0f;
        public float midArea = 3.0f;
        public int numMidPlane = 3;
        public float radius = 3.0f;
    }

    [RequireComponent(typeof(PlaneScanner))]
    public class GroundFinder : MonoBehaviour
    {
        public Transform cameraTransform;
        public Button scanButton;
        // public Button clearButton;
        public DebugPanel debugPanel;
        public Toggle occulusionToggle;

        [SerializeField] GroudFindCondition condition;
        [SerializeField] Material groundMaterial;

        PlaneScanner planeScanner;
        IAROcclusionManager occlusionManager;
        IARPlane groundPlane = null;
        Material matBackup;
        GameObject offset;
        Transform trackablesTransform;

        void setGroundVisible(bool v)
        {
            if (groundPlane != null)
            {
                groundPlane.visible = v;
            }
        }

        void setGroundMaterial(Material material)
        {
            if (groundPlane != null)
            {
                if (material == null)
                {
                    groundPlane.material = matBackup;
                }
                else
                {
                    matBackup = groundPlane.material;
                    groundPlane.material = material;
                }
            }
        }

        void setGroundPlane(IARPlane plane)
        {
            if (this.groundPlane != plane)
            {
                if(this.groundPlane!=null) {
                    this.groundPlane.transform.SetParent(this.trackablesTransform);
                }
                this.groundPlane = plane;
                if (this.groundPlane != null)
                {
                    this.trackablesTransform = this.groundPlane.transform.parent;
                    this.offset.transform.position = Vector3.zero + new Vector3(0, 0.1f, 0);
                    this.offset.transform.SetParent(this.trackablesTransform);
                    this.groundPlane.transform.SetParent(this.offset.transform);
                    debugPanel.PrintLog(string.Format("<color=red>Ground is #{0} area={1} center={2}</color>",
                        this.groundPlane.id,
                        this.groundPlane.GetArea(),
                        this.groundPlane.center.ToString()));
                }
            }
        }

        void setAllPlaneVisible(bool v)
        {
            foreach (var plane in planeScanner.planeManager.planes)
            {
                plane.visible = v;
            }
        }

        async UniTask scanPlane(CancellationToken token)
        {
            var currentPosition = cameraTransform.position;
            setAllPlaneVisible(true);
            setGroundMaterial(null);
            setGroundPlane(null);

            var ret = await planeScanner.Scan(
                (planeManager) => {
                    var planes = new List<IARPlane>();
                    foreach(var plane in planeManager.ActivePlanes()) {
                        var area = plane.GetArea();
                        // Debug.Log($"scanning. #{plane.id} {area}");
                        if (area >= condition.largeArea)
                        {
                            planes.Add(plane);
                            return (true, $"Found a large plane ({plane.ToShortStrig()}, area={area})");
                        }
                        else if (area >= condition.midArea)
                        {
                            planes.Add(plane);
                        }
                    }
                    if (planes.Count >= condition.numMidPlane)
                        return (true, $"Found more {condition.numMidPlane} planes ({string.Join(",", planes.Select(p => p.ToShortStrig()))})");
                    else
                        return (false, "");
                },
                timeout: 10, token: token,
                waitFirstPlane: true, firstTimeout: 5);

            if (ret.result == PlaneScanResult.Found || ret.result == PlaneScanResult.Timeout)
            {
                debugPanel.PrintLog($"{ret.result.ToString()} ({ret.message})");
                var nearPlanes = planeScanner.planeManager.ActivePlanes().Where(p => (p.center - currentPosition).magnitude <= condition.radius);
                var orderedPlanes = nearPlanes.OrderByDescending(p => p.GetArea());
                var maxPlane = orderedPlanes.FirstOrDefault();
                var maxArea = maxPlane.GetArea();
                var lowestPlane = maxPlane;

                foreach (var plane in orderedPlanes)
                {
                    var square = plane.GetArea();
                    if (square < maxArea * 0.75f)
                    {
                        break;
                    }
                    if (plane.center.y < maxPlane.center.y)
                    {
                        lowestPlane = plane;
                    }
                }
                // select ground plane
                setGroundPlane(lowestPlane);

                // make rest of all invisible
                setAllPlaneVisible(false);
                setGroundVisible(true);
                setGroundMaterial(groundMaterial);
            }
            else
            {
                debugPanel.PrintLog($"{ret.result.ToString()} ({ret.message})");
                var actives = planeScanner.planeManager.ActivePlanes();
                var msg = string.Join(",", actives.Select(p => $"#{p.id}({p.GetArea()})"));
                debugPanel.PrintLog($"planes are {msg}");
                foreach(var plane in actives) {
                    // plane.SetFlag(ARPlaneDebugFlag.OutlineOnly);
                    plane.visible = false;
                }
            }
        }

        CancellationToken watchCameraMove(CompositeDisposable disposable)
        {
            var currentPosition = cameraTransform.position;
            var cts = new CancellationTokenSource();
            this.UpdateAsObservable().Subscribe(_ =>
            {
                var distance = (currentPosition - cameraTransform.position).magnitude;
                if (distance > condition.radius)
                {
                    debugPanel.PrintLog("Moved too far. cancel plane scan");
                    cts.Cancel();
                    cts.Dispose();
                    cts = null;
                }
                else if (distance > condition.radius/2.0)
                {
                    debugPanel.PrintLog($"Stay and scan your surroundings. moved {distance} meters");
                }
            }).AddTo(disposable);
            return cts.Token;
        }

        void Start()
        {
            scanButton.OnClickAsObservable().Subscribe(async _ => {
                debugPanel.PrintLog($"StartScan");

                scanButton.interactable = false;
                var disposable = new CompositeDisposable();
                await scanPlane(watchCameraMove(disposable));
                disposable.Dispose();
                scanButton.interactable = true;

                debugPanel.PrintLog($"Scan Ended");
            }).AddTo(this);

            // clearButton.OnClickAsObservable().Subscribe(async _ =>
            // {
            //     planeScanner.planeManager.RemoveAll();
            // }).AddTo(this);

            // this.UpdateAsObservable().Subscribe(_ => {
            //     clearButton.interactable = planeScanner.planeManager.planes.Count() > 0
            //         && !planeScanner.planeManager.EnableSearchPlanes;
            // }).AddTo(this);

            this.occlusionManager = ARServiceLocator.Instant.GetOcclusionManager();
            occulusionToggle.OnValueChangedAsObservable().Subscribe(v => {
                occlusionManager.EnableOcculusion = v;
            }).AddTo(this);
        }

        void Awake()
        {
            this.planeScanner = GetComponent<PlaneScanner>();
            this.offset = new GameObject("offset");
        }
    }
}
