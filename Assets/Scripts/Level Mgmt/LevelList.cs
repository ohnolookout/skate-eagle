using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level List", menuName = "ScriptableObjects/LevelList")]
public class LevelList : ScriptableObject
{
    public LevelNode[] levelNodes;

    void Awake()
    {
        for(int i = 0; i < levelNodes.Length; i++)
        {
            levelNodes[i].order = i;
        }
    }

}
