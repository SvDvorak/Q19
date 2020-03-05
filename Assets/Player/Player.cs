using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using vnc;

namespace Q19
{
    [Serializable]
    public class BoostSettings
    {
        public Transform BoostBar;
        public Transform MinBoostMeasure;
        public Transform BoostMeasureEnd;
        public float Energy;
        public AnimationCurve BoostCurve;
        public float ActivationCost;
        public float BoostTime;
        public float Acceleration;
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
        public bool ForceLockAimMove;
        public AnimationCurve SpeedAdjustment;
        public float NominalSpeed;
        public AnimationCurve FrictionCurve;
        public float SlowMotionTimeScale;
        public float FiringEnergyReduction;

        private WeaponShake _weaponShake;
        [DebugGUIGraph(min: 0, max: 50, r: 0, g: 1, b: 0, autoScale: true)]
        private float _moveSpeed;
        private float _acceleration;
        private Vector3 _moveForward;
        private bool _lockedAimMove = true;
        private Vector3 _boostMeasureInitialPos;
        private int _queuedBoosts;
        private bool _isCurrentlyBoosting;
        [DebugGUIGraph(min: 0, max: 2, r: 1, g: 0, b: 0, autoScale: true)]
        private float _speedAdjustment;

        private float _timeLastBoost;
        [DebugGUIGraph(min: 0, max: 1f, r: 0, g: 0, b: 1, autoScale: true)]
        private float _debugFriction;

        public void Awake()
        {
            mouseLook.Init(transform, playerView);
            mouseLook.SetCursorLock(true);
            retroController.OnTeleport += OnTeleport;
            LockedAimMove(true);
        }

        public void Start()
        {
            _boostMeasureInitialPos = Boost.MinBoostMeasure.localPosition;
            _weaponShake = GetComponent<WeaponShake>();
        }

        private void OnDestroy()
        {
            retroController.OnTeleport -= OnTeleport;
        }

        private void OnTeleport(Vector3 position, Quaternion rotation)
        {
            mouseLook.RealignRotation(rotation);
            //_moveForward = rotation * _moveForward;
        }

        public virtual void FixedUpdate()
        {
            _debugFriction = 1 - FrictionCurve.Evaluate(Time.time - _timeLastBoost);
            var friction = 1 - FrictionCurve.Evaluate(Time.time - _timeLastBoost) * Time.fixedDeltaTime;

            var accelerationSpeed = _isCurrentlyBoosting ? Boost.BoostCurve.Evaluate(_acceleration) * Boost.Acceleration : 0;
            _speedAdjustment = SpeedAdjustment.Evaluate(_moveSpeed / NominalSpeed);
            _moveSpeed = (_moveSpeed + accelerationSpeed * _speedAdjustment) * friction;
            //retroController.Velocity = transform.forward * 5 * Time.fixedDeltaTime;
            var moveDir = _lockedAimMove ? transform.forward : _moveForward;
            retroController.Velocity = moveDir * _moveSpeed * Time.fixedDeltaTime;

            var boostChange = Boost.IncreasePerSecond;
            if (Shooting.CurrentlyFiring)
                boostChange *= FiringEnergyReduction;
            ChangeEnergy(boostChange * Time.fixedDeltaTime);
        }

        public virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Application.isEditor)
                    mouseLook.SetCursorLock(!mouseLook.lockCursor);
                else
                    Application.Quit(0);
                //retroController.updateController = !retroController.updateController;
            }

            //var fwd = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
            //var slow = - (Input.GetKey(KeyCode.S) ? 1 : 0);
            //var strafe = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
            //swim = (Input.GetKey(KeyCode.Space) ? 1 : 0) - (Input.GetKey(KeyCode.C) ? 1 : 0);
            var boost = Input.GetKeyDown(KeyCode.Space);
            //sprint = Input.GetKey(KeyCode.LeftShift);
            var slowdown = Input.GetKey(KeyCode.LeftControl);

            if (boost)
            {
                _queuedBoosts += 1;
            }

            if (_queuedBoosts > 0 && !_isCurrentlyBoosting)
            {
                if(Boost.Energy >= Boost.ActivationCost)
                {
                    _isCurrentlyBoosting = true;
                    ChangeEnergy(-Boost.ActivationCost);
                    //_moveSpeed += 30f;
                    DOTween.To(() => _acceleration, x => _acceleration = x, 1, Boost.BoostTime).OnComplete(() =>
                    {
                        _isCurrentlyBoosting = false;
                        _acceleration = 0;
                    });
                    mouseLook.DoBoostKick();
                    _weaponShake.SingleShortKick();
                    _timeLastBoost = Time.time;
                }

                _queuedBoosts -= 1;
            }

            if (slowdown)
            {
                _moveSpeed *= 1 - 0.98f * Time.deltaTime;
            }

            Boost.BoostBar.localScale = new Vector3(1, 1, Boost.Energy);

            Boost.MinBoostMeasure.localPosition =
                Vector3.Lerp(_boostMeasureInitialPos, Boost.BoostMeasureEnd.localPosition, Boost.ActivationCost);

            retroController.SetInput(0, 0, 0, false, false, false);

            mouseLook.LookRotation(transform, playerView, _moveSpeed / 40);
            mouseLook.UpdateCursorLock();


            Time.timeScale = retroController.updateController ? Time.timeScale : 0;
        }

        private void ChangeEnergy(float amount)
        {
            Boost.Energy = Mathf.Clamp(Boost.Energy + amount, 0, 1);
        }

        public void AddKillEnergy()
        {
            ChangeEnergy(Boost.EnemyKillIncrease);
        }

        public void LockedAimMove(bool lockedAimMove)
        {
            if(!ForceLockAimMove)
            {
                _moveForward = transform.forward;
                _lockedAimMove = lockedAimMove;
            }
        }
    }
}
