using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private ObjectsDatabaseSO road_database;

    [SerializeField] private InputManager inputManager;

    [SerializeField] private GameObject mouseIndicator;

    [SerializeField] private GridGraph _gridGraph;

    [SerializeField] private int selectedObject;

    [SerializeField] private GameObject blueBuilding;

    private Dictionary<Vector3Int, bool> _isBusy = new();

    private bool _isButtonHold;

    private Vector3Int _previousCellPosition;

    private void Start()
    {
        // inputManager.OnClicked += PlaceObject;
        PlaceRandomBuildings();
    }

    private void Update()
    {
        var mousePosition = inputManager.GetSelectedMapPosition();
        var gridPosition = _gridGraph.WorldToCell(mousePosition);
        mouseIndicator.transform.position = _gridGraph.CellToWorld(gridPosition) + new Vector3(0.5f, 0, 0.5f);

        if (Input.GetButton("Fire1"))
            PlaceRoad();
        else
            _isButtonHold = false;
    }

    public void PlaceObject()
    {
        var mousePosition = inputManager.GetSelectedMapPosition();
        var gridPosition = _gridGraph.WorldToCell(mousePosition);
        if (_gridGraph.IsCellFree(gridPosition)) return;
        var newObject = Instantiate(road_database.objectsData[selectedObject].Prefab);
        newObject.transform.position =
            _gridGraph.CellToWorld(gridPosition) + road_database.objectsData[selectedObject].Offset;
        _gridGraph.PlaceNewObject(gridPosition);
    }

    public void PlaceRoad()
    {
        var mousePosition = inputManager.GetSelectedMapPosition();
        var gridPosition = _gridGraph.WorldToCell(mousePosition);
        if (!_isButtonHold)
        {
            _previousCellPosition = gridPosition;
            _isButtonHold = true;
        }

        if (_gridGraph.IsPointInCell(mousePosition) && _gridGraph.ConnectTwoPoints(_previousCellPosition, gridPosition)) _previousCellPosition = gridPosition;
    }

    public void PlaceRandomBuildings(int amount = 3)
    {
        //Vector3Int[] keys = new Vector3Int[states.Count];
        //states.Keys.CopyTo(keys, 0);

        for (int i = 0; i < amount;)
        {
            var randPos = new Vector3Int(Random.Range(-5, 5), 0, Random.Range(-5, 5));
            
            if (_gridGraph.PlaceBuilding(blueBuilding, randPos))
            {
                ++i;
            }
        }
    }
}