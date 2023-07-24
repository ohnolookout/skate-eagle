using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class CurveTypeSettings : ScriptableObject
{
    string curveName;
    private CurveParameters[] parameters;

    //public string curveTypeName;
    public CurveTypeSettings()
    {
        parameters = CurveTypes.SmallRoller();

    }

    public CurveTypeSettings(CurveParameters[] parameters)
    {
        this.parameters = parameters;
    }


    public CurveParameters[] CurveParameters
    {
        get
        {
            return parameters;
        }
    }

}

