using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    private Slider slider;
    private ILevelManager logic;

    void Start()
    {
        slider = GetComponent<Slider>();
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<ILevelManager>();
    }
    
    void Update()
    {
        //slider.value = Mathf.Clamp01(logic.DistancePassed); DO NOT NEED
    }
}
