using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRespawn : Respawnable
{
    private EnemyDeath _enemyDeath;

    public void Start()
    {
        _enemyDeath = GetComponent<EnemyDeath>();
    }

    public override void Respawn()
    {
        _enemyDeath.EnemyModel.localScale = Vector3.one;
        _enemyDeath.EnemyModel.localPosition = Vector3.zero;
        _enemyDeath.transform.tag = "Hittable";
        Debug.Log("RESPAWN!");
    }
}
