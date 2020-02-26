using Q19;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public static bool CurrentlyFiring;

    public Transform View;
    private Player _player;

    public void Start()
    {
        _player = GetComponent<Player>();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Time.timeScale = 0.5f;
            _player.LockedAimMove(false);
            CurrentlyFiring = true;
        }
        
        if(Input.GetMouseButtonUp(0))
        {
            Time.timeScale = 1;
            _player.LockedAimMove(true);
            CurrentlyFiring = false;
        }
    }

    public void FixedUpdate()
    {
        if(CurrentlyFiring)
            KillAimedAt();
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
