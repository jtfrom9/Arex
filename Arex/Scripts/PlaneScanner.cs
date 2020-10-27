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

    public static class PlaneConditionMatcher
    {
        public static Predicate<IARPlane> IsValidPlane = (plane) => !plane.subsumed();
    }

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

            planeManager_.Added.Subscribe(plane =>
            {
                Debug.Log($"<color=blue>new plane: {plane.ToShortStrig()}, {plane.subsumed()}</color>");
                var totalPlanes = planeManager_.planes.Where(p => condition(p)).Count();
                if (totalPlanes >= currentPlanes + newPlanes)
                {
                    Debug.Log($"found {newPlanes}");
                    utc.TrySetResult();
                }
            }).AddTo(dispoable);

            this.UpdateAsObservable().Subscribe(_ => {
                if (token.IsCancellationRequested)
                {
                    var totalPlanes = planeManager_.planes.Where(p => condition(p)).Count();
                    utc.TrySetCanceled();
                }
            }).AddTo(dispoable);

            return utc.Task;
        }

        async public UniTask<PlaneScanResultArg> StartScan(
            int newPlanes,
            int timeout,
            Predicate<IARPlane> condition,
            CancellationToken token,
            bool waitFirstPlane = false,
            int firstTimeout=10)
        {
            planeManager_.EnableSearchPlanes = true;
            var disposable = new CompositeDisposable();
            var currentPlanes = planeManager_.planes.Where(p => condition(p)).Count();

            if (waitFirstPlane && newPlanes > 1)
            {
                newPlanes--;
                try
                {
                    await scanPlanes(disposable, currentPlanes, 1, condition, token)
                        .Timeout(TimeSpan.FromSeconds(firstTimeout));
                }catch(TimeoutException)
                {
                    return new PlaneScanResultArg
                    {
                        result = PlaneScanResult.Timeout,
                        message = "No planes found"
                    };
                }catch(OperationCanceledException)
                {
                    return new PlaneScanResultArg
                    {
                        result = PlaneScanResult.Cancel,
                    };
                }
            }

            // meature in session error
            float errorTime = 0;
            this.UpdateAsObservable().Subscribe(_ =>
            {
                if (session.State.Value != ARSessionState.Tracking)
                    errorTime += Time.deltaTime;
            }).AddTo(disposable);

            // scan planes
            var scanTask = scanPlanes(disposable, currentPlanes, newPlanes, condition, token);

            bool did_timeout = false;
            Exception error = null;
            try
            {
                if (timeout <= 0)
                {
                    await scanTask;
                }
                else
                {
                    await scanTask.Timeout(TimeSpan.FromSeconds(timeout));
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

            var planesFound = planeManager_.planes.Where(p => condition(p)).Count() - currentPlanes;

            planeManager_.EnableSearchPlanes = false;
            disposable.Dispose();

            var ret = new PlaneScanResultArg
            {
                planesFound = planesFound
            };

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
                        ret.message = $"{planesFound}/{newPlanes} planes found.";
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