using Q19;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public static bool CurrentlyFiring;

    public Transform View;
    public Animator WeaponAnimator;
    private Player _player;
    private WeaponShake _weaponShake;

    public void Start()
    {
        _player = GetComponent<Player>();
        _weaponShake = GetComponent<WeaponShake>();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _weaponShake.StartShake();
            Time.timeScale = _player.SlowMotionTimeScale;
            _player.LockedAimMove(false);
            WeaponAnimator.SetBool("IsFiring", true);
            CurrentlyFiring = true;
        }
        
        if(Input.GetMouseButtonUp(0))
        {
            _weaponShake.StopShake();
            Time.timeScale = 1;
            _player.LockedAimMove(true);
            WeaponAnimator.SetBool("IsFiring", false);
            CurrentlyFiring = false;
        }

        if(CurrentlyFiring)
            KillAimedAt();
    }

    private void KillAimedAt()
    {
        var ray = new Ray(View.position, View.forward * 1000);
        if (Physics.SphereCast(ray, 0.5f, out var hit, float.MaxValue) && hit.transform.tag == "Hittable")
        {
            hit.transform.GetComponent<EnemyDeath>().Kill();
            _player.AddKillEnergy();
        }
    }
}
