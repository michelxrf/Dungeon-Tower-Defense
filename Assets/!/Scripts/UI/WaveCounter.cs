using TMPro;
using UnityEngine;

public class WaveCounter : MonoBehaviour
{
    [SerializeField] TMP_Text waves;

    private void Start()
    {
        UpdateCurrentWave();
        LevelManager.Instance.OnWaveEnded += UpdateCurrentWave;
    }

    private void UpdateCurrentWave()
    {
        waves.text = $"{LevelManager.Instance.GetCurrentWaveIndex() + 1} / {LevelManager.Instance.GetTotalWaves()}";
    }

}
