using UnityEngine;

[CreateAssetMenu(fileName = "CardInGameSettings", menuName = "Settings/CardInGameSettings", order = 1)]
public class CardInGameSettings : ScriptableObject
{
    [SerializeField] private TroopSO[] _troops;
    public TroopSO[] Troops => _troops;
}
