using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Level Data")]
public class LevelSO : ScriptableObject
{
    public LevelName levelName;
    public float knowledgeMultiplier = 1.0f;
    public WaveSO[] waves;
    public float speedMultiplierPerWave = 0.1f; // Speed increase per wave
}
