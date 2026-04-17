using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class AutoDestroyUI : MonoBehaviour
{
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private float fadeDuration = 0.5f;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        // Tự thêm CanvasGroup nếu chưa có
        _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        AutoDestroy(destroyCancellationToken).Forget();
    }

    private async UniTaskVoid AutoDestroy(CancellationToken ct)
    {
        await UniTask.WaitForSeconds(displayDuration, cancellationToken: ct);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            await UniTask.Yield(ct);
        }

        Destroy(gameObject);
    }
}