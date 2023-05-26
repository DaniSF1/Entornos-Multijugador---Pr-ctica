using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class healthBar : MonoBehaviour
{

    public Slider slider;
    
    public void SetMaxHealth(float health)
    {
        slider.maxValue = health;
        slider.value = health;

    }
    public void setHealth(float val)
    {
        slider.value = val;
    }
}