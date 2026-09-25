using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TomatoHud : MonoBehaviour
{
    [Header("Visual Meters")]
    public Image waterM;
    public Image fertiM;

    [Header("References")]
    public InteractionPlant plant;
    public TextMeshProUGUI percent;

    void Update()
    {
        percent.text = (Mathf.Round(plant.currentGrowth*10)/10) + "%";
        waterM.fillAmount = plant.currentWaterLevel / plant.maxWaterLevel;
        fertiM.fillAmount = plant.currentFertileLevel / plant.maxFertileLevel;
    }
}
