using System.Collections;
using UnityEngine;

public class PulseOverTime : MonoBehaviour
{
    [SerializeField] private float _pulseDuration = 1f;
    [SerializeField] private float _pulseInterval = 1f;
    [SerializeField] private LeanTweenType _curveAnimation;
    [SerializeField] private float _scaleIntensity = 1.2f;

    private Coroutine _pulseRoutine;

    private void OnEnable()
    {
        _pulseRoutine = StartCoroutine(PulseRoutine());
    }

    private void OnDisable()
    {
        if (_pulseRoutine != null)
        {
            StopCoroutine(_pulseRoutine);
            _pulseRoutine = null;
        }

        LeanTween.cancel(gameObject);
    }

    private IEnumerator PulseRoutine()
    {
        var pulseStepDuration = _pulseDuration / 2f;
        var targetScale = Vector3.one * _scaleIntensity;

        while (true)
        {
            LeanTween.scale(gameObject, targetScale, pulseStepDuration).setEase(_curveAnimation);
            yield return new WaitForSeconds(pulseStepDuration);

            LeanTween.scale(gameObject, Vector3.one, pulseStepDuration).setEase(_curveAnimation);
            yield return new WaitForSeconds(pulseStepDuration);

            yield return new WaitForSeconds(_pulseInterval);
        }
    }
}
