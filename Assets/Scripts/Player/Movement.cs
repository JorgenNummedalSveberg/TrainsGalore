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
    public LayerMask trainMask;


    private bool _drivingTrain;

    private bool _onTrain;
    private GameObject _train;

    public bool creative;

    private Vector3 _storedMovement = Vector3.zero;

    private bool _grounded;
    private bool _headTouching;

    private Vector3 _movementVector;
    
    private void TrainControls()
    {
        float trainSpeed = 10;
        Vector3 forward = new Vector3(1, 0, 0);
        _train.transform.Translate(forward * Time.deltaTime * trainSpeed);
        controller.Move(forward * Time.deltaTime * trainSpeed);
    }

    private void PlayerControls()
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

        _movementVector = transform.right * moveX + transform.forward * moveY;
        if (Input.GetKey(runKey))
        {
            _movementVector *= runningMultiplier;
        }

        if (creative)
        {
            _movementVector *= creativeMultiplier;
        }
    

        if (Input.GetButton("Jump") && (_grounded || creative))
        {
            _movementVector.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    
        if (creative)
        {
            _movementVector.y *= creativeMultiplier;
        }
    
        _velocity.y += calculatedGravity;
        if (lockedY)
        {
            _velocity.y = 0;
        }
    }

    public void StoreMovement(Vector3 movement)
    {
        _storedMovement += movement;
    }

    private void FixedUpdate()
    {
        controller.Move(_movementVector * movementSpeed * Time.deltaTime);

        controller.Move(_velocity * Time.deltaTime * Time.deltaTime + _storedMovement);
    }


    // Update is called once per frame
    void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(groundCheck.position, groundDistance, trainMask);
        if (colliders.Length > 0)
        {
            _train = colliders[0].gameObject;
        }
        else
        {
            _train = null;
        }

        _onTrain = _train != null;
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            _drivingTrain = !_drivingTrain;
        }

        if (!_onTrain)
        {
            _drivingTrain = false;
        }

        if (_drivingTrain)
        {
            TrainControls();
        }
        
        PlayerControls();
    }
}
