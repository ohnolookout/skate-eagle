using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    private Slider slider;
    private LiveRunManager logic;

    void Start()
    {
        slider = GetComponent<Slider>();
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
    }
    void Update()
    {
        slider.value = Mathf.Clamp01(logic.DistancePassed);
    }
}
