using System;
using System.Collections;
using UnityEngine;

public abstract class TimedTask : MonoBehaviour
{
    public abstract IEnumerator StartTask();
}