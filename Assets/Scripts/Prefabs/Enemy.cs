using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using EZCameraShake;

public class Enemy : MonoBehaviour
{
    public EnemyDataSO enemyData;
    public TextMeshPro nameText;
    public Slider enemyHPBar;
    public PlayerDataSO playerData;
    public Text message;
    public SoundManager soundManager;
    private Animator _animator;
    private int currentHP;
    private float moveSpeed = 3f;   // Move speed for the enemy
    private int defenseScalingFactor;
    public bool idleState = false;
    public bool aggroState = false;
    private float detectionRange = 5f;
    public bool check = false;

    private static readonly List<string> enemyNames = new List<string>
    {
        "AC", "AS", "BD", "BT", "CG", "CT", "CV", "DD", "DO", "FO", 
        "FR", "FW", "GN", "GY", "HO", "JK", "KH", "MJ", "MM", "MR", 
        "MV", "NB", "NE", "NS", "NT", "OV", "PL", "RU", "SC", "TI", 
        "VD", "VM", "VX", "WS", "WW", "YD"
    };

    private void Awake() {
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        enemyHPBar.value = 1;
        enemyHPBar.interactable = false;
        InitializeEnemy();
    }

    private void Update() {
        if(check == false){
            if(idleState && CheckLineOfSight()){
                check = true;
                Aggro();
            }
        }
    }

    void InitializeEnemy()
    {
        if (enemyData != null)
        {
            nameText = GetComponentInChildren<TextMeshPro>();
            nameText.text = GetRandomEnemyName();
            currentHP = enemyData.maxHP;
        }
    }

    public static string GetRandomEnemyName()
    {
        int randomIndex = Random.Range(0, enemyNames.Count);
        return enemyNames[randomIndex];
    }

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

        StartCoroutine(SwordAnimation());

        StartCoroutine(ResetMessage());

        if (currentHP <= 0)
        {
            StartCoroutine(SwordAnimation2());
            return;
        }
    }

    private IEnumerator SwordAnimation(){
        yield return new WaitForSeconds(1f);
        UpdateHPBar();
    }

    private IEnumerator SwordAnimation2(){
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

    public void Alert(){
        if(idleState){
            return;
        }
        idleState = true;
        message.color = Color.white;
        message.text = "??";
        message.gameObject.SetActive(true);
        StartCoroutine(ResetMessage());
    }

    public void Aggro(){
        if(aggroState == true){
            return;
        }else if(aggroState == false){
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Vector3 rotationToPlayer = (player.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(rotationToPlayer);
            transform.rotation = lookRotation; 
            idleState = true;
            aggroState = true;
            message.color = Color.red;
            message.text = "!!";
            message.gameObject.SetActive(true);
            StartCoroutine(ResetMessage());
            TurnManager.Instance.AddEnemyToTurnOrder(this);
        }
    }

    private bool CheckLineOfSight()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 directionToPlayer = player.transform.position - transform.position;
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= detectionRange)
        {
            RaycastHit hit;

            // Draw the ray in the Scene view for debugging
            Debug.DrawRay(transform.position + Vector3.up * 1f, directionToPlayer.normalized * detectionRange, Color.red, 1f);  // Draw in red initially
            
            if (Physics.Raycast(transform.position + Vector3.up * 1f, directionToPlayer.normalized, out hit, detectionRange))
            {
                // If the ray hits the player, make the ray green
                if (hit.transform.CompareTag("Player"))
                {
                    Debug.DrawRay(transform.position + Vector3.up * 1f, directionToPlayer.normalized * detectionRange, Color.green, 1f);  // Draw in green
                    return true;
                }
                else
                {
                    Debug.DrawRay(transform.position + Vector3.up * 1f, directionToPlayer.normalized * detectionRange, Color.red, 1f);  // Draw in red if it hits an obstacle
                    Debug.Log("Ray hit: " + hit.transform.name);  // Debug the object it hit
                }
            }
        }
        return false;
    }



    private IEnumerator ResetMessage()
    {
        yield return new WaitForSeconds(1f);

        message.gameObject.SetActive(false);
    }

    void Die()
    {
        TurnManager.Instance.RemoveEnemyFromTurnList(this);
        GridManager.Instance.RemoveEnemyFromGrid(this);
        MovementPlayer playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<MovementPlayer>();
        playerMovement.EnemyRemove(this);

        StartCoroutine(DeathAnimation());
        Destroy(gameObject, 2.5f);
    }

    private IEnumerator DeathAnimation(){
        yield return new WaitForSeconds(1f);
        _animator.SetTrigger("Death");
    }

    private void UpdateHPBar()
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

    public void FootstepSFX(){
        soundManager.walkSound();
    }

    public IEnumerator EnemyTurn()
    {
        if (aggroState)
        {
            if (IsAdjacent(GameObject.FindGameObjectWithTag("Player").transform.position, transform.position))
            {
                if(currentHP > 0){
                    AttackPlayer();
                }
            }else if(!IsAdjacent(GameObject.FindGameObjectWithTag("Player").transform.position, transform.position)){
                MoveTowardsPlayer();
            }
            yield return new WaitForSeconds(1f);
            TurnManager.Instance.EndTurn();  
        }
    }

    public void AttackPlayer(){
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 rotationToPlayer = (player.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(rotationToPlayer);
        transform.rotation = lookRotation; 
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
        float damageOutput = enemyData.damage * defenseFactor;

        GridManager.Instance.AttackPlayer((int)damageOutput);
    }

    bool IsAdjacent(Vector3 playerPosition, Vector3 enemyPosition)
    {
        Vector3 playerRounded = new Vector3(Mathf.Round(playerPosition.x), 0, Mathf.Round(playerPosition.z));
        Vector3 enemyRounded = new Vector3(Mathf.Round(enemyPosition.x), 0, Mathf.Round(enemyPosition.z));

        return (Mathf.Abs(playerRounded.x - enemyRounded.x) == 1 && playerRounded.z == enemyRounded.z) ||
            (Mathf.Abs(playerRounded.z - enemyRounded.z) == 1 && playerRounded.x == enemyRounded.x);
    }
    

    public void MoveTowardsPlayer()
    {
        if (aggroState)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Debug.Log("Move towards the player");
                List<Vector3> path = FindPath(transform.position, player.transform.position);
                if (path != null && path.Count > 0)
                {
                    StartCoroutine(MoveOneTileAtATime(path)); 
                }else{
                    Debug.LogError("Path is null or empty.");
                }
            }
        }
    }

    private IEnumerator MoveOneTileAtATime(List<Vector3> path)
    {
        _animator.SetTrigger("WalkEnemy");
        Vector3 destination = new Vector3(path[0].x, transform.position.y, path[0].z);

        Vector3 direction = (destination - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 1f) 
        {
            transform.rotation = targetRotation;
        }
        
        while (Vector3.Distance(transform.position, destination) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }


        transform.position = destination;
    }

    //A* Logic
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
                if (closedList.Contains(new Node(neighbor, null, 0, 0))) continue;
                if (!GridManager.Instance.IsValidTile(neighbor)) continue;

                float gCost = currentNode.GCost + 1;
                float hCost = GetHeuristic(neighbor, targetTile);
                Node neighborNode = new Node(neighbor, currentNode, gCost, hCost);

                if (!openList.Contains(neighborNode))
                {
                    openList.Add(neighborNode);
                }
            }
        }

        return null;
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
        return Mathf.Abs(start.x - target.x) + Mathf.Abs(start.z - target.z);  
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
