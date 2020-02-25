using System;
using System.Collections.Generic;
using UnityEngine;

public class RespawnOnRestart : MonoBehaviour
{
    public List<Respawnable> Respawnables = new List<Respawnable>();

    public void Respawn()
    {
        foreach (var respawnable in Respawnables)
        {
            respawnable.Respawn();
        }
    }
}

[Serializable]
public abstract class Respawnable : MonoBehaviour
{
    public abstract void Respawn();
}
