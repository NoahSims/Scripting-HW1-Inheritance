using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TankController))]
public class Player : MonoBehaviour
{
    TankController _tankController;
    //[SerializeField] UIWriter _uiWriter;
    [SerializeField] Material _bodyMaterial;
    [SerializeField] Color _defaultBodyColor;

    /*
    [SerializeField] private int _treasureCount = 0;
    public int TreasureCount
    {
        get => _treasureCount;
        set
        {
            _treasureCount = value;
            _uiWriter.SetUITreasure(_treasureCount);
        }
    }
    */

    private void Awake()
    {
        _tankController = GetComponent<TankController>();
        _defaultBodyColor = _bodyMaterial.color;
    }

    
    void Start()
    {
        //_uiWriter.SetUITreasure(_treasureCount);
    }

    public void SetBodyColor(Color color)
    {
        _bodyMaterial.color = color;
    }

    public void ResetMaterial()
    {
        _bodyMaterial.color = _defaultBodyColor;
    }
}
