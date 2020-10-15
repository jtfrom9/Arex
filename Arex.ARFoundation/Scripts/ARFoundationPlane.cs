using System;
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

        object IARPlane.internalObject { get => nativePlane; }
        public int id {
            get => this._id;
            set => this._id = value;
        }

        Vector3 IARPlane.normal { get => nativePlane.normal; }
        Vector3 IARPlane.center { get => nativePlane.center; }
        Vector2 IARPlane.extents { get => nativePlane.extents; }
        Vector2 IARPlane.size { get => nativePlane.size; }
        NativeArray<Vector2> IARPlane.boundary { get => nativePlane.boundary; }

        public IARPlane subsumedBy {
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

        void setShowInfoFlag()
        {
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

        public void SetDebug(ARPlaneDebugFlag flag)
        {
            if(id <0) {
                Debug.LogError("failed SetDebug.");
                return;
            }
            if((flag & ARPlaneDebugFlag.ShowInfo) != 0) {
                setShowInfoFlag();
            }
            this.flag = flag;
        }

        string vString()
        {
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

        void Awake()
        {
            this.nativePlane = GetComponent<ARPlane>();
        }
    }
}
