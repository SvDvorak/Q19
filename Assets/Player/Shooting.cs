using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public Transform View;

    void FixedUpdate()
    {
        var ray = new Ray(View.position, View.forward*1000);

        if (Input.GetMouseButton(0) && Physics.Raycast(ray, out var hit, float.MaxValue) && hit.transform.tag == "Hittable")
        {
            hit.transform.GetComponent<EnemyDeath>().Kill();
        }
    }
}
