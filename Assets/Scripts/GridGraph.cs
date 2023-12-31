using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GridGraph : MonoBehaviour
{
    [SerializeField] private ObjectsDatabaseSO database;

    [SerializeField] private float lineWidth = 100f;

    [SerializeField] private int _colorsAmt = 4;

    [SerializeField] private GameObject[] _buildings = new GameObject[4];

    [SerializeField] private GameObject _powerPlant;

    [SerializeField] private Material[] _powerPlantMaterials = new Material[4];

    private Vector3Int[] powerPlantsPos = new Vector3Int[4];

    [SerializeField] private int _buildingsAddedPerRound = 4;

    [SerializeField] private int _roadsAddedPerRound = 50;

    [SerializeField] private int _roundsAmt = 5;

    [SerializeField] private int _availableRoadsAmt = 50;

    public Slider _progressBar;

    private int _roundIdx;

    public TMP_Text _roadsAmtText;

    private readonly Dictionary<Vector3Int, List<Vector3Int>> _graph = new();

    private readonly Dictionary<Vector3Int, State> _graphStates = new();

    private LineRenderer _lineDrawer;

    private GameObject _newLine;

    private Vector3 _offset;

    private Dictionary<Tuple<Vector3Int, Vector3Int>, GameObject> _roads = new();

    private Grid grid;

    private Vector3 _cellCenterOffset;

    private void Start()
    {
        grid = GetComponent<Grid>();
        _offset = new Vector3(grid.cellSize.x / 2f, 0.01f, grid.cellSize.x / 2f);
        _cellCenterOffset = new Vector3(grid.cellSize.x / 2f, 0, grid.cellSize.x / 2f);
        // Debug.Log(grid.cellSize);
        GetChilds();

        PlacePowerPlant();
        PlaceBuilding(2 * _buildingsAddedPerRound);

        _roundIdx = 1;
        _progressBar.value = 0f;
    }

    private void Update()
    {
        _roadsAmtText.SetText("" + _availableRoadsAmt);

        if (CheckTurnEnd())
        {
            if (_roundIdx == _roundsAmt)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }

            ++_roundIdx;
            _progressBar.value = (float)(_roundIdx - 1) / _roundsAmt;

            PlaceBuilding(_buildingsAddedPerRound);

            _availableRoadsAmt += _roadsAddedPerRound;
        }
    }

    private bool CheckTurnEnd()
    {
        foreach (var pos in _graphStates.Keys)
        {
            var currState = _graphStates[pos];

            if (currState.type == "building" && currState.color != currState.neededColor)
            {
                return false;
            }
        }

        return true;
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
        if (_availableRoadsAmt == 0)
        {
            return false;
        }

        if (point_1 == point_2 ||
            Vector3.Distance(point_1, point_2) >= Math.Sqrt(2) * grid.cellSize.x)
        {
            return false;
        }

        if (_graph.ContainsKey(point_1) && _graph[point_1].Contains(point_2))
        {
            return true;
        }

        var lhs_color = _graphStates[point_1].color;
        var rhs_color = _graphStates[point_2].color;

        if (lhs_color != rhs_color && 
            (lhs_color != Color.gray && rhs_color != Color.gray))
        {
            return false;
        }


        var points = new Vector3[2];
        points[0] = CellToWorld(point_1) + _offset;
        points[1] = CellToWorld(point_2) + _offset;

        _newLine = Instantiate(database.objectsData[0].Prefab);
        _newLine.transform.position = points[0] + new Vector3(-grid.cellSize.x, 0.05f, 0f);

        _lineDrawer = _newLine.GetComponent<LineRenderer>();

        _lineDrawer.startColor = Color.gray;
        _lineDrawer.endColor = Color.gray;

        _lineDrawer.startWidth = lineWidth;
        _lineDrawer.endWidth = lineWidth;

        _lineDrawer.positionCount = 2;
        _lineDrawer.SetPositions(points);

        if (!_graphStates[point_1].isOccupied)
        {
            _graphStates[point_1].isOccupied = true;
            _graphStates[point_1].type = "road";
            _graphStates[point_1].color = Color.gray;
        }

        if (!_graphStates[point_2].isOccupied)
        {
            _graphStates[point_2].isOccupied = true;
            _graphStates[point_2].type = "road";
            _graphStates[point_2].color = Color.gray;
        }

        var newRoad = new Tuple<Vector3Int, Vector3Int>(point_1, point_2);
        _roads[newRoad] = _newLine;

        _graph[point_1].Add(point_2);
        _graph[point_2].Add(point_1);

        --_availableRoadsAmt;

        SpreadEnergy();

        return true;
    }

    public bool IsPointInCell(Vector3 vector3)
    {
        return Vector3.Distance(CellToWorld(WorldToCell(vector3)) + _cellCenterOffset, vector3)
               < grid.cellSize.x / 2.5f;
    }

    public void PlaceBuilding(int amount = 8)
    {
        Color[] colorMap = new Color[4]
        {
            Color.red, Color.green, Color.blue, Color.yellow
        };

        Vector3Int[] keys = new Vector3Int[_graphStates.Count];
        _graphStates.Keys.CopyTo(keys, 0);

        for (int i = 0; i < amount;)
        {
            var randPos = keys[UnityEngine.Random.Range(0, keys.Length)];

            if (_graphStates[randPos].type == "building" || _graphStates[randPos].type == "power")
            {
                continue;
            }

            State newState = new State();
            newState.isOccupied = true;
            newState.type = "building";
            // newState.color = Color.gray;

            int randColor = UnityEngine.Random.Range(0, _colorsAmt);

            newState.neededColor = colorMap[randColor];

            var prevColor = _graphStates[randPos].color;
            _graphStates[randPos] = newState;
            _graphStates[randPos].color = prevColor;

            (randPos.y, randPos.z) = (randPos.z, randPos.y);
            var newBuilding = Instantiate(
                _buildings[randColor],
                randPos,
                Quaternion.identity
            );
            newBuilding.transform.position += new Vector3(_offset.x, 0f, _offset.z);

            MeshRenderer renderer = newBuilding.GetComponentInChildren<MeshRenderer>();
            renderer.enabled = true;

            ++i;
        }
    }

    public void PlacePowerPlant()
    {
        Vector3Int[] keys = new Vector3Int[_graphStates.Count];
        _graphStates.Keys.CopyTo(keys, 0);

        List<Color> colors = new List<Color>
        {
            Color.red, Color.green, Color.blue, Color.yellow
        };

        for (int i = 0; i < _colorsAmt; ++i)
        {
            var randPos = keys[UnityEngine.Random.Range(0, keys.Length)];

            if (_graphStates[randPos].isOccupied)
            {
                continue;
            }

            State newState = new State();
            newState.isOccupied = true;
            newState.type = "power";
            newState.color = colors[i];
            newState.neededColor = colors[i];

            _graphStates[randPos] = newState;

            (randPos.y, randPos.z) = (randPos.z, randPos.y);
            var newBuilding = Instantiate(_powerPlant, randPos, Quaternion.identity);
            newBuilding.transform.position += new Vector3(_offset.x, 0.0f, _offset.z);

            MeshRenderer renderer = newBuilding.GetComponentInChildren<MeshRenderer>();
            renderer.material = _powerPlantMaterials[i];
            renderer.enabled = true;

            powerPlantsPos[i] = randPos;
        }
    }

    private void ReDrawLine(Vector3Int startPos, Vector3Int endPos, Color newColor)
    {
        var currLine = new Tuple<Vector3Int, Vector3Int>(startPos, endPos);
        var currLineBack = new Tuple<Vector3Int, Vector3Int>(endPos, startPos);
        _graphStates[startPos].color = newColor;
        _graphStates[endPos].color = newColor;

        var resultCurrLine = new Tuple<Vector3Int, Vector3Int>(new Vector3Int(), new Vector3Int());

        if (_roads.ContainsKey(currLine))
        {
            _roads[currLine].GetComponent<LineRenderer>().startColor = newColor;
            _roads[currLine].GetComponent<LineRenderer>().endColor = newColor;
            // Destroy(_roads[currLine], 0);
            resultCurrLine = currLine;
        }
        if (_roads.ContainsKey(currLineBack))
        {
            _roads[currLineBack].GetComponent<LineRenderer>().startColor = newColor;
            _roads[currLineBack].GetComponent<LineRenderer>().endColor = newColor;
            // Destroy(_roads[currLineBack], 0);
            resultCurrLine = currLineBack;
        }

        // var newRoad = Instantiate(database.objectsData[0].Prefab);
        // newRoad.transform.position = startPos + new Vector3(0f, 0.05f, 0f);
        // var lineDrawer = newRoad.GetComponent<LineRenderer>();

        var currPos = resultCurrLine.Item1;
        // (currPos.y, currPos.z) = (currPos.z, currPos.y);

        if (_graphStates[currPos].color == Color.gray)
        {
            currPos = resultCurrLine.Item2;
            (currPos.y, currPos.z) = (currPos.z, currPos.y);
        }

        // lineDrawer.startColor = _graphStates[currPos].color;
        // lineDrawer.endColor = _graphStates[currPos].color;
        // lineDrawer.startWidth = lineWidth;
        // lineDrawer.endWidth = lineWidth;
        //
        // lineDrawer.positionCount = 2;
        // Vector3[] points = new Vector3[2];
        // points[0] = CellToWorld(startPos) + _offset;
        // points[1] = CellToWorld(endPos) + _offset;
        //
        // lineDrawer.SetPositions(points);
        // _roads.Add(resultCurrLine, newRoad);
    }

    private bool ConnectsToPowerPlant(Vector3Int startPos)
    {
        var colorMap = new Dictionary<Color, int>
        {
            { Color.red, 0 },
            { Color.green, 1 },
            { Color.blue, 2 },
            { Color.yellow, 3 },
        };
        Vector3Int endPos = powerPlantsPos[colorMap[_graphStates[startPos].color]];

        if (_graphStates[startPos].type != "building" || startPos == endPos)
        {
            return false;
        }

        // Initialize the visited dictionary
        var visited = new Dictionary<Vector3Int, bool>();

        // Initialize the queue
        var queue = new Queue<Vector3Int>();

        // Initialize the parent dictionary to track the shortest path
        var parent = new Dictionary<Vector3Int, Vector3Int>();

        // Enqueue the start node
        queue.Enqueue(startPos);
        visited[startPos] = true;

        bool connects = false;
        while (queue.Count > 0)
        {
            var currentPos = queue.Dequeue();

            if (currentPos == endPos)
            {
                connects = true;
                break;
            }

            foreach (var neighborNode in _graph[currentPos])
            {
                if (!visited.ContainsKey(neighborNode))
                {
                    visited[neighborNode] = true;
                    queue.Enqueue(neighborNode);
                    parent[neighborNode] = currentPos;
                }
            }
        }

        if (!connects)
        {
            Debug.Log("Does not connect!");
            return false;
        }

        Debug.Log("Connects!!");

        // Reconstruct the shortest path
        var node = endPos;

        while (node != startPos)
        {
            var currRoad = new Tuple<Vector3Int, Vector3Int>(node, parent[node]);
            Destroy(_roads[currRoad]);

            var newRoad = Instantiate(database.objectsData[0].Prefab);
            _newLine.transform.position = node + new Vector3(0f, 0.05f, 0f);
            var lineDrawer = _newLine.GetComponent<LineRenderer>();

            lineDrawer.startColor = _graphStates[startPos].color;
            lineDrawer.endColor = _graphStates[startPos].color;
            lineDrawer.startWidth = lineWidth;
            lineDrawer.endWidth = lineWidth;

            lineDrawer.positionCount = 2;
            Vector3[] points = new Vector3[2]
            {
                node, parent[node]
            };

            lineDrawer.SetPositions(points);

            node = parent[node];
        }

        return true;
    }

    private void SpreadEnergy()
    {
        for (int i = 0; i < _colorsAmt; ++i)
        {
            var powerPlant = powerPlantsPos[i];
            var visitQueue = new Queue<Vector3Int>();
            var visited = new List<Vector3Int>();

            visitQueue.Enqueue(powerPlant);

            while (visitQueue.Count != 0)
            {
                var currPos = visitQueue.Dequeue();

                if (!_graph.ContainsKey(currPos))
                {
                    (currPos.y, currPos.z) = (currPos.z, currPos.y);
                }

                visited.Add(currPos);

                foreach (Vector3Int neighbourPos in _graph[currPos])
                {
                    if (visited.Contains(neighbourPos))
                    {
                        continue;
                    }

                    ReDrawLine(currPos, neighbourPos, _graphStates[currPos].color);
                    visitQueue.Enqueue(neighbourPos);
                }
            }
        }
    }

    public void CheckConnection(Vector3Int pos)
    {
        if (_graphStates[pos].type == "building")
        {
            //Debug.Log("Try to check!");
            //var path = ConnectsToPowerPlant(pos);
        }
        else if (_graphStates[pos].type == "power")
        {
            //Debug.Log("Spreading electricity!");
            //SpreadEnergy(pos);
        }
    }

    public void DeletePoint(Vector3Int pos)
    {
        if (!_graph.ContainsKey(pos)) return;

        foreach (Vector3Int neighbourPos in _graph[pos])
        {
            var currLine = new Tuple<Vector3Int, Vector3Int>(pos, neighbourPos);
            var currLineBack = new Tuple<Vector3Int, Vector3Int>(neighbourPos, pos);

            if (_roads.ContainsKey(currLine))
            {
                Destroy(_roads[currLine], 0);
                _roads.Remove(currLine);
            }

            if (_roads.ContainsKey(currLineBack))
            {
                Destroy(_roads[currLineBack], 0);
                _roads.Remove(currLineBack);
            }

            _graph[neighbourPos].Remove(pos);

            if (_graphStates[pos].type != "power")
            {
                _graphStates[pos].color = Color.gray;

                if (_graphStates[pos].type != "building")
                {
                    _graphStates[pos].type = "";
                }
            }

            if (_graphStates[neighbourPos].type != "power")
            {
                _graphStates[neighbourPos].color = Color.gray;

                if (_graphStates[neighbourPos].type != "building")
                {
                    _graphStates[neighbourPos].type = "";
                }
            }

            ++_availableRoadsAmt;
        }

        _graph[pos].Clear();

        SetDefaultColor();
        SpreadEnergy();
    }

    private void SetDefaultColor()
    {
        foreach (var item in _roads)
        {
            Vector3Int startPos = item.Key.Item1;
            Vector3Int endPos = item.Key.Item2;
            item.Value.GetComponent<LineRenderer>().startColor = Color.gray;
            item.Value.GetComponent<LineRenderer>().endColor = Color.gray;

            if (_graphStates[startPos].type != "power")
            {
                _graphStates[startPos].color = Color.gray;

                if (_graphStates[startPos].type != "building")
                {
                    _graphStates[startPos].type = "";
                }
            }

            if (_graphStates[endPos].type != "power")
            {
                _graphStates[endPos].color = Color.gray;

                if (_graphStates[endPos].type != "building")
                {
                    _graphStates[endPos].type = "";
                }
            }
        }
    }

    private class State
    {
        public Color color = Color.gray;
        public Color neededColor = Color.white;
        public bool isOccupied = false;
        public string type = "";
    }

    private class StartEnd
    {
        public StartEnd(Vector3Int p1, Vector3Int p2)
        {
            if (p1.x < p2.x ||
                (p1.x == p2.x && p1.y < p2.y) ||
                (p1.x == p2.x && p1.y == p2.y && p1.z < p2.z))
            {
                startPoint = p1;
                endPoint = p2;
                return;
            }
            startPoint = p2;
            endPoint = p1;
        }
        public Vector3Int endPoint;
        public Vector3Int startPoint;

        public StartEnd(Vector3Int start, Vector3Int end)
        {
            startPoint = start;
            endPoint = end;
        }

        public static bool operator ==(StartEnd lhs, StartEnd rhs)
        {
            return (lhs.startPoint == rhs.startPoint && lhs.endPoint == rhs.endPoint) ||
                (lhs.startPoint == rhs.endPoint && lhs.endPoint == rhs.startPoint);
        }

        public static bool operator !=(StartEnd lhs, StartEnd rhs)
        {
            return !(lhs == rhs);
        }
    }
}