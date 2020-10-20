using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Arex.ARFoundation.Tests
{
    public static class ARPlaneDebugFlagAssersion
    {
        public static void ShouldBe(this ARPlaneDebugFlag flag, int expected)
        {
            Assert.That((int)flag, Is.EqualTo(expected));
        }
        public static void ShouldOn(this ARPlaneDebugFlag flag, ARPlaneDebugFlag v)
        {
            Assert.That((int)(flag & v), Is.Not.EqualTo(0));
        }
        public static void ShouldOff(this ARPlaneDebugFlag flag, ARPlaneDebugFlag v)
        {
            Assert.That((int)(~flag & v), Is.Not.EqualTo(0));
        }
    }

    public class ARFoundationPlaneTest
    {
        [Test]
        public void FlagTest()
        {
            var go = new GameObject();
            var plane = go.AddComponent<ARFoundationPlane>();
            plane.id = 0;
            Assert.That(plane, Is.Not.Null);
            plane.Flag.ShouldBe(0);

            {
                plane.SetFlag(ARPlaneDebugFlag.ShowInfo);
                plane.Flag.ShouldOn(ARPlaneDebugFlag.ShowInfo);
                plane.SetFlag(ARPlaneDebugFlag.ShowInfo); // twice
                plane.Flag.ShouldOn(ARPlaneDebugFlag.ShowInfo);
            }

            {
                plane.ClearFlag(ARPlaneDebugFlag.ShowInfo);
                plane.Flag.ShouldOff(ARPlaneDebugFlag.ShowInfo);
                plane.ClearFlag(ARPlaneDebugFlag.ShowInfo); // twice
                plane.Flag.ShouldOff(ARPlaneDebugFlag.ShowInfo);
            }

            {
                plane.SetFlag(ARPlaneDebugFlag.OutlineOnly);
                plane.Flag.ShouldOn(ARPlaneDebugFlag.OutlineOnly);
                plane.SetFlag(ARPlaneDebugFlag.OutlineOnly);
                plane.Flag.ShouldOn(ARPlaneDebugFlag.OutlineOnly);

                plane.ClearFlag(ARPlaneDebugFlag.OutlineOnly);
                plane.Flag.ShouldOff(ARPlaneDebugFlag.OutlineOnly);
                plane.ClearFlag(ARPlaneDebugFlag.OutlineOnly);
                plane.Flag.ShouldOff(ARPlaneDebugFlag.OutlineOnly);
            }

            {
                plane.SetFlag(ARPlaneDebugFlag.ShowInfo | ARPlaneDebugFlag.OutlineOnly);
                plane.Flag.ShouldOn(ARPlaneDebugFlag.ShowInfo);
                plane.Flag.ShouldOn(ARPlaneDebugFlag.OutlineOnly);
                plane.Flag.ShouldOn(ARPlaneDebugFlag.ShowInfo | ARPlaneDebugFlag.OutlineOnly);

                plane.SetFlag(ARPlaneDebugFlag.ShowInfo);
                plane.Flag.ShouldOn(ARPlaneDebugFlag.ShowInfo | ARPlaneDebugFlag.OutlineOnly);

                plane.ClearFlag(ARPlaneDebugFlag.ShowInfo | ARPlaneDebugFlag.OutlineOnly); // both clear
                plane.Flag.ShouldBe(0);
            }

            {
                plane.SetFlag(ARPlaneDebugFlag.ShowInfo);
                plane.ClearFlag(ARPlaneDebugFlag.ShowInfo | ARPlaneDebugFlag.OutlineOnly); // both clear
                plane.Flag.ShouldBe(0);

                plane.SetFlag(ARPlaneDebugFlag.ShowInfo);
                plane.ClearFlag(ARPlaneDebugFlag.OutlineOnly);
                plane.Flag.ShouldOn(ARPlaneDebugFlag.ShowInfo);
            }
        }
    }
}
