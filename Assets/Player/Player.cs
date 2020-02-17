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

        [Space, Tooltip("Switch to ducking and standing by pressing once instead of holding")]
        public bool toggleDucking;

        private void Awake()
        {
            mouseLook.Init(transform, playerView);
            mouseLook.SetCursorLock(true);
        }


        public virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                mouseLook.SetCursorLock(!mouseLook.lockCursor);
                retroController.updateController = !retroController.updateController;
            }

            // Here the sample gets input from the player
            var fwd = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
            //var slow = - (Input.GetKey(KeyCode.S) ? 1 : 0);
            var strafe = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
            //swim = (Input.GetKey(KeyCode.Space) ? 1 : 0) - (Input.GetKey(KeyCode.C) ? 1 : 0);
            var boost = Input.GetKeyDown(KeyCode.Space);
            //sprint = Input.GetKey(KeyCode.LeftShift);

            var speed = 0f;
            var move = Vector3.zero;
            if (boost && (fwd != 0 || strafe != 0))
            {
                move = fwd * transform.forward + strafe * transform.right;
                speed = 0.5f;
            }

            // these inputs are fed into the controller
            // this is the main entry point for the controller
            //retroController.Velocity =
            //    transform.forward * retroController.Velocity.magnitude + transform.forward * speed;
            var mid = Vector3.Dot(transform.forward, retroController.Velocity.normalized);
            //Debug.Log($"Forward { transform.forward }   Velocity { retroController.Velocity.normalized }");
            var mag = retroController.Velocity.magnitude;
            retroController.Velocity = mag * Vector3.RotateTowards(retroController.Velocity.normalized, transform.forward, 1 * Time.deltaTime, 1);
            retroController.Velocity += move * 0.3f;
            retroController.SetInput(0, 0, 0, false, false, false);

            mouseLook.LookRotation(transform, playerView);
            mouseLook.UpdateCursorLock();


            Time.timeScale = retroController.updateController ? 1 : 0;
        }
    }

}
