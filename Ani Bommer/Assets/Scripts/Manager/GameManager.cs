using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private SkillListUI skillListUI;

    public void RegisterLocalPlayer(GameObject player)
    {
        Debug.Log("🔥 RegisterLocalPlayer CALLED");
        StartCoroutine(BindPlayerSkillsNextFrame(player));
        GameEvents.OnPlayerSpawned?.Invoke();
    }

    private IEnumerator BindPlayerSkillsNextFrame(GameObject player)
    {
        // Chờ 1 frame để các Start()/Init() của Player chạy xong
        yield return null;

        var playerSkills = player.GetComponent<PlayerSkills>();

        skillListUI.Bind(playerSkills);
    }
}
