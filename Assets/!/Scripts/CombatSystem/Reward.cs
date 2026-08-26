using UnityEngine;

public class Reward : MonoBehaviour
{
    [SerializeField] private int _money;

    private void OnDestroy()
    {
        LevelManager.Instance.AddMoney(_money);
    }
}
