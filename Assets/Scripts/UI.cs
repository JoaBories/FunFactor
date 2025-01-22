using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] private GameObject dashIcon;

    void Update()
    {
        dashIcon.SetActive(PlayerMovement.instance.canDash);
    }
}
