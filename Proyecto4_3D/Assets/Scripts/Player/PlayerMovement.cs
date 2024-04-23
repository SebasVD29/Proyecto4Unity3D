using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform orientation;

    private Rigidbody rb;
    private bool activeGrapple = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (!activeGrapple)
        {
            MovePlayer();
        }
    }

    void MovePlayer()
    {
        Vector3 moveDirection = orientation.forward * Input.GetAxisRaw("Vertical") + orientation.right * Input.GetAxisRaw("Horizontal");
        rb.AddForce(moveDirection.normalized * moveSpeed, ForceMode.VelocityChange);
    }

    public void Grapple(Vector3 grapplePoint, float grappleForce)
    {
        activeGrapple = true;
        rb.velocity = CalculateGrappleVelocity(grapplePoint, grappleForce);
    }

    private Vector3 CalculateGrappleVelocity(Vector3 startPoint, float grappleForce)
    {
        Vector3 direction = startPoint - transform.position;
        float distance = direction.magnitude;
        direction.Normalize();
        float speed = Mathf.Sqrt(2 * grappleForce * distance);
        return direction * speed;
    }
}
