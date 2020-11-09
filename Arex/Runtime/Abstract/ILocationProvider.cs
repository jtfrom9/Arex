using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Arex
{
    public struct VectorD2
    {
        public double X;
        public double Y;
        public override string ToString() { return $"({X},{Y})"; }

        public static VectorD2 operator -(VectorD2 left, VectorD2 right)
        {
            return new VectorD2()
            {
                X = left.X - right.X,
                Y = left.Y - right.Y
            };
        }
        public static VectorD2 zero = new VectorD2();
    };

    public interface ILocationData
    {
        DateTime Timestamp { get; }
        float Latitude { get; }
        float Longitude { get; }

        bool HasAltitude { get; }
        float Altitude { get; }
        float Accuracy { get; }
        float Direction { get; }

        VectorD2 World { get; }
    }

    public interface ILocationDataProvider
    {
        // IObservable<ILocationData> Current { get; }
        IReadOnlyReactiveProperty<ILocationData> Current { get; }
        string Info { get; }
    }
}