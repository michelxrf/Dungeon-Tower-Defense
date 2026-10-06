using UnityEngine;

public class UiSoundTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip _clickSound;

    public void PlayClickSound()
    {
        if (_clickSound == null) return;
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySFX(_clickSound);
    }
}
