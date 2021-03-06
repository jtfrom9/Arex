﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Arex
{
    public interface IUserGuidelineController
    {
        void Show();
    }

    public enum PlaneScanResult
    {
        Found = 0,
        Timeout = 1,
        Error = 2,
        Cancel = 3
    }

    public struct PlaneScanResultArg
    {
        public PlaneScanResult result;
        public List<IARPlane> planes;
        public string message;
    }

    // public static class PlaneConditionMatcher
    // {
    //     public static Predicate<IARPlane> IsValidPlane = (plane) => !plane.subsumed();
    // }

    public class PlaneScanner : MonoBehaviour
    {
        IUserGuidelineController guidelineController;
        IARSession session;
        IARPlaneManager planeManager_;

        [Inject]
        void Inject(IARSession session, IARPlaneManager planeManager_)
        {
            this.session = session;
            this.planeManager_ = planeManager_;
        }

        public IARPlaneManager planeManager { get => planeManager_; }

        struct ScanResult {
            public string message;
            public List<IARPlane> planes;
        }

        UniTask<ScanResult> scanPlanes(Func<IARPlaneManager, List<IARPlane>, (bool, string)> scanCompletionConditionHandler,
            CompositeDisposable dispoable,
            CancellationToken token)
        {
            var utc = new UniTaskCompletionSource<ScanResult>();

            Action handler = () =>
            {
                if (token.IsCancellationRequested)
                {
                    utc.TrySetCanceled();
                }
                var tmp_planes = new List<IARPlane>();
                var (result, message) = scanCompletionConditionHandler(this.planeManager, tmp_planes);
                if (result)
                {
                    utc.TrySetResult(new ScanResult { message = message, planes = tmp_planes });
                }
            };
            planeManager_.Added.Subscribe(_=> handler()).AddTo(this);
            planeManager_.Updated.Where(p => !p.subsumed()).ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(plane => {
                Debug.Log($"Updated: {plane.ToShortStrig()}");
                handler();
            }).AddTo(dispoable);
            return utc.Task;
        }

        public UniTask<PlaneScanResultArg> Scan(
            int numPlanes,
            int timeout,
            CancellationToken token)
        {
            var currentPlanes = planeManager_.ActivePlanes().Count();
            return Scan((_, planes) =>
            {
                var diff = planeManager_.ActivePlanes().Count() - currentPlanes;
                if (diff >= numPlanes)
                {
                    return (true, $"Found {diff} planes");
                }
                else
                {
                    return (false, "");
                }
            },
            timeout, token);
        }

        async public UniTask<PlaneScanResultArg> Scan(
            Func<IARPlaneManager, List<IARPlane>, (bool, string)> scanCompletionConditionHandler,
            int timeout,
            CancellationToken token)
        {
            var disposable = new CompositeDisposable();
            var currentPlanes = planeManager_.ActivePlanes().Count();
            bool did_timeout = false;
            float errorTime = 0;
            Exception error = null;

            // begin search planes
            planeManager_.EnableSearchPlanes = true;

            // meature in session error
            this.UpdateAsObservable().Subscribe(_ =>
            {
                if (session.State.Value != ARSessionState.Tracking)
                    errorTime += Time.deltaTime;
            }).AddTo(disposable);

            // scan planes
            var scanTask = scanPlanes(scanCompletionConditionHandler, disposable, token);
            var scanResult = new ScanResult { planes = new List<IARPlane>() };
            try
            {
                if (timeout <= 0)
                {
                    scanResult = await scanTask;
                }
                else
                {
                    scanResult = await scanTask.Timeout(TimeSpan.FromSeconds(timeout));
                }
            }
            catch (TimeoutException)
            {
                did_timeout = true;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                error = e;
            }
            var ret = new PlaneScanResultArg()
            {
                message = scanResult.message,
                planes = scanResult.planes,
            };

            // end of search planes
            planeManager_.EnableSearchPlanes = false;
            disposable.Dispose();

            if (!token.IsCancellationRequested)
            {
                if (!did_timeout)
                {
                    ret.result = PlaneScanResult.Found;
                }
                else
                {
                    if ((float)timeout / 2 < errorTime || error != null)
                    {
                        ret.result = PlaneScanResult.Error;
                        ret.message = (error == null) ? $"Session Error ({session.LostReason})" : $"{error.ToString()}";
                    }
                    else
                    {
                        ret.result = PlaneScanResult.Timeout;
                        ret.message = $"{ret.planes.Count} planes matched";
                    }
                }
            }
            else
            {
                ret.result = PlaneScanResult.Cancel;
            }
            return ret;
        }
    }
}