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
    public bool idleState = false;
    public bool aggroState = false;
    public SoundManager soundManager;

    private void Awake() {
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        enemyHPBar.value = 1;
        enemyHPBar.interactable = false;
        InitializeEnemy();
    }

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
        TurnManager.Instance.RemoveEnemyFromTurnList(this);
        GridManager.Instance.RemoveEnemyFromGrid(this);

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




    //CODING DISINI
    public IEnumerator EnemyTurn()
    {
        if (aggroState)
        {
            // Attack if adjacent to the player
            if (IsAdjacent(GameObject.FindGameObjectWithTag("Player").transform.position, transform.position))
            {
                if(currentHP > 0){
                    AttackPlayer();
                }
            }else if(!IsAdjacent(GameObject.FindGameObjectWithTag("Player").transform.position, transform.position)){
                MoveTowardsPlayer();
            }
            // Wait before ending the turn
            yield return new WaitForSeconds(1f); // Adjust this delay based on your game flow
            TurnManager.Instance.EndTurn();  // Assuming you have the TurnManager set up properly
        }
    }

    bool IsAdjacent(Vector3 playerPosition, Vector3 enemyPosition)
    {
        // Rounding positions to integers to eliminate floating-point precision issues
        Vector3 playerRounded = new Vector3(Mathf.Round(playerPosition.x), 0, Mathf.Round(playerPosition.z));
        Vector3 enemyRounded = new Vector3(Mathf.Round(enemyPosition.x), 0, Mathf.Round(enemyPosition.z));

        // Check if player and enemy are adjacent (left/right or forward/back)
        return (Mathf.Abs(playerRounded.x - enemyRounded.x) == 1 && playerRounded.z == enemyRounded.z) ||
            (Mathf.Abs(playerRounded.z - enemyRounded.z) == 1 && playerRounded.x == enemyRounded.x);
    }

    public void AttackPlayer(){
        _animator.SetTrigger("Punch");

        GameObject skill = GameObject.FindGameObjectWithTag("Passive_2");
        int defense = playerData.armor.itemPoint;
        if(skill != null){
            defense += defense * 20 / 100;
        }
        if(enemyData.damage == 2){
            defenseScalingFactor = Random.Range(100, 201);
        }else if(enemyData.damage >= 10){
            defenseScalingFactor = Random.Range(50, 101);
        }else if(enemyData.damage >= 50){
            defenseScalingFactor = Random.Range(20, 51);
        }

        float defenseFactor = 1 - (defense / (defense + defenseScalingFactor));
        Debug.Log("Defense Factor: " + defenseFactor);
        float damageOutput = enemyData.damage * defenseFactor;

        GridManager.Instance.attackPlayer((int)damageOutput);
    }

    

    public float moveSpeed = 3f;   // Move speed for the enemy
    public float rotationSpeed = 10f;  // Rotation speed to turn smoothly towards the player

    // Call this function to start moving towards the player
    public void MoveTowardsPlayer()
    {
        if (aggroState)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                List<Vector3> path = FindPath(transform.position, player.transform.position);
                if (path != null && path.Count > 0)
                {
                    Debug.Log("PATH COUNT (NOT NULL) = " + path.Count);
                    StartCoroutine(MoveOneTileAtATime(path));  // Move enemy one tile at a time
                }else{
                    Debug.Log("PATH NOT EXIST (BLOCKED BY DECORATION)");
                }
            }
        }
    }

    private IEnumerator MoveOneTileAtATime(List<Vector3> path)
    {
        Debug.Log("MASUK KE MoveOneTileAtATime");
        // Only move to the first waypoint (one tile at a time)
        Vector3 destination = new Vector3(path[0].x, transform.position.y, path[0].z);

        // Smoothly rotate towards the destination
        Vector3 direction = (destination - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Rotate the enemy towards the destination, with a threshold for smooth rotation
        while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)  // Tolerance for rotation
        {
            Debug.Log("MASUK KE MoveOneTileAtATime (ROTATION)");
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        // Move the enemy towards the destination, with a small tolerance
        while (Vector3.Distance(transform.position, destination) > 0.1f)  // Tolerance for position
        {
            Debug.Log("MASUK KE MoveOneTileAtATime (MOVEMENT)");
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }


        // Snap to the exact destination position
        transform.position = destination;
    }

    public List<Vector3> FindPath(Vector3 start, Vector3 target)
    {
        Vector3Int startTile = Vector3Int.RoundToInt(start);
        Vector3Int targetTile = Vector3Int.RoundToInt(target);

        // Implement A* Pathfinding
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
                {
                    currentNode = node;
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode.Position == targetTile)
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (Vector3Int neighbor in GetNeighbors(currentNode.Position))
            {
                // Skip if the neighbor is already in the closed list
                if (closedList.Contains(new Node(neighbor, null, 0, 0))) continue;

                // Check if the neighbor tile is valid (tagged with "Tile" or "Hallway")
                if (!IsValidTile(neighbor)) continue;

                float gCost = currentNode.GCost + 1;
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

    // Helper function to check if the tile is valid (tagged with "Tile" or "Hallway")
    private bool IsValidTile(Vector3Int position)
    {
        // Convert Vector3Int position to Vector3 for querying the tile's tag
        Vector3 worldPosition = new Vector3(position.x, 0, position.z); // Assuming y-coordinate is not relevant
        RaycastHit hit;
        
        // Cast a ray to check if the tile at the given position has a valid tag
        if (Physics.Raycast(worldPosition + Vector3.up * 10, Vector3.down, out hit, Mathf.Infinity))
        {
            // Check if the tile has the required tag ("Tile" or "Hallway")
            return hit.collider.CompareTag("Tile") || hit.collider.CompareTag("Hallway");
        }

        return false;  // If no valid tile found, return false
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

    private float GetHeuristic(Vector3Int start, Vector3Int target)
    {
        return Mathf.Abs(start.x - target.x) + Mathf.Abs(start.z - target.z);  // Manhattan Distance
    }

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
