using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public enum Turn { Player, Enemy }
    public Turn currentTurn = Turn.Player;

    public static GridManager Instance { get; private set; }
    public GameManager gameManager;
    public AudioSource audioSource;
    public AudioClip combatClip;
    public AudioClip normalClip;

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject hallwayPrefab;
    public GameObject playerPrefab;

    [Header("Grid Settings")]
    public int gridWidth = 50;
    public int gridHeight = 50;
    public int roomCount = 5;
    public int minRoomSize = 6;
    public int maxRoomSize = 12;
    public int maxAttempts = 100;
    public int bufferSize = 1;
    private string[] attackAnimations = {"Attack3", "Attack", "Attack2"};

    private List<Rect> rooms = new List<Rect>();
    private List<Vector3> validTilePositions = new List<Vector3>();
    private MovementPlayer playerMovement;
    private HashSet<(Rect, Rect)> connectedRoomPairs = new HashSet<(Rect, Rect)>();
    private List<Renderer> highlightedPathRenderers = new List<Renderer>();
    public List<Enemy> enemies;

    [Header("Decoration Prefabs")]
    public GameObject stonePrefab;
    public GameObject tablePrefab;
    public GameObject barrelPrefab;
    public GameObject pillarPrefab;
    public GameObject fireplacePrefab;
    public GameObject enemyLowPrefab;
    public GameObject enemyMidPrefab;
    public GameObject enemyHighPrefab;
    public PlayerDataSO playerData;

    [SerializeField]
    private EnemyDataSO lowEnemyData;
    [SerializeField]
    private EnemyDataSO mediumEnemyData;
    [SerializeField]
    private EnemyDataSO highEnemyData;
    [SerializeField]
    private EnemyDataSO bossEnemyData;

    [Header("Materials")]
    public Material normalStoneMaterial;
    private GameObject[] decorationPrefabs;
    private Dictionary<Renderer, Material> originalTileMaterials = new Dictionary<Renderer, Material>();
    private Animator _animator;
    private bool combatMusic = false;
    private List<GameObject> instantiatedTiles = new List<GameObject>();

    private float lastAttackTime = 0f;
    public float attackCooldown = 2f;
    public bool turn = false;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        decorationPrefabs = new GameObject[] { tablePrefab, barrelPrefab, pillarPrefab, fireplacePrefab };
        if(playerData.selectedFloor == -30){
            roomCount = 1;
            minRoomSize = 8;
            maxRoomSize = 8;
            maxAttempts = 100;
            bufferSize = 1;
            GenerateRooms();
            SpawnPlayer();
            SpawnEnemies();
        }else{
            GenerateRooms();
            ConnectRooms();
            SpawnPlayer();
            SpawnEnemies();
        }

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
        CheckEnemyAggro();
        HandleInput();
        CheckEnemyPlayerProximity();
        if(combatMusic){
            audioSource.clip = combatClip;
            if (!audioSource.isPlaying) 
            {
                audioSource.Play();
            }
        }else{
            audioSource.clip = normalClip;
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }

    void CheckEnemyAggro()
    {
        combatMusic = false;
        foreach (Enemy enemy in enemies)
        {
            if (enemy.aggroState)
            {
                combatMusic = true;
                break;
            }
        }
    }

    void GenerateRooms()
    {
        int attempts = 0;

        while (rooms.Count < roomCount && attempts < maxAttempts)
        {
            attempts++;

            int roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);
            int roomX = Random.Range(0, gridWidth - roomWidth);
            int roomZ = Random.Range(0, gridHeight - roomHeight);

            Rect newRoom = new Rect(roomX, roomZ, roomWidth, roomHeight);

            bool overlaps = false;
            foreach (Rect room in rooms)
            {
                Rect expandedRoom = new Rect(
                    room.xMin - bufferSize,
                    room.yMin - bufferSize,
                    room.width + 2 * bufferSize,
                    room.height + 2 * bufferSize
                );

                if (newRoom.Overlaps(expandedRoom))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                rooms.Add(newRoom);
                CreateRoom(newRoom);
            }
        }

        if (rooms.Count < roomCount)
        {
            Debug.LogWarning("Not enough rooms generated. Increase grid size or maxAttempts.");
        }
    }

    void CreateRoom(Rect room)
    {
        float tileSize = 1.0f;

        List<Vector3> roomPositions = new List<Vector3>();
        for (int x = (int)room.xMin; x < (int)room.xMax; x++)
        {
            for (int z = (int)room.yMin; z < (int)room.yMax; z++)
            {
                Vector3 position = new Vector3(x * tileSize, 0, z * tileSize);
                roomPositions.Add(position);
            }
        }

        ShuffleList(roomPositions);

        for (int i = 0; i < 10 && roomPositions.Count > 0; i++)
        {
            Vector3 position = roomPositions[0];
            roomPositions.RemoveAt(0);

            Instantiate(stonePrefab, position, Quaternion.identity, transform);
            validTilePositions.Add(position);
        }

        for (int i = 0; i < 7 && roomPositions.Count > 0; i++)
        {
            Vector3 position = roomPositions[0];
            roomPositions.RemoveAt(0);

            GameObject randomPrefab = decorationPrefabs[Random.Range(0, decorationPrefabs.Length)];
            Instantiate(randomPrefab, position, Quaternion.identity, transform);
        }

        foreach (Vector3 position in roomPositions)
        {
            Instantiate(tilePrefab, position, Quaternion.identity, transform);
            validTilePositions.Add(position);
        }
    }

    void ShuffleList(List<Vector3> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            Vector3 temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    void ConnectRooms()
    {
        if (rooms.Count == 0) return;

        List<Rect> connectedRooms = new List<Rect>();
        connectedRooms.Add(rooms[0]);

        while (connectedRooms.Count < rooms.Count)
        {
            Rect roomA = connectedRooms[Random.Range(0, connectedRooms.Count)];
            Rect? roomB = FindUnconnectedRoom(connectedRooms);

            if (roomB.HasValue)
            {
                if (!IsRoomConnected(roomA, roomB.Value))
                {
                    Vector3 centerA = new Vector3(Mathf.RoundToInt(roomA.center.x), -0.005f, Mathf.RoundToInt(roomA.center.y));
                    Vector3 centerB = new Vector3(Mathf.RoundToInt(roomB.Value.center.x), -0.010f, Mathf.RoundToInt(roomB.Value.center.y));

                    CreateHallway(centerA, centerB);
                    connectedRooms.Add(roomB.Value);

                    connectedRoomPairs.Add((roomA, roomB.Value));
                    connectedRoomPairs.Add((roomB.Value, roomA));
                }
            }
        }
    }

    bool IsRoomConnected(Rect roomA, Rect roomB)
    {
        return connectedRoomPairs.Contains((roomA, roomB)) || connectedRoomPairs.Contains((roomB, roomA));
    }

    Rect? FindUnconnectedRoom(List<Rect> connectedRooms)
    {
        foreach (Rect room in rooms)
        {
            if (!connectedRooms.Contains(room))
            {
                return room;
            }
        }
        return null;
    }

    void CreateHallway(Vector3 start, Vector3 end)
    {
        Vector3 current = start;

        while (Mathf.RoundToInt(current.x) != Mathf.RoundToInt(end.x))
        {
            Vector3 hallwayPosition = new Vector3(current.x, -0.0015f, current.z);

            RemoveExistingTile(hallwayPosition);

            GameObject newTile = Instantiate(hallwayPrefab, hallwayPosition, Quaternion.identity, transform);
            instantiatedTiles.Add(newTile);

            if (!validTilePositions.Contains(hallwayPosition))
            {
                validTilePositions.Add(hallwayPosition);
            }

            current.x += current.x < end.x ? 1 : -1;
        }

        while (Mathf.RoundToInt(current.z) != Mathf.RoundToInt(end.z))
        {
            Vector3 hallwayPosition = new Vector3(current.x, -0.0020f, current.z);

            RemoveExistingTile(hallwayPosition);

            GameObject newTile = Instantiate(hallwayPrefab, hallwayPosition, Quaternion.identity, transform);
            instantiatedTiles.Add(newTile);

            if (!validTilePositions.Contains(hallwayPosition))
            {
                validTilePositions.Add(hallwayPosition);
            }

            current.z += current.z < end.z ? 1 : -1;
        }
    }

    void RemoveExistingTile(Vector3 position)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, 0.1f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Decoration")) 
            {
                Destroy(hitCollider.gameObject);
                break; 
            }
        }
    }

    public bool IsValidTile(Vector3 position)
    {
        foreach (Vector3 validPosition in validTilePositions)
        {
            if (Vector3.Distance(validPosition, position) < 0.1f)
            {
                Collider[] colliders = Physics.OverlapSphere(validPosition, 0.1f);
                foreach (Collider collider in colliders)
                {
                    if (collider.CompareTag("Decoration"))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        return false;
    }

    public void RemoveEnemyFromGrid(Enemy enemy){
        enemies.Remove(enemy);
    }
    void HandleInput()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Tile"))
            {
                Vector3 targetPosition = hit.collider.transform.position;
                if (playerMovement.isMoving)
                {
                    List<Vector3> path = playerMovement.findPath(targetPosition, targetPosition);
                    HighlightPath(path);
                }
                if (playerMovement != null && IsValidTile(targetPosition))
                {
                    List<Vector3> path = playerMovement.findPath(playerMovement.transform.position, targetPosition);
                    HighlightPath(path);
                }
            }
            else
            {
                ClearHighlightedPath(); // Clear highlights if not on a tile
            }
        }
        else
        {
            ClearHighlightedPath(); // Clear highlights if not on a tile
        }


        if (Input.GetMouseButtonDown(0))
        {
            if(!playerMovement.isPlayerTurn){
                return;
            }
            if (Physics.Raycast(ray, out hit) && turn)
            {
                turn = false;
                if (hit.collider.CompareTag("Tile"))
                {
                    if(playerMovement.isMoving){
                        playerMovement.isMoving = false;
                        return;
                    }
                    Vector3 targetPosition = hit.collider.transform.position;
                    if (playerMovement != null && IsValidTile(targetPosition))
                    {
                        playerMovement.MoveTo(targetPosition, CheckEnemyStates());
                        ClearHighlightedPath(); 
                    }
                }
                else if (hit.collider.CompareTag("Enemy"))
                {
                    if (Time.time - lastAttackTime >= attackCooldown){
                        _animator = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
                        GameObject enemy = hit.collider.gameObject;
                        Vector3 enemyPosition = enemy.transform.position;
                        Vector3 playerPosition = playerMovement.transform.position;
                        
                        if (IsAdjacent(playerPosition, enemyPosition))
                        {
                            gameManager.action();
                            playerMovement.isAttack = true;
                            string randomAttack = attackAnimations[Random.Range(0, attackAnimations.Length)];
                            _animator.SetTrigger(randomAttack);
                            Vector3 directionToFace = (enemyPosition - playerMovement.transform.position).normalized;
                            directionToFace.y = 0;
                            Quaternion lookRotation = Quaternion.LookRotation(directionToFace);

                            lastAttackTime = Time.time;

                            playerMovement.transform.rotation = lookRotation;
                            Enemy enemyScript = enemy.GetComponent<Enemy>();
                            if (enemyScript != null)
                            {
                                GameObject skill1 = GameObject.FindGameObjectWithTag("Passive_1");
                                GameObject skill2 = GameObject.FindGameObjectWithTag("Physical_1");
                                GameObject skill3 = GameObject.FindGameObjectWithTag("Passive_2");
                                int damage = playerData.sword.itemPoint;
                                if(skill1 != null){
                                    gameManager.lifeSteal(damage * 20 / 100);
                                }
                                if(skill2 != null){
                                    damage += damage * 150 / 100;
                                    skill2.SetActive(false);
                                    gameManager.setActiveCooldown();
                                }
                                if(skill3 != null){
                                    damage += damage * 20 / 100;
                                }

                                enemyScript.TakeDamage(damage);
                                StartCoroutine(playerMovement.EndPlayerTurn());
                            }
                        }
                    }
                }
                else
                {
                    ClearHighlightedPath(); 
                }
            }
        }
    }

    bool IsAdjacent(Vector3 playerPosition, Vector3 enemyPosition)
    {
        Vector3 playerRounded = new Vector3(Mathf.Round(playerPosition.x), 0, Mathf.Round(playerPosition.z));
        Vector3 enemyRounded = new Vector3(Mathf.Round(enemyPosition.x), 0, Mathf.Round(enemyPosition.z));

        return (Mathf.Abs(playerRounded.x - enemyRounded.x) == 1 && playerRounded.z == enemyRounded.z) ||
            (Mathf.Abs(playerRounded.z - enemyRounded.z) == 1 && playerRounded.x == enemyRounded.x);
    }

    bool CheckEnemyStates()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy.idleState || enemy.aggroState)
            {
                return true;
            }
        }
        return false;
    }

    public void attackPlayer(int damage){
        gameManager.PlayerTakeDamage(damage);
    }

    void HighlightPath(List<Vector3> path)
    {
        ClearHighlightedPath(); 

        foreach (Vector3 position in path)
        {
            Collider[] colliders = Physics.OverlapSphere(position, 0.1f);
            foreach (Collider collider in colliders)
            {
                Renderer renderer = collider.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (!originalTileMaterials.ContainsKey(renderer))
                    {
                        originalTileMaterials[renderer] = renderer.material; 
                    }

                    Material highlightMaterial = new Material(renderer.material); 
                    highlightMaterial.color = Color.white; 
                    renderer.material = highlightMaterial; 

                    highlightedPathRenderers.Add(renderer);
                }
            }
        }
    }

    void ClearHighlightedPath()
    {
        foreach (Renderer renderer in highlightedPathRenderers)
        {
            if (originalTileMaterials.ContainsKey(renderer))
            {
                renderer.material = originalTileMaterials[renderer];  
            }
        }

        highlightedPathRenderers.Clear();
        originalTileMaterials.Clear();  
    }

    void SpawnPlayer()
    {
        if (validTilePositions.Count == 0)
        {
            Debug.LogError("No valid tiles for player spawn.");
            return;
        }

        int randomIndex = Random.Range(0, validTilePositions.Count);
        Vector3 spawnPosition = validTilePositions[randomIndex];
        spawnPosition.y += 0.4f;

        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        playerMovement = player.GetComponent<MovementPlayer>();

        if (playerMovement == null)
        {
            Debug.LogError("MovementPlayer component not found on player prefab.");
        }

        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.setPlayer(player.transform);
        }
        else
        {
            Debug.LogError("CameraFollow instance not found.");
        }
    }

    void SpawnEnemies()
    {
        int totalEnemies = Mathf.RoundToInt((playerData.selectedFloor * 0.2f) + 7);

        Dictionary<Rect, List<Vector3>> roomTileMapping = GridManager.Instance.GetRoomTileMapping();

        if (roomTileMapping.Count == 0)
        {
            Debug.LogError("No valid rooms found for spawning enemies.");
            return;
        }

        List<Rect> rooms = new List<Rect>(roomTileMapping.Keys);
        List<Vector3> occupiedTiles = new List<Vector3>(); 

        int enemiesPerRoom = totalEnemies / rooms.Count;
        int remainingEnemies = totalEnemies % rooms.Count;

        foreach (Rect room in rooms)
        {
            List<Vector3> roomTiles = roomTileMapping[room];

            for (int i = 0; i < enemiesPerRoom; i++)
            {
                List<Vector3> validTiles = GridManager.Instance.GetValidTilesWithBuffer(roomTiles, occupiedTiles);

                if (validTiles.Count > 0)
                {
                    Vector3 spawnPosition = validTiles[Random.Range(0, validTiles.Count)];
                    spawnPosition.y += 0.4f;
                    occupiedTiles.Add(spawnPosition); 
                    roomTiles.Remove(spawnPosition); 

                    string enemyType = GetRandomEnemyType(playerData.selectedFloor);
                    SpawnEnemyAtPosition(spawnPosition, enemyType);
                }
            }
        }

        while (remainingEnemies > 0)
        {
            Rect randomRoom = rooms[Random.Range(0, rooms.Count)];
            List<Vector3> roomTiles = roomTileMapping[randomRoom];
            List<Vector3> validTiles = GridManager.Instance.GetValidTilesWithBuffer(roomTiles, occupiedTiles);

            if (validTiles.Count > 0)
            {
                Vector3 spawnPosition = validTiles[Random.Range(0, validTiles.Count)];
                spawnPosition.y += 0.4f;
                occupiedTiles.Add(spawnPosition); 
                roomTiles.Remove(spawnPosition);

                string enemyType = GetRandomEnemyType(playerData.selectedFloor);
                SpawnEnemyAtPosition(spawnPosition, enemyType);
                remainingEnemies--;
            }
        }
    }

    string GetRandomEnemyType(int floor)
    {
        float lowChance = Mathf.Clamp01(1.0f - (floor * 0.05f)); 
        float mediumChance = Mathf.Clamp01(0.3f + (floor * 0.03f)); 
        float highChance = Mathf.Clamp01(0.1f + (floor * 0.02f));

        float totalChance = lowChance + mediumChance + highChance;
        lowChance /= totalChance;
        mediumChance /= totalChance;
        highChance /= totalChance;

        float randomValue = Random.value;
        if (randomValue < lowChance) return "Low";
        else if (randomValue < lowChance + mediumChance) return "Medium"; 
        else return "High";
    }

    void SpawnEnemyAtPosition(Vector3 position, string type)
    {
        GameObject enemyPrefab;
        switch(type)
        {
            case "Low":
                enemyPrefab = enemyLowPrefab;
                break;
            case "Medium":
                enemyPrefab = enemyMidPrefab;
                break;
            case "High":
                enemyPrefab = enemyHighPrefab;
                break;
            default:
                Debug.LogError("Unknown enemy type!");
                return;
        }
        if(playerData.selectedFloor == -30){
            enemyPrefab = enemyHighPrefab;
            enemyPrefab.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        }else{
            enemyPrefab.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        }
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.tag = "Enemy";

        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if(playerData.selectedFloor == -30){
            enemyComponent.enemyData = bossEnemyData;
        }else{
            if (enemyComponent != null)
            {
                switch (type)
                {
                    case "Low":
                        enemyComponent.enemyData = lowEnemyData;
                        break;
                    case "Medium":
                        enemyComponent.enemyData = mediumEnemyData;
                        break;
                    case "High":
                        enemyComponent.enemyData = highEnemyData;
                        break;
                    default:
                        Debug.LogError("Unknown enemy type!");
                        break;
                }
            }
            else
            {
                Debug.LogError("Enemy script not found on the prefab!");
            }
        }
    }

    void CheckEnemyPlayerProximity()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null) return;

        Vector3 playerPosition = player.transform.position;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Vector3 enemyPosition = enemy.transform.position;

            float distanceToPlayer = Vector3.Distance(playerPosition, enemyPosition);

            Enemy enemyScript = enemy.GetComponent<Enemy>();

            if (distanceToPlayer <= 5f)
            {
                enemyScript.alert(); 
            }
        }
    }

    public List<Vector3> GetValidTilesWithBuffer(List<Vector3> roomTiles, List<Vector3> occupiedTiles, int buffer = 2)
    {
        List<Vector3> validTiles = new List<Vector3>();

        foreach (Vector3 tile in roomTiles)
        {
            bool isWithinBuffer = false;

            foreach (Vector3 occupiedTile in occupiedTiles)
            {
                if (Vector3.Distance(tile, occupiedTile) < buffer)
                {
                    isWithinBuffer = true;
                    break;
                }
            }

            if (!isWithinBuffer)
            {
                validTiles.Add(tile);
            }
        }

        return validTiles;
    }

    public Dictionary<Rect, List<Vector3>> GetRoomTileMapping()
    {
        Dictionary<Rect, List<Vector3>> roomTileMapping = new Dictionary<Rect, List<Vector3>>();

        foreach (Rect room in rooms)
        {
            List<Vector3> roomTiles = new List<Vector3>();

            foreach (Vector3 position in validTilePositions)
            {
                if (room.Contains(new Vector2(position.x, position.z)))
                {
                    Collider[] colliders = Physics.OverlapSphere(position, 0.1f);
                    bool isOccupied = false;

                    foreach (Collider collider in colliders)
                    {
                        if (collider.CompareTag("Decoration") || collider.CompareTag("Enemy") || collider.CompareTag("Player"))
                        {
                            isOccupied = true;
                            break;
                        }
                    }

                    if (!isOccupied)
                    {
                        roomTiles.Add(position);
                    }
                }
            }

            roomTileMapping[room] = roomTiles;
        }

        return roomTileMapping;
    }
}
