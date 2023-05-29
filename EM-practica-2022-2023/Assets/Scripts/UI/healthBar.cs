using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class healthBar : MonoBehaviour
{

    public Slider slider;                   //Slider que muestra la vida
    
    public void SetMaxHealth(float health)  //Vida maxima que representa el slider
    {
        slider.maxValue = health;
        slider.value = health;
    }
    public void setHealth(float val)        //Actualizamos el slider en funcion de la vida
    {
        slider.value = val;
    }
}
