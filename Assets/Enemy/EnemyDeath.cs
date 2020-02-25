using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    public Transform EnemyModel;
    public ParticleSystem Gibs;

    public void Kill()
    {
        EnemyModel.DOScale(0, 0.2f);
        EnemyModel.DOLocalMoveY(1, 0.2f);
        transform.tag = "Untagged";
        Gibs.Play();
    }
}
