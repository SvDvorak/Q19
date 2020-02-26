using System;
using Assets;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Q19
{
    [Serializable]
    public struct BoostViewSettings
    {
        public float Time;
        public AnimationCurve RotationKick;
        public AnimationCurve FOVKick;
    }

    public class MouseLook : MonoBehaviour
    {
        public float mouseSensitivity = 2f;
        public bool clampVerticalRotation = true;
        public Limits XLimits;
        public Limits FovLimits;

        [Header("Settings")]
        public BoostViewSettings Boost;
        public bool lockCursor { get; private set; }
        public float deltaRotation { get; private set; }

        private Quaternion _characterTargetRot;
        private Quaternion _cameraTargetRot;
        private float _kick;

        private float _tilt;
        private TweenerCore<float, float, FloatOptions> _kickTween;

        public void Init(Transform character, Camera camera)
        {
            _characterTargetRot = character.localRotation;
            _cameraTargetRot = camera.transform.localRotation;
        }

        public void LookRotation(Transform character, Camera camera, float speed)
        {
            if (!lockCursor)
                return;

            var yRot = Input.GetAxis("Mouse X") * mouseSensitivity;
            var xRot = Input.GetAxis("Mouse Y") * mouseSensitivity;

            _characterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            _cameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation)
                _cameraTargetRot = ClampRotationAroundXAxis(_cameraTargetRot);

            var tmp = _characterTargetRot * Quaternion.Inverse(character.localRotation);
            var angle = tmp.eulerAngles.y % 360;
            deltaRotation = angle > 180 ? angle - 360 : angle;
            // Cumulative moving average
            const int tiltSamples = 20;
            _tilt += (-deltaRotation - _tilt) / (tiltSamples + 1);

            character.localRotation = _characterTargetRot;
            var rotationKick = Quaternion.Euler(Boost.RotationKick.Evaluate(_kick), 0, 0);
            var turnTilt = Quaternion.Euler(0, 0, _tilt * 5);
            camera.transform.localRotation = _cameraTargetRot * rotationKick * turnTilt;
            camera.fieldOfView = FovLimits.Lerp(speed); //FovLimits.Min + Boost.FOVKick.Evaluate(_kick);
        }

        public void DoBoostKick()
        {
            //if (_kick <= 0.001)
            if(_kickTween != null && !_kickTween.IsComplete())
                _kickTween.Complete();
            _kickTween = DOTween.To(() => _kick, x => _kick = x, 1, Boost.Time).OnComplete(() => _kick = 0);
        }

        public void SetCursorLock(bool value)
        {
            lockCursor = value;
        }

        public void UpdateCursorLock()
        {
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void RealignRotation(Quaternion turn)
        {
            _characterTargetRot = turn;
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, XLimits.Min, XLimits.Max);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
    }
}
