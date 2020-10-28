using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;

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
        public int planesFound;
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

        CancellationTokenSource tokenSource;

        public IARPlaneManager planeManager { get => planeManager_; }

        void Start()
        {
            this.session = ARServiceLocator.Instant.GetSession();
            this.planeManager_ = ARServiceLocator.Instant.GetPlaneManager();
        }

        UniTask scanPlanes(CompositeDisposable dispoable,
                int currentPlanes,
                int newPlanes,
                Predicate<IARPlane> condition,
                CancellationToken token)
        {
            var utc = new UniTaskCompletionSource();

            Action<IARPlane> handler = (plane) =>
            {
                if (token.IsCancellationRequested)
                {
                    utc.TrySetCanceled();
                }
                var totalPlanes = planeManager_.planes.Where(p => condition(p)).Count();
                if (totalPlanes >= currentPlanes + newPlanes)
                {
                    utc.TrySetResult();
                }
            };
            planeManager_.Added.Subscribe(handler).AddTo(dispoable);
            planeManager_.Updated.Subscribe(handler).AddTo(dispoable);

            return utc.Task;
        }

        UniTask<string> scanPlanes(Func<IARPlaneManager, (bool, string)> condition,
                CompositeDisposable dispoable,
                CancellationToken token)
        {
            var utc = new UniTaskCompletionSource<string>();

            Action<IARPlane> handler = (plane) =>
            {
                if (token.IsCancellationRequested)
                {
                    utc.TrySetCanceled();
                }
                var (result, message) = condition(this.planeManager);
                if (result)
                {
                    utc.TrySetResult(message);
                }
            };
            planeManager_.Added.Subscribe(handler).AddTo(dispoable);
            planeManager_.Updated.Subscribe(handler).AddTo(dispoable);
            return utc.Task;
        }

        public UniTask<PlaneScanResultArg> Scan(
            int numPlanes,
            int timeout,
            CancellationToken token,
            bool waitFirstPlane = false,
            int firstTimeout = 10)
        {
            var currentPlanes = planeManager_.ActivePlanes().Count();
            return Scan(_ =>
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
            timeout, token, waitFirstPlane, firstTimeout);
        }

        async public UniTask<PlaneScanResultArg> Scan(
            Func<IARPlaneManager, (bool, string)> condition,
            int timeout,
            CancellationToken token,
            bool waitFirstPlane = false,
            int firstTimeout = 10)
        {
            planeManager_.EnableSearchPlanes = true;
            var disposable = new CompositeDisposable();
            var currentPlanes = planeManager_.ActivePlanes().Count();
            var ret = new PlaneScanResultArg();
            bool did_timeout = false;
            float errorTime = 0;
            Exception error = null;

            if (waitFirstPlane)
            {
                try
                {
                    var task = scanPlanes(_ =>
                    {
                        var result = planeManager_.planes.Where(plane => !plane.subsumed()).Count() > currentPlanes;
                        return (result, "");
                    }, disposable, token);
                    await task.Timeout(TimeSpan.FromSeconds(firstTimeout));
                }
                catch (TimeoutException)
                {
                    ret.result = PlaneScanResult.Timeout;
                    ret.message = "No planes found";
                    did_timeout = true;
                    goto error;
                }
                catch (OperationCanceledException)
                {
                    ret.result = PlaneScanResult.Cancel;
                    goto error;
                }
            }

            // meature in session error
            this.UpdateAsObservable().Subscribe(_ =>
            {
                if (session.State.Value != ARSessionState.Tracking)
                    errorTime += Time.deltaTime;
            }).AddTo(disposable);

            // scan planes
            var scanTask = scanPlanes(condition, disposable, token);

            string message = null;
            try
            {
                if (timeout <= 0)
                {
                    message = await scanTask;
                }
                else
                {
                    message = await scanTask.Timeout(TimeSpan.FromSeconds(timeout));
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
            ret.planesFound = planeManager_.planes.Where(p => !p.subsumed()).Count() - currentPlanes;
            ret.message = message;

        error:
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
                        ret.message = $"{ret.planesFound} planes found.";
                    }
                }
            }
            else
            {
                ret.result = PlaneScanResult.Cancel;
            }
            return ret;
        }

        void OnDestroy()
        {
            if (this.tokenSource != null)
            {
                this.tokenSource.Cancel();
                this.tokenSource.Dispose();
            }
        }
    }
}