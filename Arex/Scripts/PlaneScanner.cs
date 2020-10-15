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
        public static System.Predicate<IARPlane> IsValidPlane = (plane) => !plane.subsumed();
    }

    public class PlaneScanner : MonoBehaviour
    {
        public Transform _camera;

        [SerializeField] float radius = 5.0f;

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
                int newPlanes,
                System.Predicate<IARPlane> condition)
        {
            var utc = new UniTaskCompletionSource();
            var currentPlanes = planeManager_.planes.Where(p => condition(p)).Count();

            planeManager_.Added.Subscribe(plane =>
            {
                Debug.Log($"<color=blue>new plane: {plane.ToString()}, {plane.subsumed()}</color>");
                if (planeManager_.planes.Where(p => condition(p)).Count() >= currentPlanes + newPlanes)
                {
                    Debug.Log($"found {newPlanes}");
                    utc.TrySetResult();
                }
            }).AddTo(dispoable);
            return utc.Task;
        }

        async public UniTask<PlaneScanResultArg> StartScan(
            int planes,
            int timeout,
            System.Predicate<IARPlane> condition,
            CancellationToken token)
        {
            planeManager_.EnableSearchPlanes = true;
            var disposable = new CompositeDisposable();

            var tasks = new List<UniTask>();

            // scan planes
            tasks.Add(scanPlanes(disposable, planes, condition)); // index: 0

            // timeout
            if (timeout > 0)
            {
                tasks.Add(UniTask.Delay(timeout * 1000, cancellationToken: token)); // index: 1
            }

            var index = await UniTask.WhenAny(tasks);

            planeManager_.EnableSearchPlanes = false;
            disposable.Dispose();

            var ret = new PlaneScanResultArg
            {
                planesFound = planeManager_.planes.Where(p => condition(p)).Count()
            };

            if (!token.IsCancellationRequested)
            {
                switch (index)
                {
                    case 0:
                        ret.result = PlaneScanResult.Found;
                        break;
                    case 1:
                        ret.result = PlaneScanResult.Timeout;
                        break;
                    default:
                        ret.result = PlaneScanResult.Error;
                        break;
                }
            } else
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

        public void Cancel()
        {
            if (this.tokenSource != null)
            {
                this.tokenSource.Cancel();
                this.tokenSource.Dispose();
                this.tokenSource = null;
            }
        }
    }
}