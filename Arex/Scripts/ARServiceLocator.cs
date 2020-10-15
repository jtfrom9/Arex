using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arex
{
    public class ARServiceLocator
    {
        IARSession session;
        IARPlaneManager planeManager;
        IAROcclusionManager occlusionManager;

        static ARServiceLocator inst = null;

        public static ARServiceLocator Instant {
            get
            {
                if (inst == null)
                {
                    inst = new ARServiceLocator();
                }
                return inst;
            }
        }

        public void Register(IARSession session)
        {
            this.session = session;
        }

        public void Register(IARPlaneManager planeManager)
        {
            this.planeManager = planeManager;
        }

        public void Register(IAROcclusionManager occlusionManager)
        {
            this.occlusionManager = occlusionManager;
        }

        public IARSession GetSession()
        {
            return session;
        }

        public IARPlaneManager GetPlaneManager()
        {
            return planeManager;
        }

        public IAROcclusionManager GetOcclusionManager()
        {
            return occlusionManager;
        }
    }
}