using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Character Config")]
public class CharacterConfig : ScriptableObject
{
    public string id;           
    public string displayName;
    public Sprite sprite;
    public GameObject prefab;   
    public Characters stats;    
}

