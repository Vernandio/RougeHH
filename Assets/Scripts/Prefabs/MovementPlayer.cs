using System.Collections;
using System.Collections.Generic;
using EZCameraShake;
using UnityEngine;
using UnityEngine.UI;

public class MovementPlayer : MonoBehaviour
{
    public Text playerMessage;
    public SoundManager soundManager;
    private List<Enemy> enemies;
    private float moveSpeed = 3f;
    private float rotationSpeed = 720f;
    public bool isMoving = false;
    private Animator _animator;
    public bool isPlayerTurn = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private IEnumerator Start() {
        yield return new WaitForSeconds(1f);

        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        enemies = new List<Enemy>();

        foreach (GameObject enemyObject in enemyObjects)
        {
            Enemy enemy = enemyObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemies.Add(enemy);
            }
        }
    }

    void Update()
    {
        checkIsMoving();
        if (isMoving)
        {
            _animator.SetBool("IsMoving", true);
        }
        else
        {
            _animator.SetBool("IsMoving", false);
        }
    }

    //Movement Logic
    public void MoveTo(Vector3 targetPosition, bool aggro)
    {
        if(isMoving) {
            return;
        }

        List<Vector3> path = findPath(transform.position, targetPosition);
        if (path != null && path.Count > 0)
        {
            if(aggro){
                StartCoroutine(MoveOneTileAtATime(path));
            }else{
                StartCoroutine(moveAlongPath(path));
            }
        }
    }

    public IEnumerator MoveOneTileAtATime(List<Vector3> path)
    {
        isMoving = true; 

        Vector3 destination = new Vector3(path[1].x, transform.position.y, path[1].z);
        Vector3 startPosition = transform.position;

        Vector3 direction = (destination - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction); 

        while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
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

        isMoving = false;

        StartCoroutine(EndPlayerTurn());
    }

    private IEnumerator moveAlongPath(List<Vector3> path)
    {
        isMoving = true;

        foreach (Vector3 waypoint in path)
        {
            if (IsAnyEnemyAlerted() || !checkIsMoving())  
            {
                break;
            }

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
        StartCoroutine(EndPlayerTurn());
    }

    //A* Logic
    public List<Vector3> findPath(Vector3 start, Vector3 target)
    {
        Vector3Int startTile = Vector3Int.RoundToInt(start);
        Vector3Int targetTile = Vector3Int.RoundToInt(target);

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        Node startNode = new Node(startTile, null, 0, getHeuristic(startTile, targetTile));
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
                return reconstructPath(currentNode);
            }

            foreach (Vector3Int neighborPos in getNeighbors(currentNode.Position))
            {
                if (!GridManager.Instance.IsValidTile(neighborPos)) continue;

                if (closedList.Contains(new Node(neighborPos, null, 0, 0))) continue;

                int tentativeG = currentNode.G + 1;

                Node neighborNode = openList.Find(n => n.Position == neighborPos);
                if (neighborNode == null)
                {
                    neighborNode = new Node(neighborPos, currentNode, tentativeG, getHeuristic(neighborPos, targetTile));
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

    private List<Vector3> reconstructPath(Node node)
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

    private List<Vector3Int> getNeighbors(Vector3Int position)
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

    private int getHeuristic(Vector3Int a, Vector3Int b)
    {
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

    //Turn Logic
    public void SetPlayerTurn()
    {
        isPlayerTurn = true;
    }

    public IEnumerator EndPlayerTurn()
    {
        yield return new WaitForSeconds(2.5f);
        isPlayerTurn = false;
        TurnManager.Instance.EndTurn();
    }

    
    //Function-function tambahan
    public void FootstepSFX(){
        soundManager.walkSound();
    }
    
    public void SwordSFX(){
        soundManager.swordSound();
        CameraShaker.Instance.ShakeOnce(3f, 3f, 0.5f, 0.5f);
    }

    private bool checkIsMoving(){
        return isMoving;
    }

    private bool IsAnyEnemyAlerted()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy.idleState)
            {
                return true;
            }
        }
        return false;
    }

    public void enemyRemove(Enemy enemy){
        enemies.Remove(enemy);
    }

    public void levelUp(){
        playerMessage.gameObject.SetActive(true);
        playerMessage.text = "Level Up!!";
        playerMessage.color = Color.yellow;
        StartCoroutine(resetMessage());
    }

    public void getDamage(int damage){
        playerMessage.gameObject.SetActive(true);
        playerMessage.text = damage.ToString();
        playerMessage.color = Color.red;
        StartCoroutine(resetMessage());
    }

    private IEnumerator resetMessage()
    {
        yield return new WaitForSeconds(1f);

        playerMessage.gameObject.SetActive(false);
    }
}
