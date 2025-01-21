using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerObj;
    [SerializeField] private Rigidbody _rb;

    [SerializeField] private float rotationSpeed;

    private void Update()
    {
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        if (PlayerMovement.instance.moveDir != Vector2.zero)
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, orientation.forward, rotationSpeed * Time.deltaTime);
        }
    }
}
