using Q19;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public Transform View;
    private Player _player;

    public void Start()
    {
        _player = GetComponent<Player>();
    }

    public void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            Time.timeScale = 0.5f;
            _player.LockedAimMove(false);
            KillAimedAt();
        }
        else
        {
            _player.LockedAimMove(true);
            Time.timeScale = 1;
        }
    }

    private void KillAimedAt()
    {
        var ray = new Ray(View.position, View.forward * 1000);
        if (Physics.Raycast(ray, out var hit, float.MaxValue) && hit.transform.tag == "Hittable")
        {
            hit.transform.GetComponent<EnemyDeath>().Kill();
            _player.AddKillEnergy();
        }
    }
}
