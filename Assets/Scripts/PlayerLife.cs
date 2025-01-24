using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    private Transform currentCheckpoint;

    private Animator _anim;
    private Rigidbody _rb;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
    }
    private void Death()
    {
        _anim.Play("Death");
        PlayerMovement.instance.enabled = false;
        _rb.velocity = Vector3.zero;
    }

    private void Respawn()
    {
        transform.position = currentCheckpoint.position;
        _anim.Play("Respawn");
    }

    private void EndRespawn()
    {
        PlayerMovement.instance.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            currentCheckpoint = other.transform;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Damage"))
        {
            Death();
        }
    }

}
