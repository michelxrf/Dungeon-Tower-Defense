using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public void LoadLevel(string levelName)
    {
        // Load the specified level
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
    }
}

