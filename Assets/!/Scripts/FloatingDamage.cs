using TMPro;
using UnityEngine;

public class FloatingDamage : MonoBehaviour
{
    [SerializeField] private float _floatSpeed = 1f;
    [SerializeField] private float _lifetime = 1f;
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private TMP_Text _damageLabel;

    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Init(int damage)
    {
        _damageLabel.text = damage.ToString();
    }

    private void Update()
    {
        // Move the damage text upwards
        transform.position += Vector3.up * _floatSpeed * Time.deltaTime;
        // Fade out the text over time
        float elapsedTime = Time.timeSinceLevelLoad;
        if (elapsedTime >= _lifetime)
        {
            float fadeAmount = 1f - ((elapsedTime - _lifetime) / _fadeDuration);
            _canvasGroup.alpha = fadeAmount;
            if (fadeAmount <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
