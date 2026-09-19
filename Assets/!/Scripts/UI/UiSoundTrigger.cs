using UnityEngine;

public class UiSoundTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip _clickSound;

    public void PlayClickSound()
    {
        AudioManager.Instance.PlaySFX(_clickSound);
    }
}
