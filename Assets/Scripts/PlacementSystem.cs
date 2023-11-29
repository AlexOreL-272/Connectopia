using System;
using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    private ObjectsDatabaseSO database;

    [SerializeField] private InputManager inputManager;

    [SerializeField] private GameObject mouseIndicator;

    [SerializeField] private Grid _grid;

    [SerializeField] private int selectedObject = 0;

    private Dictionary<Vector3Int, bool> _isBusy = new Dictionary<Vector3Int, bool>(); 
    
    private void Start()
    {
        inputManager.OnClicked += PlaceObject;
    }

    private void Update()
    {
        var mousePosition = inputManager.GetSelectedMapPosition();
        var gridPosition = _grid.WorldToCell(mousePosition);
        mouseIndicator.transform.position = _grid.CellToWorld(gridPosition);
    }
    
    public void PlaceObject()
    {
        var mousePosition = inputManager.GetSelectedMapPosition();
        var gridPosition = _grid.WorldToCell(mousePosition);
        if (IfBusy(gridPosition))
        {
            return;
        }
        GameObject newObject = Instantiate(database.objectsData[selectedObject].Prefab);
        newObject.transform.position = _grid.CellToWorld(gridPosition) + database.objectsData[selectedObject].Offset;
        _isBusy[gridPosition] = true;
    }

    private bool IfBusy(Vector3Int position)
    {
        bool value;
        if (_isBusy.TryGetValue(position, out value))
        {
            return value;
        }

        return false;
    }
}//