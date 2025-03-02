using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
[CreateAssetMenu(fileName = "Level List", menuName = "ScriptableObjects/LevelList")]
public class LevelList : ScriptableObject
{
    public LevelNode[] levelNodes;
    public Dictionary<string, LevelNode> levelNodeDict  = new();

    public void Build()
    {
        for(int i = 0; i < levelNodes.Length; i++)
        {
            levelNodes[i].order = i;
            if(i < levelNodes.Length - 1)
            {
                levelNodes[i].next = levelNodes[i + 1];
            }
            if(i > 0)
            {
                levelNodes[i].previous = levelNodes[i - 1];
            }
            levelNodeDict[levelNodes[i].levelUID] = levelNodes[i];
        }
    }

}
*/