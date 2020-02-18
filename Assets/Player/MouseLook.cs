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

            float yRot = Input.GetAxis("Mouse X") * mouseSensitivity;
            float xRot = Input.GetAxis("Mouse Y") * mouseSensitivity;

            _characterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            _cameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation)
                _cameraTargetRot = ClampRotationAroundXAxis(_cameraTargetRot);

            var tmp = _characterTargetRot * Quaternion.Inverse(character.localRotation);
            var angle = tmp.eulerAngles.y % 360;
            deltaRotation = angle > 180 ? angle - 360 : angle;

            character.localRotation = _characterTargetRot;
            camera.transform.localRotation = _cameraTargetRot * Quaternion.Euler(Boost.RotationKick.Evaluate(_kick), 0, 0);
            camera.fieldOfView = _initialFov + Boost.FOVKick.Evaluate(_kick);
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
