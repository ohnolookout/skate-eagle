using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarScript : MonoBehaviour
{
    private Slider slider;
    private LogicScript logic;

    void Start()
    {
        slider = GetComponent<Slider>();
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
    }
    void Update()
    {
        slider.value = Mathf.Clamp01(logic.DistancePassed);
    }
}
