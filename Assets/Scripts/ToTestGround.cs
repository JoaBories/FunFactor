using UnityEngine;
using UnityEngine.SceneManagement;

public class ToTestGround : MonoBehaviour
{
    [SerializeField] bool testGround = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (testGround)
            {
                SceneManager.LoadScene("TestGround");
            }
            else
            {
                SceneManager.LoadScene("Tuto");
            }
        }
    }
}
