using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.World.Teleport {
    public class Teleportation : MonoBehaviour
    {
        void Start()
        {
        
        }

        void Update()
        {
        
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.transform.tag == "Player")
                SceneManager.LoadScene("Level_1");
        }
    }
}
