using System.Collections;
using UnityEngine;
using TMPro;

public class DamageNumberPopup : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private float floatDistance = 0.8f;
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private Vector3 randomJitter = new Vector3(0.1f, 0f, 0.1f);

    [Header("Colors")]
    [SerializeField] private Color damageColor = new Color(1f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color healColor = new Color(0.25f, 1f, 0.35f, 1f);

    private TMP_Text _tmpText;
    private Camera _cam;
    private Coroutine _routine;

    private void Awake()
    {
        _tmpText = GetComponentInChildren<TMP_Text>();
        _cam = Camera.main;
    }

    public void InitDamage(int amount)
    {
        if (amount <= 0) return;
        InitInternal("-" + amount, damageColor);
    }

    public void InitHeal(int amount)
    {
        if (amount <= 0) return;
        InitInternal("+" + amount, healColor);
    }

    private void InitInternal(string value, Color color)
    {
        if (_tmpText == null)
            _tmpText = GetComponentInChildren<TMP_Text>();

        if (_tmpText == null)
        {
            ObjectPoolingManager.Instance.Despawn(gameObject);
            return;
        }

        _tmpText.text = value;
        _tmpText.color = color;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        Vector3 startPos = transform.position;

        Vector3 jitter = new Vector3(
            Random.Range(-randomJitter.x, randomJitter.x),
            0f,
            Random.Range(-randomJitter.z, randomJitter.z)
        );

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.position = startPos + jitter + Vector3.up * (floatDistance * t);

            if (_tmpText != null)
            {
                var c = _tmpText.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                _tmpText.color = c;
            }

            yield return null;
        }

        ObjectPoolingManager.Instance.Despawn(gameObject);
    }

    private void LateUpdate()
    {
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        // Always face camera
        transform.forward = _cam.transform.forward;
    }
}