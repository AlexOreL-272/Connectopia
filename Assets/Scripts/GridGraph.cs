using System;
using System.Collections.Generic;
using UnityEngine;

public class GridGraph : MonoBehaviour
{
    [SerializeField] private ObjectsDatabaseSO database;

    [SerializeField] private float lineWidth = 100f;

    [SerializeField] private GameObject blueBuilding;

    [SerializeField] private GameObject powerPlant;

    private readonly Dictionary<Vector3Int, List<Vector3Int>> _graph = new();

    private readonly Dictionary<Vector3Int, State> _graphStates = new();

    private LineRenderer _lineDrawer;

    private GameObject _newLine;

    private Vector3 _offset;

    private Dictionary<StartEnd, GameObject> _roads = new();

    private Grid grid;

    private Vector3 _cellCenterOffset;

    private void Start()
    {
        grid = GetComponent<Grid>();
        _offset = new Vector3(grid.cellSize.x / 2f, 0.01f, grid.cellSize.x / 2f);
        _cellCenterOffset = new Vector3(grid.cellSize.x / 2f, 0, grid.cellSize.x / 2f);
        Debug.Log(grid.cellSize);
        GetChilds();

        PlacePowerPlant();
        PlaceBuilding();
    }

    private void GetChilds()
    {
        foreach (Transform tilemap in transform)
        foreach (Transform child in tilemap.GetComponent<Transform>())
        {
            var gridPosition = grid.WorldToCell(child.position);
            _graph[gridPosition] = new List<Vector3Int>();
            _graphStates[gridPosition] = new State();
        }
    }

    public Vector3Int WorldToCell(Vector3 vector3)
    {
        return grid.WorldToCell(vector3);
    }

    public Vector3 CellToWorld(Vector3Int vector3Int)
    {
        return grid.CellToWorld(vector3Int);
    }

    private List<Vector3Int> FindShortestPathBFS(Vector3Int startNode)
    {
        // Initialize the visited dictionary
        var visited = new Dictionary<Vector3Int, bool>();

        // Initialize the queue
        var queue = new Queue<Vector3Int>();

        // Initialize the parent dictionary to track the shortest path
        var parent = new Dictionary<Vector3Int, Vector3Int>();

        // Enqueue the start node
        queue.Enqueue(startNode);
        visited[startNode] = true;

        var endNode = startNode;

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();

            if (_graphStates[currentNode].color != Color.gray)
            {
                endNode = currentNode;
                break; // Found the end node, exit the loop
            }

            foreach (var neighborNode in _graph[currentNode])
                if (!visited.ContainsKey(neighborNode))
                {
                    visited[neighborNode] = true;
                    queue.Enqueue(neighborNode);
                    parent[neighborNode] = currentNode;
                }
        }

        if (startNode == endNode) return new List<Vector3Int>();

        // Reconstruct the shortest path
        var path = new List<Vector3Int>();
        var node = endNode;
        while (node != startNode)
        {
            path.Insert(0, node);
            node = parent[node];
        }

        path.Insert(0, startNode);

        return path;
    }

    public bool IsCellFree(Vector3Int vector3Int)
    {
        return _graphStates[vector3Int].isOccupied;
    }

    public void PlaceNewObject(Vector3Int vector3Int)
    {
        _graphStates[vector3Int].isOccupied = true;
    }

    public bool ConnectTwoPoints(Vector3Int point_1, Vector3Int point_2)
    {
        if (point_1 == point_2 ||
            Vector3.Distance(point_1, point_2) >= Math.Sqrt(2) * grid.cellSize.x)
            return false;
        //if (_graphStates[point_2].isOccupied)
        //    return false;
        

        var points = new Vector3[2];
        points[0] = CellToWorld(point_1) + _offset;
        points[1] = CellToWorld(point_2) + _offset;


        _newLine = Instantiate(database.objectsData[0].Prefab);
        _newLine.transform.position = points[0];
        _lineDrawer = _newLine.GetComponent<LineRenderer>();
        // _lineDrawer.startColor = Color.red;
        // _lineDrawer.endColor = Color.red;
        // _lineDrawer.startWidth = lineWidth;
        // _lineDrawer.endWidth = lineWidth;


        _lineDrawer.positionCount = 2;
        _lineDrawer.SetPositions(points);

        _graphStates[point_1].isOccupied = true;
        _graphStates[point_2].isOccupied = true;
        _graphStates[point_1].type = "road";
        _graphStates[point_2].type = "road";
        _graphStates[point_1].color = Color.gray;
        _graphStates[point_2].color = Color.gray;

        _graph[point_1].Add(point_2);
        _graph[point_2].Add(point_1);

        return true;
    }

    public bool IsPointInCell(Vector3 vector3)
    {
        return Vector3.Distance(CellToWorld(WorldToCell(vector3)) + _cellCenterOffset, vector3) 
               < grid.cellSize.x / 3f;
    }

    public void PlaceBuilding(int amount = 8)
    {
        Vector3Int[] keys = new Vector3Int[_graphStates.Count];
        _graphStates.Keys.CopyTo(keys, 0);

        for (int i = 0; i < amount;)
        {
            var randPos = keys[UnityEngine.Random.Range(0, keys.Length)];

            if (_graphStates[randPos].type == "building")
            {
                continue;
            }

            State newState = new State();
            newState.isOccupied = true;
            newState.type = "building";
            newState.color = Color.blue;

            _graphStates[randPos] = newState;

            (randPos.y, randPos.z) = (randPos.z, randPos.y);
            var newBuilding = Instantiate(blueBuilding, randPos, Quaternion.identity);
            newBuilding.transform.position += new Vector3(grid.cellSize.x, 4.0f, _offset.z);

            MeshRenderer renderer = newBuilding.GetComponentInChildren<MeshRenderer>();
            renderer.enabled = true;

            ++i;
        }
    }

    public void PlacePowerPlant()
    {
        Vector3Int[] keys = new Vector3Int[_graphStates.Count];
        _graphStates.Keys.CopyTo(keys, 0);

        var randPos = keys[UnityEngine.Random.Range(0, keys.Length)];

        State newState = new State();
        newState.isOccupied = true;
        newState.type = "power";
        newState.color = Color.blue;

        _graphStates[randPos] = newState;

        (randPos.y, randPos.z) = (randPos.z, randPos.y);
        var newBuilding = Instantiate(powerPlant, randPos, Quaternion.identity);
        newBuilding.transform.position += new Vector3(grid.cellSize.x, 0.0f, _offset.z);

        MeshRenderer renderer = newBuilding.GetComponentInChildren<MeshRenderer>();
        renderer.enabled = true;
    }

    private class State
    {
        public Color color;
        public bool isOccupied = false;
        public string type = "";
    }

    private class StartEnd
    {
        public Vector3Int endPoint;
        public Vector3Int startPoint;
    }
}