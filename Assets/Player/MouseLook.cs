using UnityEngine;

namespace Q19
{
    public class MouseLook : MonoBehaviour
    {
        public float maxTurnPerSecond;
        public float mouseSensitivity = 2f;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool lockCursor { get; private set; }
        public float deltaRotation { get; private set; }

        [Space]
        public bool cameraKick = true;
        public float cameraKickOffset;
        public float cameraKickoffsetWindow;
        public float cameraKickSpeed = 10;

        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CharacterPreviousTargetRot;
        private Quaternion m_CameraTargetRot;
        private float kick = 0;

        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
        }


        public void LookRotation(Transform character, Transform camera)
        {
            if (!lockCursor)
                return;

            kick -= (Time.deltaTime * cameraKickSpeed);
            kick = Mathf.Clamp(kick, 0, cameraKickOffset);

            float yRot = Input.GetAxis("Mouse X") * mouseSensitivity;
            float xRot = Input.GetAxis("Mouse Y") * mouseSensitivity;

            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            var tmp = m_CharacterTargetRot * Quaternion.Inverse(character.localRotation);
            var angle = tmp.eulerAngles.y % 360;
            deltaRotation = angle > 180 ? angle - 360 : angle;

            character.localRotation = m_CharacterTargetRot;
            camera.localRotation = m_CameraTargetRot;

            if (cameraKick)
            {
                var x = Mathf.Clamp(kick, 0, cameraKickOffset - cameraKickoffsetWindow);
                camera.localRotation *= Quaternion.Euler(-x, 0, 0);
            }

            m_CharacterPreviousTargetRot = m_CharacterTargetRot;
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
}

