using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class LevelValidator : MonoBehaviour
{
    public Level level;
    public bool validateLevel = false;
    public bool massValidate = false;
    public int levelGenCount = 10;

    // Start is called before the first frame update
    void Start()
    {
        

    }

    void OnValidate()
    {
        if (level == null)
        {
            Debug.Log("Must set level to validate.");
            return;
        }
        if (validateLevel)
        {
            ValidateSections();
            validateLevel = false;
            return;
        }
        if (massValidate)
        {
            MassValidate();
            massValidate = false;
        }
    }


    private void MassValidate()
    {
        int passCount = 0, failCount = 0;
        for (int i = 0; i < levelGenCount; i++)
        {
            if (ValidateSections())
            {
                passCount++;
            }
            else
            {
                failCount++;
            }
        }
        Debug.Log($"Tested {levelGenCount} sections. Pass: {passCount} Fail: {failCount}");
    }

    private bool ValidateSections()
    {
        foreach (LevelSection section in level.LevelSections)
        {
            if (!SectionCache.ValidateSection(section))
            {
                return false;
            };
        }
        return true;
    }


}
*/