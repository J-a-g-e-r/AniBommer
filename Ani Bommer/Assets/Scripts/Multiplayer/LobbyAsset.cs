using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class LobbyAsset : MonoBehaviour
{
    public static LobbyAsset Instance { get; private set; }

    [SerializeField] private GameDatabase gameDatabase;

    private void Awake()
    {
        Instance = this;
    }

    public Sprite GetSprite(string characterId)
    {
        CharacterConfig character = gameDatabase.GetCharacter(characterId);

        if (character == null)
        {
            Debug.LogWarning("Character not found: " + characterId);
            return null;
        }

        return character.sprite;
    }
}
