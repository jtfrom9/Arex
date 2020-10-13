using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arex
{
    public class ARServiceLocator
    {
        IARSession session;
        IARPlaneManager planeManager;

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

        public IARSession GetSession()
        {
            return session;
        }

        public IARPlaneManager GetPlaneManager()
        {
            return planeManager;
        }
    }
}