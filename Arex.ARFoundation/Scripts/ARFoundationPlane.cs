using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.Collections;

using Arex;

namespace Arex.ARFoundation
{
    [RequireComponent(typeof(ARPlane))]
    public class ARFoundationPlane : MonoBehaviour, IPlane
    {
        ARPlane nativePlane;
        int _id;
        ARFoundationPlane subsumePlane;

        object IPlane.internalObject { get => nativePlane; }
        public int id {
            get => this._id;
            set => this._id = value;
        }

        Vector3 IPlane.normal { get => nativePlane.normal; }
        Vector3 IPlane.center { get => nativePlane.center; }
        Vector2 IPlane.extents { get => nativePlane.extents; }
        Vector2 IPlane.size { get => nativePlane.size; }
        NativeArray<Vector2> IPlane.boundary { get => nativePlane.boundary; }

        public IPlane subsumedBy {
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

        bool IPlane.visible
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
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
