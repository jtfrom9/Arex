using System.Collections;
using System.Collections.Generic;
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

    public class PlaneScanSequencer : MonoBehaviour
    {
        public Transform _camera;

        [SerializeField] float radius = 5.0f;

        IARPlaneManager arEnv;
        IUserGuidelineController guidelineController;
        IARSession session;

        CancellationTokenSource tokenSource;

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        // async UniTask<bool> scanPlane(float scanRdius, CancellationToken token)
        // {
        //     var beginPosition = _camera.position;

        //     using (var disposable = new CompositeDisposable())
        //     {

        //         arEnv.Added.Subscribe(plane =>
        //         {

        //         }).AddTo(disposable);

        //         this.UpdateAsObservable().Subscribe(_ =>
        //         {
        //             if ((_camera.position - beginPosition).magnitude > scanRadius)
        //             {
        //                 // error
        //             }
        //             // if (ARSession.state != ARSessionState.SessionTracking)
        //             // {
        //             //     break;
        //             // }
        //         }).AddTo(disposable);
        //     }
        // }

        void OnDestroy()
        {
            if (this.tokenSource != null)
            {
                this.tokenSource.Cancel();
                this.tokenSource.Dispose();
            }
        }

        public void StartSequence(float scanRadius)
        {
            this.tokenSource = new CancellationTokenSource();

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