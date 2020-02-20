using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempMove : MonoBehaviour
{
    public float Speed;
    private Rigidbody _rigidbody;

    public void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        //var pos = transform.position;
        //if(pos.z > 14)
        //    transform.position = new Vector3(pos.x, pos.y, 14);
        //_rigidbody.MovePosition(_rigidbody.position + transform.forward * Speed * Time.fixedDeltaTime);
        transform.position += transform.forward * Speed * Time.fixedDeltaTime;
    }
}
