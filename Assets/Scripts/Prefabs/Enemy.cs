using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using EZCameraShake;

public class Enemy : MonoBehaviour
{
    public EnemyDataSO enemyData;
    public int currentHP;
    public TextMeshPro nameText;
    private Animator _animator;
    public Slider enemyHPBar;
    public PlayerDataSO playerData;
    public Text message;
    private bool idleState = false;
    private bool aggroState = false;
    public SoundManager soundManager;

    private void Awake() {
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        enemyHPBar.value = 1;
        enemyHPBar.interactable = false;
        InitializeEnemy();
        // TurnManager.Instance.OnTurnChanged += HandleTurnChange;
    }

    // void OnDestroy()
    // {
    //     // Unsubscribe to avoid memory leaks
    //     TurnManager.Instance.OnTurnChanged -= HandleTurnChange;
    // }

    void InitializeEnemy()
    {
        if (enemyData != null)
        {
            nameText = GetComponentInChildren<TextMeshPro>();
            nameText.text = GetRandomEnemyName();
            currentHP = enemyData.maxHP;
            Debug.Log($"{enemyData.enemyName} spawned with {currentHP} HP and {enemyData.damage} damage.");
        }
        else
        {
            Debug.LogError("EnemyData is not assigned!");
        }
    }

    private static readonly List<string> enemyNames = new List<string>
    {
        "AC", "AS", "BD", "BT", "CG", "CT", "CV", "DD", "DO", "FO", 
        "FR", "FW", "GN", "GY", "HO", "JK", "KH", "MJ", "MM", "MR", 
        "MV", "NB", "NE", "NS", "NT", "OV", "PL", "RU", "SC", "TI", 
        "VD", "VM", "VX", "WS", "WW", "YD"
    };

    public static string GetRandomEnemyName()
    {
        int randomIndex = Random.Range(0, enemyNames.Count);
        return enemyNames[randomIndex];
    }

    private int defenseScalingFactor;
    public void TakeDamage(int damage)
    {
        int defense = enemyData.defense;
        if(damage <= 10){
            defenseScalingFactor = Random.Range(100, 201);
        }else if(damage < 50){
            defenseScalingFactor = Random.Range(50, 101);
        }else if(damage >= 50){
            defenseScalingFactor = Random.Range(20, 51);
        }

        float defenseFactor = 1 - (defense / (defense + defenseScalingFactor));
        float damageOutput = damage * defenseFactor;
        
        damage = (int)damageOutput;

        float critChance = playerData.defense.itemPoint * 0.01f;
        float critDamage = playerData.sword.itemPoint * (1 + playerData.magic.itemPoint * 0.01f);
        float realDamage = damage;

        if (Random.value < critChance) 
        {
            realDamage = critDamage;
        }

        if(realDamage == damage){
            message.color = Color.white;
        }else{
            message.color = new Color(1f, 0.647f, 0f);
        }
        
        message.text = realDamage.ToString();
        message.gameObject.SetActive(true);

        currentHP -= (int)realDamage;
        Debug.Log($"{enemyData.enemyName} took {realDamage} damage. Remaining HP: {currentHP}");

        StartCoroutine(swordAnimation());

        StartCoroutine(resetMessage());

        if (currentHP <= 0)
        {
            StartCoroutine(swordAnimation2());
            return;
        }
    }

    private IEnumerator swordAnimation(){
        yield return new WaitForSeconds(1f);
        updateHPBar();
    }

    private IEnumerator swordAnimation2(){
        yield return new WaitForSeconds(1.5f);
        if(enemyData.maxHP == 10){
            playerData.playerExp += 1000;
            playerData.currentZhen += 2;
        }else if(enemyData.maxHP == 50){
            playerData.playerExp += 500;
            playerData.currentZhen += 15;
        }else if(enemyData.maxHP == 200){
            playerData.playerExp += 1000;
            playerData.currentZhen += 50;
        }
        Die();
    }

    public void alert(){
        if(idleState){
            return;
        }
        idleState = true;
        message.color = Color.white;
        message.text = "??";
        message.gameObject.SetActive(true);
        StartCoroutine(resetMessage());
    }

    public void aggro(){
        // soundManager.combatSound();
        if(aggroState){
            return;
        }
        idleState = true;
        aggroState = true;
        message.color = Color.red;
        message.text = "!!";
        message.gameObject.SetActive(true);
        StartCoroutine(resetMessage());
    }

    private IEnumerator resetMessage()
    {
        yield return new WaitForSeconds(1f);

        message.gameObject.SetActive(false);
    }

    void Die()
    {
        StartCoroutine(deathAnimation());
        Debug.Log($"{enemyData.enemyName} has been defeated.");
        Destroy(gameObject, 2.5f);
    }

    private IEnumerator deathAnimation(){
        yield return new WaitForSeconds(1f);
        _animator.SetTrigger("Death");
    }

    private void updateHPBar()
    {
        enemyHPBar.value = (float)currentHP/(float)enemyData.maxHP;
    }

    public void DeathSFX(){
        soundManager.deathSound();
    }

    public void PunchSFX(){
        soundManager.punchSound();
        CameraShaker.Instance.ShakeOnce(3f, 3f, 0.5f, 0.5f);
    }

    //Test add move

    private void HandleTurnChange()
    {
        if (TurnManager.Instance.IsEnemyTurn() && aggroState)
        {
            StartCoroutine(EnemyTurn());
        }
    }

    private IEnumerator EnemyTurn()
    {
        // Move towards player one tile at a time
        MoveTowardsPlayer();

        // Wait for the movement to finish, then end the turn
        yield return new WaitForSeconds(1f);  // Adjust the time based on your move speed
        TurnManager.Instance.EndTurn();  // End the enemy's turn
    }
    
    public void MoveTowardsPlayer()
    {
        if (aggroState)
        {
            Vector3 playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;  // Assuming you store player position as a Vector3 in playerData

            // Find path to player using A*
            List<Vector3> path = FindPath(transform.position, playerPosition);
            if (path != null && path.Count > 0)
            {
                StartCoroutine(MoveOneTileAtATime(path));
            }
        }
    }

    // Move one tile at a time towards the player
    private IEnumerator MoveOneTileAtATime(List<Vector3> path)
    {
        if (path.Count == 0) yield break;

        Vector3 destination = path[0];
        Vector3 direction = (destination - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Rotate towards the destination tile
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 720f * Time.deltaTime);
            yield return null;
        }

        // Move the enemy
        while (Vector3.Distance(transform.position, destination) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, 5f * Time.deltaTime);
            yield return null;
        }

        transform.position = destination;

        // Attack the player if adjacent
        if (Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position) <= 1.5f)
        {
            AttackPlayer();
        }
    }

    // Attack the player if adjacent
    public void AttackPlayer()
    {
        Debug.Log("INSERT ATTACK LOGIC HERE");
    }

    // A* Pathfinding method (simplified)
    public List<Vector3> FindPath(Vector3 start, Vector3 target)
    {
        Vector3Int startTile = Vector3Int.RoundToInt(start);
        Vector3Int targetTile = Vector3Int.RoundToInt(target);

        // Implement your A* pathfinding here
        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        Node startNode = new Node(startTile, null, 0, GetHeuristic(startTile, targetTile));
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            foreach (Node node in openList)
            {
                if (node.FCost < currentNode.FCost || (node.FCost == currentNode.FCost && node.HCost < currentNode.HCost))
                    currentNode = node;
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode.Position == targetTile)
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (Vector3Int neighbor in GetNeighbors(currentNode.Position))
            {
                if (closedList.Contains(new Node(neighbor, null, 0, 0))) continue;

                float gCost = currentNode.GCost + 1;  // assuming each tile is 1 distance away
                float hCost = GetHeuristic(neighbor, targetTile);
                Node neighborNode = new Node(neighbor, currentNode, gCost, hCost);

                if (!openList.Contains(neighborNode))
                {
                    openList.Add(neighborNode);
                }
            }
        }

        return null;  // Return null if no path found
    }

    private List<Vector3Int> GetNeighbors(Vector3Int node)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>
        {
            new Vector3Int(node.x + 1, node.y, node.z),
            new Vector3Int(node.x - 1, node.y, node.z),
            new Vector3Int(node.x, node.y, node.z + 1),
            new Vector3Int(node.x, node.y, node.z - 1)
        };

        return neighbors;
    }

    private float GetHeuristic(Vector3Int start, Vector3Int target)
    {
        return Mathf.Abs(start.x - target.x) + Mathf.Abs(start.z - target.z);
    }

    private List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }
        path.Reverse();

        return path;
    }

    // Node class for A* pathfinding
    private class Node
    {
        public Vector3Int Position;
        public Node Parent;
        public float GCost;
        public float HCost;

        public float FCost => GCost + HCost;

        public Node(Vector3Int position, Node parent, float gCost, float hCost)
        {
            Position = position;
            Parent = parent;
            GCost = gCost;
            HCost = hCost;
        }
    }



}
