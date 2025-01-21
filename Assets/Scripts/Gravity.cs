
using UnityEngine;

public class Gravity : MonoBehaviour
{
    public float gravityScale = 1.0f;
    public static float globalGravity = -20;

    Rigidbody _rb;

    void OnEnable()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
    }

    void FixedUpdate()
    {
        Vector3 gravity = globalGravity * gravityScale * Vector3.up;
        _rb.AddForce(gravity, ForceMode.Acceleration);
    }
}
