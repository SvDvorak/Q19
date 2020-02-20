using UnityEngine;
using vnc;

namespace Assets.World.Teleport
{
    public class RetroControllerPortalTraveller : PortalTraveller
    {
        private RetroController _retroController;

        public override void Start()
        {
            base.Start();
            _retroController = GetComponent<RetroController>();
        }

        public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
        {
            _retroController.TeleportTo(pos, rot, false);
            Physics.SyncTransforms();
        }
    }
}