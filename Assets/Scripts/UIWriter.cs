using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWriter : MonoBehaviour
{
    [SerializeField] Text _healthText;
    [SerializeField] Text _treasureText;

    public void SetUIHealth(int value)
    {
        _healthText.text = "Health = " + value;
    }

    public void SetUITreasure(int value)
    {
        _treasureText.text = "Treasure = " + value;
    }
}
