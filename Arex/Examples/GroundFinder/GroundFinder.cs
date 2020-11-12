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
using Zenject;

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
        public Slider groundYSlider;

        [SerializeField] GroudFindCondition condition = default;
        [SerializeField] Material groundMaterial = default;

        PlaneScanner planeScanner;
        [Inject] IAROcclusionManager occlusionManager = default;
        IARPlane groundPlane = null;
        Material matBackup;
        GameObject groundBase;
        Transform trackablesTransform;
        float groundOffsetY = 0.1f;

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
                    this.groundBase.transform.position = Vector3.zero + new Vector3(0, 0.1f, 0);
                    this.groundBase.transform.SetParent(this.trackablesTransform);
                    this.groundPlane.transform.SetParent(this.groundBase.transform);
                    debugPanel.PrintLog(string.Format("<color=red>Ground is #{0} area={1} center={2}</color>",
                        this.groundPlane.id,
                        this.groundPlane.GetArea(),
                        this.groundPlane.center.ToString()));
                }
            } else if (plane != null)
            {
                debugPanel.PrintLog($"same plane (#{plane.id}) are ground");
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

            var ret = await planeScanner.Scan(
                (planeManager, planes) =>
                {
                    foreach (var plane in planeManager.ActivePlanes().Where(p => (p.center - currentPosition).magnitude <= condition.radius))
                    {
                        var area = plane.GetArea();
                        if (area >= condition.largeArea && plane!=this.groundPlane)
                        {
                            planes.Add(plane);
                            return (true, $"Found A New Large Plane ({plane.ToShortStrig()}, area={area}, center={plane.center})");
                        }
                        else if (area >= condition.midArea)
                        {
                            planes.Add(plane);
                        }
                    }
                    if (planes.Count >= condition.numMidPlane) {
                        var planeInfo = planes.Select(p => $"{p.ToShortStrig()}({p.GetArea()}");
                        return (true, $"Found more {condition.numMidPlane} planes ({string.Join(",", planeInfo)})");
                    } else {
                        return (false, "");
                    }
                },
                timeout: 10, token: token);

            if (ret.result == PlaneScanResult.Found)
            {
                debugPanel.PrintLog($"<color=red>{ret.result.ToString()} ({ret.message})</color>");
                // var nearPlanes = planeScanner.planeManager.ActivePlanes().Where(p => (p.center - currentPosition).magnitude <= condition.radius);
                // var orderedPlanes = nearPlanes.OrderByDescending(p => p.GetArea());
                var orderedPlanes = ret.planes.OrderByDescending(p => p.GetArea());
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
            }
            else
            {
                debugPanel.PrintLog($"<color=blue>{ret.result.ToString()} ({ret.message})</color>");
                var planes = planeScanner.planeManager.ActivePlanes().Where(p => p != this.groundPlane);
                var msg = string.Join(",", planes.Select(p => $"#{p.id}({p.GetArea()})"));
                if (groundPlane == null)
                {
                    debugPanel.PrintLog($"No ground choosen, planes are {msg}");
                } else {
                    debugPanel.PrintLog($"{groundPlane.ToShortStrig()} still choosen as ground, planes are {msg}");
                }
            }

            setAllPlaneVisible(false);
            setGroundVisible(true);
            setGroundMaterial(groundMaterial);
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

                Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(x => {
                    debugPanel.PrintLog("Scanning...");
                }).AddTo(disposable);
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

            occulusionToggle.OnValueChangedAsObservable().Subscribe(v => {
                occlusionManager.EnableOcculusion = v;
            }).AddTo(this);

            groundYSlider.value = groundOffsetY;
            groundYSlider.OnValueChangedAsObservable().Subscribe(v =>
            {
                groundOffsetY = v;
                if(groundBase!=null) {
                    var pos = groundBase.transform.position;
                    groundBase.transform.position = new Vector3(pos.x, groundOffsetY, pos.z);
                }
            }).AddTo(this);
        }

        void Awake()
        {
            this.planeScanner = GetComponent<PlaneScanner>();
            this.groundBase = new GameObject("offset");
        }
    }
}
