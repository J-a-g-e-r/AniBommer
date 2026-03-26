using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    // Hàm này sẽ dùng để gọi từ Button
    [SerializeField] Animator transitionAnim;

    public void LoadScene(string sceneName)
    {
        // Bạn có thể thêm hiệu ứng Loading ở đây nếu muốn
        StartCoroutine(FadeOut(sceneName));
    }

    IEnumerator FadeOut(string sceneName)
    {
        transitionAnim.SetTrigger("Start"); // Kích hoạt animation
        yield return new WaitForSeconds(0.5f); // Chờ cho animation hoàn thành
        SceneManager.LoadSceneAsync(sceneName);

    }
}