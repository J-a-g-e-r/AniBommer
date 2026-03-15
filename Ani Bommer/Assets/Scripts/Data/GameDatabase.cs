using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Game Database")]
public class GameDatabase : ScriptableObject
{
    public CharacterConfig[] characters;
    public BombConfig[] bombs;

    public CharacterConfig GetCharacter(string id)
        => characters.FirstOrDefault(c => c.id == id);

    public BombConfig GetBomb(string id)
        => bombs.FirstOrDefault(b => b.id == id);
}
