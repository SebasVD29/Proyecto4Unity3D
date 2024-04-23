using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class SwingingDone : MonoBehaviour
{
    [Header("References")]
    public LineRenderer lr;
    public Transform gunTip, cam, player;
    public LayerMask whatIsGrappleable;
    public PlayerMovementAdvanced pm;
    Vector2 inputMove;

    [Header("Swinging")]
    private float maxSwingDistance = 25f;
    private Vector3 swingPoint;
    private SpringJoint joint;

    [Header("OdmGear")]
    public Transform orientation;
    public Rigidbody rb;
    public float horizontalThrustForce;
    public float forwardThrustForce;
    public float extendCableSpeed;

    [Header("Prediction")]
    public RaycastHit predictionHit;
    public float predictionSphereCastRadius;
    public Transform predictionPoint;

    [Header("Input")]
    [SerializeField] bool swingingInput = false;
    bool swingingRightInput = false;
    bool swingingLeftInput = false;
    bool swingingForwardInput = false;
    bool swingingShortenInput = false;
    bool swingingExtendInput = false;
    //public KeyCode swingKey = KeyCode.Mouse0;


    private void Update()
    {
        //if (swingingInput) 
        //{ 
        //    StartSwing();
        //}
        //if(!swingingInput)
        //{
        //    StopSwing();
        //}
        
        CheckForSwingPoints();

        if (joint != null)
        {
            OdmGearMovement();
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }
    public void Swinging(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            StartSwing();
            swingingInput = true;
        }
        if (context.canceled)
        {
            StopSwing();
            swingingInput = false;
        }
    }
    public void MoveSwinging(InputAction.CallbackContext context)
    {
        inputMove = context.ReadValue<Vector2>();
        if (inputMove.y > 0)
        {
            swingingForwardInput = true;
        }
        else
        {
            swingingForwardInput = false;
        }

        if (inputMove.x > 0)
        {
            swingingRightInput = true;
            swingingLeftInput = false;
        }
        else
        {
            swingingLeftInput = true;
            swingingRightInput = false;
        }
    }
    public void ShortCable(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            swingingShortenInput = true;
        }
        if (context.canceled)
        {
            swingingShortenInput = false;
        }
    }
    public void ExtendCable(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            swingingExtendInput = true;
        }
        if (context.canceled)
        {
            swingingExtendInput = false;
        }
    }
    private void CheckForSwingPoints()
    {
        if (joint != null) 
        { 
            return; 
        }

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward,
                            out sphereCastHit, maxSwingDistance, whatIsGrappleable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward,
                        out raycastHit, maxSwingDistance, whatIsGrappleable);

        Vector3 realHitPoint;

        // Option 1 - Direct Hit
        if (raycastHit.point != Vector3.zero)
        {
            realHitPoint = raycastHit.point;
        }
        // Option 2 - Indirect (predicted) Hit
        else if (sphereCastHit.point != Vector3.zero)
        {
            realHitPoint = sphereCastHit.point;
        }
        // Option 3 - Miss
        else
        {
            realHitPoint = Vector3.zero;
        }

        // realHitPoint found
        if (realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        }
        // realHitPoint not found
        else
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }
    private void StartSwing()
    {
        // return if predictionHit not found
        if (predictionHit.point == Vector3.zero) 
        { 
            return; 
        }
        // deactivate active grapple
        if (GetComponent<Grappling>() != null)
        {
            GetComponent<Grappling>().StopGrapple();
        }
        pm.ResetRestrictions();
        pm.swinging = true;

        //RaycastHit hit;
        //if (Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, whatIsGrappleable))
        //{

            swingPoint = predictionHit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = swingPoint;

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            // the distance grapple will try to keep from grapple point. 
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            // customize values as you like
            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
        //}
    }
    public void StopSwing()
    {
        pm.swinging = false;
        lr.positionCount = 0;
        Destroy(joint);
    }
    private void OdmGearMovement()
    {
        // right
        if (swingingRightInput) 
        { 
            rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime); 
        }
        // left
        if (swingingLeftInput)
        {
            rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);
        }
        // forward
        if (swingingForwardInput)
        {
            rb.AddForce(orientation.forward * horizontalThrustForce * Time.deltaTime);
        }
        // shorten cable
        if (swingingShortenInput)
        {
            Vector3 directionToPoint = swingPoint - transform.position;
            rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

            float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;
        }
        // extend cable
        if (swingingExtendInput)
        {
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed;

            joint.maxDistance = extendedDistanceFromPoint * 0.8f;
            joint.minDistance = extendedDistanceFromPoint * 0.25f;
        }
    }

    private Vector3 currentGrapplePosition;
    private void DrawRope()
    {
        // if not grappling, don't draw rope
        if (!joint) 
        { 
            return; 
        }

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }
}
