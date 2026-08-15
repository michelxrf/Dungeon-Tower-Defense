using UnityEngine;
using UnityEngine.SceneManagement;

public class KillPlayer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("GAME OVER. RESTARTING...");
        SceneManager.LoadScene("SampleScene");
    }
}
