using DG.Tweening;
using UnityEngine;
using vnc;

namespace Q19
{
    /// <summary>
    /// This is a sample class to illustrate how you 
    /// give input commands to the controller.
    /// 
    /// This class is supposed to be a learning example and
    /// it's not recommended to be used in a final product.
    /// </summary>
    public class Player : MonoBehaviour
    {
        public RetroController retroController; // the controller used
        public RetroControllerView retroView;
        public MouseLook mouseLook;             // mouse look
        public Camera playerView;            // the controller view
        public AnimationCurve BoostCurve;
        private float _moveSpeed;
        private float _acceleration;
        private float _boostTime = 0.3f;

        private void Awake()
        {
            mouseLook.Init(transform, playerView);
            mouseLook.SetCursorLock(true);
            retroController.OnTeleport += OnTeleport;
        }

        private void OnDestroy()
        {
            retroController.OnTeleport -= OnTeleport;
        }

        private void OnTeleport(Vector3 position, Quaternion rotation)
        {
            mouseLook.RealignRotation(rotation);
        }

        public virtual void FixedUpdate()
        {
            var accelerationSpeed = _acceleration > 0 ? BoostCurve.Evaluate(_acceleration) / _boostTime : 0;
            _moveSpeed = (_moveSpeed + accelerationSpeed) *
                         (1 - retroController.Profile.GroundFriction * Time.fixedDeltaTime);
            //retroController.Velocity = transform.forward * 5 * Time.fixedDeltaTime;
            retroController.Velocity = transform.forward * _moveSpeed * Time.fixedDeltaTime;
        }

        public virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                mouseLook.SetCursorLock(!mouseLook.lockCursor);
                retroController.updateController = !retroController.updateController;
            }

            //var fwd = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
            //var slow = - (Input.GetKey(KeyCode.S) ? 1 : 0);
            //var strafe = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
            //swim = (Input.GetKey(KeyCode.Space) ? 1 : 0) - (Input.GetKey(KeyCode.C) ? 1 : 0);
            var boost = Input.GetKeyDown(KeyCode.Space);
            //sprint = Input.GetKey(KeyCode.LeftShift);
            var DEBUGStop = Input.GetKeyDown(KeyCode.LeftControl);


            if (boost)
            {
                //_moveSpeed += 30f;
                DOTween.To(() => _acceleration, x => _acceleration = x, 1, _boostTime).OnComplete(() => _acceleration = 0);
                mouseLook.DoBoostKick();
            }

            if (DEBUGStop)
            {
                _moveSpeed = 0;
            }

            retroController.SetInput(0, 0, 0, false, false, false);

            mouseLook.LookRotation(transform, playerView);
            mouseLook.UpdateCursorLock();


            Time.timeScale = retroController.updateController ? 1 : 0;
        }
    }
}
