using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class FloatingDamage : MonoBehaviour
{
    [SerializeField] private float _floatSpeed = 1f;
    [SerializeField] private float _delay = 1f;
    [SerializeField] private float _fadeDuration = 0.5f;

    [SerializeField] private TMP_Text _damageLabel;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Sets the damage value and starts the fading effect
    /// </summary>
    /// <param name="damage"></param>
    public void Init(int damage)
    {
        _damageLabel.text = damage.ToString();
        transform.localScale = Vector3.one / 2f;
        LeanTween.scale(gameObject, Vector3.one, _delay).setEasePunch();
        StartCoroutine(Fade());
    }

    private void Update()
    {
        // Move the damage text upwards
        transform.position += Vector3.up * _floatSpeed * Time.deltaTime;
    }

    /// <summary>
    /// // Fade out the text over time
    /// </summary>
    IEnumerator Fade()
    {
        yield return new WaitForSeconds(_delay);

        float elapsedTime = 0f;
        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;


            float fadeAmount = 1f - (elapsedTime / _fadeDuration);
            _canvasGroup.alpha = fadeAmount;
        }
        
        Destroy(gameObject);
    }
}
