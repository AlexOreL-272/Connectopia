using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    private ObjectsDatabaseSO database;

    [SerializeField] private InputManager inputManager;

    [SerializeField] private GameObject mouseIndicator;

    [SerializeField] private GridGraph _gridGraph;

    [SerializeField] private int selectedObject = 0;

    private Dictionary<Vector3Int, bool> _isBusy = new Dictionary<Vector3Int, bool>(); 
    
    private void Start()
    {
        inputManager.OnClicked += PlaceObject;
    }

    private void Update()
    {
        var mousePosition = inputManager.GetSelectedMapPosition();
        var gridPosition = _gridGraph.WorldToCell(mousePosition);
        mouseIndicator.transform.position = _gridGraph.CellToWorld(gridPosition);
    }
    
    public void PlaceObject()
    {
        var mousePosition = inputManager.GetSelectedMapPosition();
        var gridPosition = _gridGraph.WorldToCell(mousePosition);
        if (_gridGraph.IsCellFree(gridPosition))
        {
            return;
        }
        GameObject newObject = Instantiate(database.objectsData[selectedObject].Prefab);
        newObject.transform.position = _gridGraph.CellToWorld(gridPosition) + database.objectsData[selectedObject].Offset;
        _gridGraph.PlaceNewObject(gridPosition);
    }
}