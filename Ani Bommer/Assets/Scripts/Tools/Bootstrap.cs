using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private string nameScene = "InputDataScene"; // scene nhập tên
    [SerializeField] private string lobbyScene = "Lobby";          // scene lobby
    [SerializeField] private Slider loadingSlider;                 // KÉO thanh loading vào đây

    private void Start()
    {
        StartCoroutine(StartFlow());
    }

    private IEnumerator StartFlow()
    {
        // Chưa có DataManager thì báo lỗi
        if (DataManager.Instance == null)
        {
            Debug.LogError("DataManager.Instance is null. Hãy chắc chắn có DataManager trong scene Loading.");
            yield break;
        }

        string targetScene;

        if (DataManager.Instance.HasSave())
        {
            DataManager.Instance.LoadPlayerData();
            Debug.Log($"Loaded player data: {Application.persistentDataPath}");
            targetScene = lobbyScene;
        }
        else
        {
            Debug.Log("No save data found. Loading name input scene.");
            targetScene = nameScene;
        }

        // Load scene đích async + cập nhật thanh loading
        yield return LoadSceneAsyncWithBar(targetScene);
    }

    private IEnumerator LoadSceneAsyncWithBar(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float fakeTimer = 0f;
        float minLoadTime = 1.5f; // tối thiểu 2 giây

        while (!op.isDone)
        {
            fakeTimer += Time.deltaTime;
            float realProgress = Mathf.Clamp01(op.progress / 0.9f);
            float progress = Mathf.Min(realProgress, fakeTimer / minLoadTime);

            loadingSlider.value = progress;

            if (progress >= 1f && fakeTimer >= minLoadTime)
                op.allowSceneActivation = true;

            yield return null;
        }
    }
}