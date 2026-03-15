using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    // Hàm này sẽ dùng để gọi từ Button
    public void LoadLevel(string sceneName)
    {
        // Bạn có thể thêm hiệu ứng Loading ở đây nếu muốn
        SceneManager.LoadScene(sceneName);
    }
}