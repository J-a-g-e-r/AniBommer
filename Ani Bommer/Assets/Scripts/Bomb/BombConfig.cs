using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Bomb Config")]
public class BombConfig : ScriptableObject
{
    public string id;            
    public string displayName;
    public GameObject prefab;    
}
