using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level List", menuName = "ScriptableObjects/LevelList")]
public class LevelList : ScriptableObject
{
    public List<LevelNode> levelNodes;


}
