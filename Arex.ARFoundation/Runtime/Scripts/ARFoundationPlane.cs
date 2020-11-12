using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.Collections;
using UniRx;
using UniRx.Triggers;

namespace Arex.ARFoundation
{
    [RequireComponent(typeof(ARPlane))]
    public class ARFoundationPlane : MonoBehaviour, IARPlane
    {
        ARPlane nativePlane;
        int _id = -1;
        ARFoundationPlane subsumePlane;
        ARPlaneDebugFlag flag;
        IDisposable debugTextDisposable;
        MeshRenderer meshRenderer;
        float? areaCalculated;

        public ARFoundationPlaneManager manager { private get; set; }

        object IARPlane.internalObject { get => nativePlane; }
        public int id
        {
            get => this._id;
            set => this._id = value;
        }

        Vector3 IARPlane.normal { get => nativePlane.normal; }
        Vector3 IARPlane.center { get => nativePlane.center; }
        Vector2 IARPlane.extents { get => nativePlane.extents; }
        Vector2 IARPlane.size { get => nativePlane.size; }

        public IARPlane subsumedBy
        {
            get
            {
                if (nativePlane.subsumedBy == null)
                {
                    return null;
                }
                if (this.subsumePlane == null)
                {
                    this.subsumePlane = nativePlane.subsumedBy.gameObject.GetComponent<ARFoundationPlane>();
                }
                return this.subsumePlane;
            }
        }

        bool IARPlane.visible
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }

        Material IARPlane.material
        {
            get => meshRenderer.material;
            set => meshRenderer.material = value;
        }

        void setShowInfoFlag(bool enable)
        {
            if(nativePlane==null) {
                return;
            }
            if (debugTextDisposable != null)
            {
                return;
            }

            var go = new GameObject("debugText");
            var tm = go.AddComponent<TextMesh>();
            tm.characterSize = 0.1f;
            tm.color = Color.black;
            tm.text = $"#{id}";
            go.transform.position = nativePlane.center;
            go.transform.SetParent(this.gameObject.transform, worldPositionStays: true);

            this.debugTextDisposable = this.UpdateAsObservable().Subscribe(_ =>
            {
                go.transform.position = nativePlane.center;
                go.transform.LookAt(Camera.main.transform);
                go.transform.Rotate(0, 180, 0);
            });
        }

        void setOutlineOnly(bool enable)
        {
            if (nativePlane == null)
            {
                return;
            }
            // var mr = GetComponent<ARPlaneMeshVisualizer>();
            // mr.enabled = !v;
            var mr = GetComponent<MeshRenderer>();
            mr.material.color = new Color(0, 0, 0, 0);
        }

        bool selectedFlag(ARPlaneDebugFlag flag, bool enable)
        {
            if (enable)
                return (~this.flag & flag) != 0;
            else
                return (this.flag & flag) != 0;
        }

        void setFlag(ARPlaneDebugFlag flag, bool enable)
        {
            if (selectedFlag(flag & ARPlaneDebugFlag.ShowInfo, enable))
            {
                setShowInfoFlag(enable);
            }
            if (selectedFlag(flag & ARPlaneDebugFlag.OutlineOnly, enable))
            {
                setOutlineOnly(enable);
            }
        }

        public void SetFlag(ARPlaneDebugFlag flag)
        {
            setFlag(flag, true);
            this.flag |= flag;
        }

        public void ClearFlag(ARPlaneDebugFlag flag)
        {
            setFlag(flag, false);
            this.flag &= ~flag;
        }

        public ARPlaneDebugFlag Flag { get => this.flag; }

        Transform IARPlane.GetAnchor(Pose pose)
        {
            return this.manager.GetAnchor(nativePlane, pose);
        }

        void IARPlane.RemoveAnchor(Transform anchor)
        {
            this.manager.RemoveAnchor(anchor);
        }

        string vString()
        {
            if(nativePlane==null)
                return "";
            return (subsumedBy!=null) ? $"(Invalid:#{subsumedBy.id}) " : "";
        }

        public override string ToString()
        {
            return $"{vString()}Plane(#{_id},{nativePlane.trackableId})";
        }

        public string ToShortStrig()
        {
            return $"{vString()}IPlane(#{_id})";
        }

        public float GetArea()
        {
            if (!areaCalculated.HasValue)
            {
                areaCalculated = nativePlane.boundary.CalcArea();
            }
            return areaCalculated.Value;
        }

        void Awake()
        {
            this.nativePlane = GetComponent<ARPlane>();
            this.meshRenderer = GetComponent<MeshRenderer>();
            Observable.FromEvent<ARPlaneBoundaryChangedEventArgs>(h => this.nativePlane.boundaryChanged += h, h => this.nativePlane.boundaryChanged -= h)
                .Subscribe(arg => {
                    areaCalculated = null; // updated
                }).AddTo(this);
        }
    }

    public static class ARPlaneExtensions
    {
        public static float CalcArea(this NativeArray<Vector2> boundary)
        {
            Vector2 origin = Vector2.zero;
            Vector2 prev = Vector2.zero;
            float ret = 0;
            foreach (var (p, i) in boundary.Select((p, i) => (p, i)))
            {
                if (i == 0)
                {
                    origin = p;
                }
                else if (i == 1)
                {
                    prev = p;
                }
                else
                {
                    var v1 = prev - origin;
                    var v2 = p - origin;
                    ret += Mathf.Abs(v1.x * v2.y - v1.y * v2.x) / 2.0f;
                    prev = p;
                }
            }
            return ret;
        }
    }
}
