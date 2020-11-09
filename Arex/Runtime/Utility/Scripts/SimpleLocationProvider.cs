using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Arex
{
    struct LocationData: ILocationData
    {
        public DateTime Timestamp { get; set; }
        public float Latitude { get => locationInfo.latitude; }
        public float Longitude { get => locationInfo.longitude; }

        public bool HasAltitude { get => false; }
        public float Altitude { get => 0; }
        public float Accuracy { get => locationInfo.horizontalAccuracy; }
        public float Direction { get; private set; }

        public VectorD2 World { get; private set; }

        LocationInfo locationInfo;

        static VectorD2 LatLonToMeters(double lat, double lon)
        {
            const int EarthRadius = 6378137; //no seams with globe example
            const double OriginShift = 2 * Math.PI * EarthRadius / 2;
            var posx = lon * OriginShift / 180;
            var posy = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
            posy = posy * OriginShift / 180;
            return new VectorD2 { X = posx, Y = posy };
        }

        public override string ToString()
        {
            return $"LocationData(lat={Latitude},lon={Longitude},accuracy={Accuracy},dir={Direction})";
        }

        public LocationData(LocationInfo locationInfo, float trueHeading)
        {
            this.locationInfo = locationInfo;
            this.Timestamp = DateTime.Now;
            this.Direction = trueHeading;
            this.World = LatLonToMeters(locationInfo.latitude, locationInfo.longitude);
        }
    }

    public class SimpleLocationProvider : MonoBehaviour, ILocationDataProvider
    {
        IReadOnlyReactiveProperty<ILocationData> ILocationDataProvider.Current { get => this.current; }
        string ILocationDataProvider.Info { get => "SimpleLocationProvider"; }

        ReactiveProperty<ILocationData> current = new ReactiveProperty<ILocationData>();

        [SerializeField] int updatePeriod = 1000;
        [SerializeField] [Range(0, 360)] float direction = 0;

        void Awake()
        {
            Input.compass.enabled = true;
            Input.location.Start();
        }

        void Start()
        {
#if UNITY_EDITOR
            float lastDirection = 0;
#endif
            Observable.Interval(TimeSpan.FromMilliseconds(updatePeriod)).Subscribe(_ =>
            {
#if UNITY_EDITOR
                if (lastDirection != direction)
                {
                    var locationData = new LocationData(new LocationInfo(), direction);
                    this.current.Value = locationData;
                    lastDirection = direction;
                }
#else
                // Debug.Log($"{Input.location.isEnabledByUser}, {Input.location.status}");
                if (Input.location.isEnabledByUser)
                {
                    if (Input.location.status == LocationServiceStatus.Running)
                    {
                        var locationData = new LocationData(Input.location.lastData, Input.compass.trueHeading);
                        this.current.Value = locationData;
                    }
                }
#endif
            }).AddTo(this);
        }
    }
}