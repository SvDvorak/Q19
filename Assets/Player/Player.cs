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
        public Transform playerView;            // the controller view
        public AnimationCurve turnStrengthCurve;
        public float MoveSpeed;
        public Vector3 StrafeBoost;

        [Space, Tooltip("Switch to ducking and standing by pressing once instead of holding")]
        public bool toggleDucking;

        private bool _boost;

        private void Awake()
        {
            mouseLook.Init(transform, playerView);
            mouseLook.SetCursorLock(true);
        }

        public virtual void FixedUpdate()
        {
            retroController.Velocity =
                transform.forward * MoveSpeed * Time.fixedDeltaTime + StrafeBoost * Time.fixedDeltaTime;
            MoveSpeed *= 1 - retroController.Profile.GroundFriction * Time.fixedDeltaTime;
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
            _boost = Input.GetKeyDown(KeyCode.Space);
            //sprint = Input.GetKey(KeyCode.LeftShift);

            if (_boost)
            {
                MoveSpeed += 30f;
            }

            retroController.SetInput(0, 0, 0, false, false, false);

            mouseLook.LookRotation(transform, playerView);
            mouseLook.UpdateCursorLock();


            Time.timeScale = retroController.updateController ? 1 : 0;
        }
    }

}
