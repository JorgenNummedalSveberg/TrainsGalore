using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private Vector3 _velocity;
    public float movementSpeed = 10;
    public float runningMultiplier = 2;
    public float creativeMultiplier = 10;
    public KeyCode runKey = KeyCode.LeftShift;
    public KeyCode creativeKey = KeyCode.V;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float gravity = -9.81f;
    public bool lockedY;
    public float jumpHeight;
    public CharacterController controller;
    
    public float groundDistance = 0.4f;
    public Transform groundCheck;
    public float roofDistance = 0.4f;
    public Transform roofCheck;
    public LayerMask groundMask;

    public bool creative;

    private bool _grounded;
    private bool _headTouching;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(creativeKey))
        {
            creative = !creative;
        }

        float calculatedGravity = creative ? 0 : gravity;
        
        _grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        _headTouching = Physics.CheckSphere(roofCheck.position, roofDistance, groundMask);
        
        if (Input.GetKey(crouchKey) || _grounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        } else if (creative || _headTouching && _velocity.y > 0)
        {
            _velocity.y = 0;
        }
        
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 movement = transform.right * moveX + transform.forward * moveY;
        if (Input.GetKey(runKey))
        {
            movement *= runningMultiplier;
        }

        if (creative)
        {
            movement *= creativeMultiplier;
        }
        controller.Move(movement * movementSpeed * Time.deltaTime);
        

        if (Input.GetButton("Jump") && (_grounded || creative))
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        if (creative)
        {
            _velocity.y *= creativeMultiplier;
        }
        
        _velocity.y += calculatedGravity * Time.deltaTime;
        if (lockedY)
        {
            _velocity.y = 0;
        }
        controller.Move(_velocity * Time.deltaTime);
    }
}
