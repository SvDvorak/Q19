using System;
using DG.Tweening;
using UnityEngine;

namespace Q19
{
    public class MouseLook : MonoBehaviour
    {
        public float mouseSensitivity = 2f;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;

        [Header("Settings")]
        public BoostSettings Boost;
        public bool lockCursor { get; private set; }
        public float deltaRotation { get; private set; }

        private Quaternion _characterTargetRot;
        private Quaternion _cameraTargetRot;
        private float _kick;
        private float _initialFov;
        [DebugGUIGraph(min: -10, max: 10, r: 0, g: 1, b: 0, autoScale: true)]
        private float _tilt;

        private int _tiltSamplesMax = 20;
        private int _tiltSamples;

        public void Awake()
        {
            DebugGUI.SetGraphProperties("tiltsamples", "Tilt Samples", -2, 2, 0, new Color(1, 0, 0), true);
        }

        public void Init(Transform character, Camera camera)
        {
            _characterTargetRot = character.localRotation;
            _cameraTargetRot = camera.transform.localRotation;
            _initialFov = camera.fieldOfView;
        }


        public void LookRotation(Transform character, Camera camera)
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
            CumulativeMovingAverage(-deltaRotation);//_tilt = (_tilt - deltaRotation * 0.1f) * 0.8f);

            character.localRotation = _characterTargetRot;
            var rotationKick = Quaternion.Euler(Boost.RotationKick.Evaluate(_kick), 0, 0);
            var b = _tiltSamples > _tiltSamplesMax;
            DebugGUI.Graph("tiltsamples", b ? 1 : 0);
            var turnTilt = Quaternion.Euler(0, 0, b ? _tilt * 5 : 0);
            camera.transform.localRotation = _cameraTargetRot * rotationKick * turnTilt;
            camera.fieldOfView = _initialFov + Boost.FOVKick.Evaluate(_kick);
        }

        void CumulativeMovingAverage(float tiltThisFrame)
        {
            _tiltSamples++;

            //This will calculate the MovingAverage AFTER the very first value of the MovingAverage
            if (_tiltSamples > _tiltSamplesMax)
            {
                _tilt += (tiltThisFrame - _tilt) / (_tiltSamplesMax + 1);
            }
            else
            {
                //NOTE: The MovingAverage will not have a value until at least "MovingAverageLength" values are known
                _tilt += tiltThisFrame;

                //This will calculate ONLY the very first value of the MovingAverage,
                if (_tiltSamples == _tiltSamplesMax)
                {
                    _tilt = _tilt / _tiltSamples;
                }
            }
        }

        public void DoBoostKick()
        {
            if (_kick <= 0.001)
                DOTween.To(() => _kick, x => _kick = x, 1, Boost.Time).OnComplete(() => _kick = 0);
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

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
    }

    [Serializable]
    public struct BoostSettings
    {
        public float Time;
        public AnimationCurve RotationKick;
        public AnimationCurve FOVKick;
    }
}
