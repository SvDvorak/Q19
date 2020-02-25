using System;
using DG.Tweening;
using UnityEngine;
using vnc;

namespace Q19
{
    [Serializable]
    public class BoostSettings
    {
        public Transform BoostBar;
        public float Energy;
        public AnimationCurve BoostCurve;
        public float ActivationCost;
        public float IncreasePerSecond;
        public float EnemyKillIncrease;
    }

    public class Player : MonoBehaviour
    {
        public RetroController retroController; // the controller used
        public RetroControllerView retroView;
        public MouseLook mouseLook;             // mouse look
        public Camera playerView;            // the controller view
        public BoostSettings Boost;

        [DebugGUIGraph(min: 0, max: 50, r: 0, g: 1, b: 0, autoScale: true)]
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
            var accelerationSpeed = _acceleration > 0 ? Boost.BoostCurve.Evaluate(_acceleration) / _boostTime : 0;
            _moveSpeed = (_moveSpeed + accelerationSpeed) *
                         (1 - retroController.Profile.GroundFriction * Time.fixedDeltaTime);
            //retroController.Velocity = transform.forward * 5 * Time.fixedDeltaTime;
            retroController.Velocity = transform.forward * _moveSpeed * Time.fixedDeltaTime;

            AddEnergy(Boost.IncreasePerSecond * Time.fixedDeltaTime);
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


            if (boost && Boost.Energy >= Boost.ActivationCost)
            {
                Boost.Energy -= Boost.ActivationCost;
                //_moveSpeed += 30f;
                DOTween.To(() => _acceleration, x => _acceleration = x, 1, _boostTime).OnComplete(() => _acceleration = 0);
                mouseLook.DoBoostKick();
            }

            if (DEBUGStop)
            {
                _moveSpeed = 0;
            }

            Boost.BoostBar.localScale = new Vector3(1, 1, Boost.Energy);


            retroController.SetInput(0, 0, 0, false, false, false);

            mouseLook.LookRotation(transform, playerView, _moveSpeed / 40);
            mouseLook.UpdateCursorLock();


            Time.timeScale = retroController.updateController ? 1 : 0;
        }

        private void AddEnergy(float amount)
        {
            Boost.Energy = Mathf.Clamp(Boost.Energy + amount, 0, 1);
        }

        public void AddKillEnergy()
        {
            AddEnergy(Boost.EnemyKillIncrease);
        }
    }
}
