using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Editor;
using Vector3 = UnityEngine.Vector3;

public class SwingingController : MonoBehaviour
{

    [Header("Input")] 
    
    public KeyCode swingKey = KeyCode.Mouse0;

    [Header("References")] 
    
    public LineRenderer lr;
    public Transform gunTip, cam, player;
    public LayerMask whatIsGrappleable;

    [Header("Swinging")] 
    
    public float maxSwingDistance = 25f;

    public float swingSpeedIncrease;
    private Vector3 swingPoint;
    private SpringJoint joint;
    private Vector3 currentGrapplePosition;

    [Header("Air Movement")] 
    public Transform orientation;

    public Rigidbody rigidbody;
    public float horizontalThrustForce;
    public float forwardThrustForce;
    public float extendCableSpeed;

    [Header("Prediction Hit")] 
    
    public RaycastHit predictionHit;
    public float predictionSphereCastRadius;
    public Transform predictionPoint;
    
    MovementController movementController;

    
    private void CheckForSwingPoints()
    {
        if (joint != null) return;

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward, out sphereCastHit, maxSwingDistance,
            whatIsGrappleable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward, out raycastHit, maxSwingDistance, whatIsGrappleable);

        Vector3 realHitPoint;

        if (raycastHit.point != Vector3.zero)
            realHitPoint = raycastHit.point;
        
        else if (sphereCastHit.point != Vector3.zero)
            realHitPoint = sphereCastHit.point;

        else
            realHitPoint = Vector3.zero;


        if (realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }
        else
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }
    
    private void StartSwing()
    {
        if (predictionHit.point == Vector3.zero) return;
        
        swingPoint = predictionHit.point;
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;

        lr.positionCount = 2;
        currentGrapplePosition = gunTip.position;
    }

    private void StopSwing()
    {
        lr.positionCount = 0;
        Destroy(joint);
    }

    private void DrawRope()
    {
        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);
        
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, swingPoint);
    }

    private void AirMovement()
    {
        if (Input.GetKey(KeyCode.D)) rigidbody.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
        if (Input.GetKey(KeyCode.A)) rigidbody.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);
        
        if (Input.GetKey(KeyCode.W)) rigidbody.AddForce(orientation.forward * horizontalThrustForce * Time.deltaTime);

        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 directionToPoint = swingPoint - transform.position;
            rigidbody.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

            float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            float extendedDistancecFromPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed;
            
            joint.maxDistance = extendedDistancecFromPoint * 0.8f;
            joint.minDistance = extendedDistancecFromPoint * 0.25f;
        }
    }

   
    
    void Start()
    {
        movementController = GetComponent<MovementController>(); 
    }

    void Update()
    {
        if (Input.GetKeyDown(swingKey))
        {
            StartSwing();
            if (joint)
            {
                movementController.moveSpeed += swingSpeedIncrease;
            } 
        }

        if (Input.GetKeyUp(swingKey))
        {
            StopSwing();
            if (joint)
            {
                movementController.moveSpeed -= swingSpeedIncrease;
            }
        }
        
        CheckForSwingPoints();
        
        if (joint != null) AirMovement();
    }

    private void LateUpdate()
    {
        DrawRope();
    }
}
