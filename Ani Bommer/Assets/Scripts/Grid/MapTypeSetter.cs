using UnityEngine;

public class MapTypeSetter : MonoBehaviour
{
    [SerializeField] private MapType mapType = MapType.Pirate;

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGMByMapType(mapType);
        }
    }
}