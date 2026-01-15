using UnityEngine;
using System.Collections;
using NaughtyAttributes;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float _shakeDuration;
    [SerializeField] private float _shakeMagnitude;
    [SerializeField] private float _dampingSpeed;

    private Vector3 _initialPosition;
    private float _currentShakeTimer;
    private bool _isShaking;

    private void Awake()
    {
        _initialPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        MergeController.OnMergeOccurred += TriggerShake;
    }

    private void OnDisable()
    {
        MergeController.OnMergeOccurred -= TriggerShake;
    }

    [Button]
    private void TriggerShake()
    {
        _currentShakeTimer = _shakeDuration;
        if (!_isShaking)
        {
            StartCoroutine(ShakeRoutine());
        }
    }

    private IEnumerator ShakeRoutine()
    {
        _isShaking = true;

        while (_currentShakeTimer > 0)
        {
            var randomPoint = Random.insideUnitSphere * _shakeMagnitude;
            randomPoint.z = 0; 

            transform.localPosition = _initialPosition + randomPoint;
            _currentShakeTimer -= Time.deltaTime * _dampingSpeed;
            yield return null;
        }

        transform.localPosition = _initialPosition;
        _isShaking = false;
    }
}