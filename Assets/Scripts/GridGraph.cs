using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class GridGraph : MonoBehaviour
{
    class State
    {
        public bool isOccupied = false;
        public string color = "";
        public string type = "";
    }
    
    private Grid grid;
    
    private Dictionary<Vector3Int, List<Vector3Int>> _graph = new Dictionary<Vector3Int, List<Vector3Int>>();

    private Dictionary<Vector3Int, State> _graphStates = new Dictionary<Vector3Int, State>();
    void Start()
    {
        grid = GetComponent<Grid>();
        GetChilds();
    }
    private void GetChilds()
    {
        foreach (Transform tilemap in transform)
        {
            foreach (Transform child in tilemap.GetComponent<Transform>())
            {
                Vector3Int gridPosition = grid.WorldToCell(child.position);
                _graph[gridPosition] = new List<Vector3Int>();
                _graphStates[gridPosition] = new State();
            }
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
        Dictionary<Vector3Int, bool> visited = new Dictionary<Vector3Int, bool>();

        // Initialize the queue
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        // Initialize the parent dictionary to track the shortest path
        Dictionary<Vector3Int, Vector3Int> parent = new Dictionary<Vector3Int, Vector3Int>();

        // Enqueue the start node
        queue.Enqueue(startNode);
        visited[startNode] = true;

        Vector3Int endNode = startNode;

        while (queue.Count > 0)
        {
            Vector3Int currentNode = queue.Dequeue();

            if (_graphStates[currentNode].type == "factory")
            {
                endNode = currentNode;
                break; // Found the end node, exit the loop
            }

            foreach (Vector3Int neighborNode in _graph[currentNode])
            {
                if (!visited.ContainsKey(neighborNode))
                {
                    visited[neighborNode] = true;
                    queue.Enqueue(neighborNode);
                    parent[neighborNode] = currentNode;
                }
            }
        }
        
        if (startNode == endNode) return new List<Vector3Int>();

        // Reconstruct the shortest path
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int node = endNode;
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
    
}
