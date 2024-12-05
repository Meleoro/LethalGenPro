using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveComponent : MonoBehaviour, ICharacterComponent
{
    [Header("Main Parameters")] 
    [SerializeField] private float walkSpeed;
    [SerializeField] private float walkAcceleration;

    [Header("Run Parameters")] 
    [SerializeField] private bool canRun;
    [SerializeField] private float runSpeed;
    [SerializeField] private float runAcceleration;

    [Header("Private Infos")]
    private Vector3 inputDirection;
    private float currentSpeed;
    private float currentAcceleration;

    [Header("References")] 
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform wantedCamPos;
    private Controls controls;
    private CameraComponent cameraComponent;


    private void Awake()
    {
        controls = new Controls();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }


    private void Start()
    {
        cameraComponent = GetComponent<CameraComponent>();
        
        if (!rb)
        {
            Debug.LogError("Il manque la référence du rigidbody sur le script de movement les GDs");
        }
    }


    public void ComponentUpdate()
    {
        ManageInputs();
        MoveCharacter();
    }

    public void ComponentFixedUpdate()
    {
        
    }

    public void ComponentLateUpdate()
    {
        
    }


    // ACTUALISE THE MOVE DIRECTION INPUT AND THE CURRENT SPEED ACCORDING TO IF THE PLAYER WANTS TO RUN OR NOT
    private void ManageInputs()
    {
        // First we take the wanted move direction and transform it according to the camera rotation 
        inputDirection = controls.Player.MoveAxis.ReadValue<Vector2>();
        inputDirection = wantedCamPos.TransformDirection(new Vector3(inputDirection.x, 0, inputDirection.y));
        
        // Then we look if the player can and wants to run to actualise the current speed
        currentAcceleration = walkAcceleration;
        currentSpeed = walkSpeed;
        
        if (canRun)
        {
            if (controls.Player.Run.IsPressed())
            {
                currentAcceleration = runAcceleration;
                currentSpeed = runSpeed;
            }
        }
    }
    
    // MOVES THE THE PLAYER IN THE WORLD SPACE
    private void MoveCharacter()
    {
        rb.AddForce(inputDirection * (Time.deltaTime * currentAcceleration), ForceMode.Force);
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, currentSpeed);
        
        // We apply the feel to the camera according to our current speed
        cameraComponent.DoMoveFeel(rb.linearVelocity.magnitude / runSpeed);
        cameraComponent.ModifyFOV(rb.linearVelocity.magnitude / runSpeed);
    }
}
