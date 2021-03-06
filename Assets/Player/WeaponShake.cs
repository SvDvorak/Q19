﻿using System;
using UnityEngine;

public class WeaponShake : MonoBehaviour
{
    public Transform[] Shakees;
    public AnimationCurve StartKick;
    public float StartKickTime;

    private Vector3[] _initialPositions;
    private float _startTime;
    private bool _shaking;
    private float _shakeStrength;
    private Quaternion _initialWeaponRot;
    private float _kickStrength;

    public void Start()
    {
        _initialPositions = new Vector3[Shakees.Length];
        for (var i = 0; i < Shakees.Length; i++)
        {
            var shakee = Shakees[i];
            _initialPositions[i] = shakee.localPosition;
        }

        _initialWeaponRot = Shakees[0].localRotation;
    }

    public void StartShake()
    {
        _startTime = Time.time;
        _shaking = true;
        _kickStrength = 1;
    }

    public void StopShake()
    {
        _shaking = false;
    }

    public void Update()
    {
        _shakeStrength = Mathf.Clamp(_shakeStrength + (_shaking ? Time.unscaledDeltaTime * 5 : -Time.unscaledDeltaTime * 4), 0, 1);

        var elapsed = Time.time - _startTime;
        var kickAmount = StartKick.Evaluate((elapsed / StartKickTime) * _kickStrength);
        var kick = Vector3.back * kickAmount * _kickStrength;
        var shake = new Vector3(Mathf.Sin(elapsed * 50), Mathf.Sin(elapsed * 9 + 1) * 0.3f, Mathf.Sin(elapsed * 30 + 3)) * 0.02f;
        for (var i = 0; i < Shakees.Length; i++)
        {
            var shakee = Shakees[i];
            shakee.localPosition = _initialPositions[i] + kick + shake * _shakeStrength;
        }

        var kickRot = Quaternion.Euler(-45f * kickAmount * _kickStrength * _kickStrength, 0, 0);
        Shakees[0].localRotation = _initialWeaponRot * kickRot;
    }

    public void SingleShortKick()
    {
        _startTime = Time.time;
        _kickStrength = 0.3f;
    }
}
