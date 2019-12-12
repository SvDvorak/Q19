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

        // input variables
        private float fwd;

        private float[] _rotationMemory = new float[20];


        public virtual void Update()
        {
            // Here the sample gets input from the player
            fwd = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
            //strafe = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
            //swim = (Input.GetKey(KeyCode.Space) ? 1 : 0) - (Input.GetKey(KeyCode.C) ? 1 : 0);
            //jump = Input.GetKeyDown(KeyCode.Space);
            //sprint = Input.GetKey(KeyCode.LeftShift);


            // these inputs are fed into the controller
            // this is the main entry point for the controller
            retroController.Velocity = transform.forward * retroController.Velocity.magnitude;
            retroController.SetInput(fwd, 0, 0, false, false, false);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                mouseLook.SetCursorLock(!mouseLook.lockCursor);
                retroController.updateController = !retroController.updateController;
            }

            var turnStrength = turnStrengthCurve.Evaluate(retroController.Velocity.magnitude / retroController.Profile.MaxGroundSpeed);
            //Debug.Log(turnStrength);

            mouseLook.LookRotation(transform, playerView, turnStrength);
            mouseLook.UpdateCursorLock();


            Time.timeScale = retroController.updateController ? 1 : 0;
        }
    }

}
