using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementPlayer : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;
    private bool isMoving = false;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isMoving)
        {
            _animator.SetBool("IsMoving", true);
        }
        else
        {
            _animator.SetBool("IsMoving", false);
        }
    }

    public void MoveTo(Vector3 targetPosition)
    {
        if (isMoving) return;

        List<Vector3> path = FindPath(transform.position, targetPosition);
        if (path != null && path.Count > 0)
        {
            StartCoroutine(MoveAlongPath(path));
        }
    }

    private IEnumerator MoveAlongPath(List<Vector3> path)
    {
        isMoving = true;

        foreach (Vector3 waypoint in path)
        {
            Vector3 destination = new Vector3(waypoint.x, transform.position.y, waypoint.z);

            Vector3 direction = (destination - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                yield return null;
            }

            while (Vector3.Distance(transform.position, destination) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = destination;
        }

        isMoving = false;
    }

    public List<Vector3> FindPath(Vector3 start, Vector3 target)
    {
        Vector3Int startTile = Vector3Int.RoundToInt(start);
        Vector3Int targetTile = Vector3Int.RoundToInt(target);

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        Node startNode = new Node(startTile, null, 0, GetHeuristic(startTile, targetTile));
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            foreach (Node node in openList)
            {
                if (node.F < currentNode.F || (node.F == currentNode.F && node.H < currentNode.H))
                {
                    currentNode = node;
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode.Position == targetTile)
            {
                return ReconstructPath(currentNode);
            }

            foreach (Vector3Int neighborPos in GetNeighbors(currentNode.Position))
            {
                if (!GridManager.Instance.IsValidTile(neighborPos)) continue;

                if (closedList.Contains(new Node(neighborPos, null, 0, 0))) continue;

                int tentativeG = currentNode.G + 1;

                Node neighborNode = openList.Find(n => n.Position == neighborPos);
                if (neighborNode == null)
                {
                    neighborNode = new Node(neighborPos, currentNode, tentativeG, GetHeuristic(neighborPos, targetTile));
                    openList.Add(neighborNode);
                }
                else if (tentativeG < neighborNode.G)
                {
                    neighborNode.G = tentativeG;
                    neighborNode.Parent = currentNode;
                }
            }
        }
        return null;
    }


    private List<Vector3> ReconstructPath(Node node)
    {
        List<Vector3> path = new List<Vector3>();
        while (node != null)
        {
            path.Add(node.Position);
            node = node.Parent;
        }
        path.Reverse();
        return path;
    }

    private List<Vector3Int> GetNeighbors(Vector3Int position)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>
        {
            position + Vector3Int.right,
            position + Vector3Int.left,
            position + Vector3Int.forward,
            position + Vector3Int.back
        };
        return neighbors;
    }

    private int GetHeuristic(Vector3Int a, Vector3Int b)
    {
        // Manhattan distance as heuristic
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
    }

    
    private class Node
    {
        public Vector3Int Position;
        public Node Parent;
        public int G;
        public int H;
        public int F => G + H; 

        public Node(Vector3Int position, Node parent, int g, int h)
        {
            Position = position;
            Parent = parent;
            G = g;
            H = h;
        }

        public override bool Equals(object obj)
        {
            if (obj is Node other)
            {
                return Position == other.Position;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}
