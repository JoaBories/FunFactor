using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateUi : MonoBehaviour
{
    [SerializeField] private GameObject inputToActive;

    private void Awake()
    {
        inputToActive.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inputToActive.SetActive(true);
        }
    }
}
