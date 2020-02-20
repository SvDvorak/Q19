using UnityEngine;
using vnc;

namespace Assets.World.Teleport
{
    public class RetroControllerPortalTraveller : PortalTraveller
    {
        private RetroController _retroController;
        private Rigidbody _rigidbody;

        public void Start()
        {
            _retroController = GetComponent<RetroController>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot)
        {
            //transform.position = pos;
            //_rigidbody.MovePosition(pos);
            _retroController.TeleportTo(pos, rot, false);
            Physics.SyncTransforms();
        }
    }
}