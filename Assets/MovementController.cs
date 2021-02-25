using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    private Vector3 _eulerAngleVelocity;
    private Rigidbody _rb;
    public float _torque;
    private Vector3 _playerInput;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }


    void Start()
    {
        //Set the axis the Rigidbody rotates in (100 in the y axis)
    }

    private void Update()
    {
        _playerInput.x = Input.GetAxis("Horizontal");
        _playerInput.y = Input.GetAxis("Vertical");
        _playerInput = Vector2.ClampMagnitude(_playerInput, 1f);
    }
    void FixedUpdate()
    {
        //float turn = Input.GetAxis("Horizontal");
        //float translation = Input.GetAxis("Vertical");
        //if (Mathf.Abs(turn + translation) > 0)
        if (_playerInput.sqrMagnitude > 0)
            MoveAndRotate(_playerInput.x, _playerInput.y);
    }

    private void MoveAndRotate(float turn, float translation)
    {
        _eulerAngleVelocity = new Vector3(0, turn * 50f, 0);

        Quaternion deltaRotation = Quaternion.Euler(_eulerAngleVelocity * Time.deltaTime);

        _rb.MoveRotation(_rb.rotation * deltaRotation);
        _rb.MovePosition(transform.position + transform.up * 0.05f * translation);
    }
}
