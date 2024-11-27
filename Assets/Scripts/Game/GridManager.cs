using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

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
    private EnemyDataSO lowEnemyData; // Assign in Inspector
    [SerializeField]
    private EnemyDataSO mediumEnemyData; // Assign in Inspector
    [SerializeField]
    private EnemyDataSO highEnemyData; // Assign in Inspector
    [SerializeField]
    private EnemyDataSO bossEnemyData; // Assign in Inspector

    [Header("Materials")]
    public Material normalStoneMaterial; // Public variable for normal stone material

    private GameObject[] decorationPrefabs;
    private Dictionary<Renderer, Material> originalTileMaterials = new Dictionary<Renderer, Material>(); // Store original materials
    private Animator _animator;

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

    void Start()
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
            return;
        }
        GenerateRooms();
        ConnectRooms();
        SpawnPlayer();
        SpawnEnemies();
    }

    void Update()
    {
        HandleInput();
        CheckEnemyPlayerProximity();
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

        for (int i = 0; i < 10 && roomPositions.Count > 0; i++)
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

    private List<GameObject> instantiatedTiles = new List<GameObject>();

    void CreateHallway(Vector3 start, Vector3 end)
    {
        Vector3 current = start;

        // Horizontal creation
        while (Mathf.RoundToInt(current.x) != Mathf.RoundToInt(end.x))
        {
            Vector3 hallwayPosition = new Vector3(current.x, -0.0015f, current.z);

            // Remove existing tile if present
            RemoveExistingTile(hallwayPosition);

            // Instantiate new tile
            GameObject newTile = Instantiate(hallwayPrefab, hallwayPosition, Quaternion.identity, transform);
            instantiatedTiles.Add(newTile);

            // Add to valid tile positions if it's not already present
            if (!validTilePositions.Contains(hallwayPosition))
            {
                validTilePositions.Add(hallwayPosition);
            }

            current.x += current.x < end.x ? 1 : -1;
        }

        // Vertical creation
        while (Mathf.RoundToInt(current.z) != Mathf.RoundToInt(end.z))
        {
            Vector3 hallwayPosition = new Vector3(current.x, -0.0020f, current.z);

            // Remove existing tile if present
            RemoveExistingTile(hallwayPosition);

            // Instantiate new tile
            GameObject newTile = Instantiate(hallwayPrefab, hallwayPosition, Quaternion.identity, transform);
            instantiatedTiles.Add(newTile);

            // Add to valid tile positions if it's not already present
            if (!validTilePositions.Contains(hallwayPosition))
            {
                validTilePositions.Add(hallwayPosition);
            }

            current.z += current.z < end.z ? 1 : -1;
        }
    }

    void RemoveExistingTile(Vector3 position)
    {
        // Assuming the tile is a GameObject that is at the given position, you can find it and delete it
        Collider[] hitColliders = Physics.OverlapSphere(position, 0.1f); // Adjust the radius as needed
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Decoration"))  // Replace "Tile" with the actual tag of the tiles
            {
                Destroy(hitCollider.gameObject);  // Destroy the tile GameObject
                break;  // Exit once we find the tile at the position
            }
        }
    }

    bool IsAdjacent(Vector3 playerPosition, Vector3 enemyPosition)
    {
        return (Mathf.Abs(playerPosition.x - enemyPosition.x) == 1 && Mathf.Abs(playerPosition.z - enemyPosition.z) == 0) ||
               (Mathf.Abs(playerPosition.x - enemyPosition.x) == 0 && Mathf.Abs(playerPosition.z - enemyPosition.z) == 1);
    }

    private float lastAttackTime = 0f;
    public float attackCooldown = 2f;
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
                    List<Vector3> path = playerMovement.FindPath(targetPosition, targetPosition);
                    HighlightPath(path);
                    return;
                }
                if (playerMovement != null && IsValidTile(targetPosition))
                {
                    List<Vector3> path = playerMovement.FindPath(playerMovement.transform.position, targetPosition);
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
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Tile"))
                {
                    Vector3 targetPosition = hit.collider.transform.position;
                    if (playerMovement != null && IsValidTile(targetPosition))
                    {
                        playerMovement.MoveTo(targetPosition);
                        ClearHighlightedPath(); // Clear highlights after clicking
                    }
                }
                else if (hit.collider.CompareTag("Enemy"))
                {
                    if (Time.time - lastAttackTime >= attackCooldown){
                        Debug.Log("Enemy Hit");
                        _animator = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
                        GameObject enemy = hit.collider.gameObject;
                        Vector3 enemyPosition = enemy.transform.position;
                        Vector3 playerPosition = playerMovement.transform.position;

                        // Check if the enemy is adjacent to the player
                        if (IsAdjacent(playerPosition, enemyPosition))
                        {
                            Debug.Log("Adjacent Hit");
                            string randomAttack = attackAnimations[Random.Range(0, attackAnimations.Length)];
                            _animator.SetTrigger(randomAttack);
                            Vector3 directionToFace = (enemyPosition - playerMovement.transform.position).normalized;
                            directionToFace.y = 0;
                            Quaternion lookRotation = Quaternion.LookRotation(directionToFace);

                            lastAttackTime = Time.time;

                            playerMovement.transform.rotation = lookRotation; // Adjust speed as needed
                            Debug.Log("Player Data Sword: " + playerData.sword.itemPoint);
                            Enemy enemyScript = enemy.GetComponent<Enemy>();
                            if (enemyScript != null)
                            {
                                enemyScript.TakeDamage(playerData.sword.itemPoint);
                                Debug.Log("Enemy HITS: " + playerData.sword.itemPoint);
                            }
                            else
                            {
                                Debug.LogWarning("Enemy script not found!");
                            }
                        }
                    }
                }
                else
                {
                    ClearHighlightedPath(); // Clear highlights if not on a tile
                }
            }
        }
    }

    void HighlightPath(List<Vector3> path)
    {
        ClearHighlightedPath(); // Clear previous highlights

        foreach (Vector3 position in path)
        {
            Collider[] colliders = Physics.OverlapSphere(position, 0.1f);
            foreach (Collider collider in colliders)
            {
                Renderer renderer = collider.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Only store the original material if not already stored
                    if (!originalTileMaterials.ContainsKey(renderer))
                    {
                        originalTileMaterials[renderer] = renderer.material; // Store original material
                    }

                    // Highlight the tile by changing its material color
                    Material highlightMaterial = new Material(renderer.material);  // Create a unique instance for each tile
                    highlightMaterial.color = Color.white; // Apply white color for highlight
                    renderer.material = highlightMaterial; // Set the new material for the tile

                    highlightedPathRenderers.Add(renderer);
                }
            }
        }
    }

    void ClearHighlightedPath()
    {
        foreach (Renderer renderer in highlightedPathRenderers)
        {
            // If the original material is stored, restore it
            if (originalTileMaterials.ContainsKey(renderer))
            {
                renderer.material = originalTileMaterials[renderer];  // Restore original material
            }
        }

        highlightedPathRenderers.Clear();  // Clear list of highlighted tiles
        originalTileMaterials.Clear();     // Clear the dictionary of original materials
    }

    public Vector3 GetRandomBlankTilePosition()
    {
        List<Vector3> blankTiles = new List<Vector3>();

        foreach (Vector3 position in validTilePositions)
        {
            Collider[] colliders = Physics.OverlapSphere(position, 0.1f);
            bool isOccupied = false;

            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Decoration") || collider.CompareTag("Enemy") || collider.CompareTag("Player") || collider.CompareTag("Enemy_Tile"))
                {
                    isOccupied = true;
                    break;
                }
            }

            if (!isOccupied)
            {
                blankTiles.Add(position);
            }
        }

        if (blankTiles.Count > 0)
        {
            return blankTiles[Random.Range(0, blankTiles.Count)];
        }

        Debug.LogWarning("No valid blank tiles available for spawning.");
        return Vector3.zero; // Fallback value
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

        // Assign the player to the camera
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.setPlayer(player.transform); // Ensure this is properly assigned
        }
        else
        {
            Debug.LogError("CameraFollow instance not found.");
        }
    }

    void SpawnEnemies()
    {
        int totalEnemies = Mathf.RoundToInt((playerData.selectedFloor * 0.2f) + 7);

        // Get room-to-tile mapping
        Dictionary<Rect, List<Vector3>> roomTileMapping = GridManager.Instance.GetRoomTileMapping();

        // Ensure rooms are valid
        if (roomTileMapping.Count == 0)
        {
            Debug.LogError("No valid rooms found for spawning enemies.");
            return;
        }

        List<Rect> rooms = new List<Rect>(roomTileMapping.Keys);
        List<Vector3> occupiedTiles = new List<Vector3>(); // Tracks enemy positions and their buffer zones

        // Calculate initial even distribution
        int enemiesPerRoom = totalEnemies / rooms.Count;
        int remainingEnemies = totalEnemies % rooms.Count;

        // Spawn enemies in each room
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
                    occupiedTiles.Add(spawnPosition); // Add position to occupied list
                    roomTiles.Remove(spawnPosition); // Remove from room's valid tiles

                    string enemyType = GetRandomEnemyType(playerData.selectedFloor);
                    SpawnEnemyAtPosition(spawnPosition, enemyType);
                }
            }
        }

        // Distribute remaining enemies randomly among rooms
        while (remainingEnemies > 0)
        {
            Rect randomRoom = rooms[Random.Range(0, rooms.Count)];
            List<Vector3> roomTiles = roomTileMapping[randomRoom];
            List<Vector3> validTiles = GridManager.Instance.GetValidTilesWithBuffer(roomTiles, occupiedTiles);

            if (validTiles.Count > 0)
            {
                Vector3 spawnPosition = validTiles[Random.Range(0, validTiles.Count)];
                spawnPosition.y += 0.4f;
                occupiedTiles.Add(spawnPosition); // Add position to occupied list
                roomTiles.Remove(spawnPosition); // Remove from room's valid tiles

                string enemyType = GetRandomEnemyType(playerData.selectedFloor);
                SpawnEnemyAtPosition(spawnPosition, enemyType);
                remainingEnemies--;
            }
        }
    }

    string GetRandomEnemyType(int floor)
    {
        // Dynamic probabilities: Low, Medium, High
        float lowChance = Mathf.Clamp01(1.0f - (floor * 0.05f)); // Decreases with floor level
        float mediumChance = Mathf.Clamp01(0.3f + (floor * 0.03f)); // Increases with floor level
        float highChance = Mathf.Clamp01(0.1f + (floor * 0.02f)); // Increases with floor level

        // Normalize probabilities to ensure they sum to 1
        float totalChance = lowChance + mediumChance + highChance;
        lowChance /= totalChance;
        mediumChance /= totalChance;
        highChance /= totalChance;

        // Randomly determine enemy type based on adjusted chances
        float randomValue = Random.value;
        if (randomValue < lowChance) return "Low"; // Low-tier enemy
        else if (randomValue < lowChance + mediumChance) return "Medium"; // Medium-tier enemy
        else return "High"; // High-tier enemy
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
        TagTileAsEnemyTile(position);
    }

    void TagTileAsEnemyTile(Vector3 position)
    {
        // Assuming the tile is a GameObject that is at the given position, you can find it and set the tag
        Collider[] hitColliders = Physics.OverlapSphere(position, 0.1f); // Adjust the radius as needed
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Tile"))  // Replace "Tile" with the actual tag of the tiles
            {
                hitCollider.gameObject.tag = "Enemy_Tile";  // Assign the "EnemyTile" tag to the tile
                break;  // Exit once we find the tile at the position
            }
        }
    }

    void CheckEnemyPlayerProximity()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player"); // Ensure the player is tagged correctly

        if (player == null) return;

        Vector3 playerPosition = player.transform.position;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Vector3 enemyPosition = enemy.transform.position;

            float distanceToPlayer = Vector3.Distance(playerPosition, enemyPosition);

            Enemy enemyScript = enemy.GetComponent<Enemy>();

            if (distanceToPlayer <= 6f)
            {
                if (enemyScript != null)
                {
                    enemyScript.alert(); // Assuming Alert() is the method in the Enemy script for the alert state
                }
            }

            if (distanceToPlayer <= 3f)
            {
                // Rotate the enemy to face the player
                enemyScript.aggro();
                Vector3 directionToPlayer = (playerPosition - enemyPosition).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * 5f); // Smooth rotation
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
