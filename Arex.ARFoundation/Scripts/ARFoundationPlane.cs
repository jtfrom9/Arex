using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.Collections;

using Arex;

namespace Arex.ARFoundation
{
    [RequireComponent(typeof(ARPlane))]
    public class ARFoundationPlane : MonoBehaviour, IARPlane
    {
        ARPlane nativePlane;
        int _id;
        ARFoundationPlane subsumePlane;

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
