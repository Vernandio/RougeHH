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

    [Header("Materials")]
    public Material normalStoneMaterial; // Public variable for normal stone material

    private GameObject[] decorationPrefabs;
    private Dictionary<Renderer, Material> originalTileMaterials = new Dictionary<Renderer, Material>(); // Store original materials

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
        GenerateRooms();
        ConnectRooms();
        SpawnPlayer();
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

        for (int i = 0; i < 5 && roomPositions.Count > 0; i++)
        {
            Vector3 position = roomPositions[0];
            roomPositions.RemoveAt(0);

            Instantiate(stonePrefab, position, Quaternion.identity, transform);
            validTilePositions.Add(position);
        }

        for (int i = 0; i < 3 && roomPositions.Count > 0; i++)
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
                    Vector3 centerA = new Vector3(Mathf.RoundToInt(roomA.center.x), 0, Mathf.RoundToInt(roomA.center.y));
                    Vector3 centerB = new Vector3(Mathf.RoundToInt(roomB.Value.center.x), 0, Mathf.RoundToInt(roomB.Value.center.y));

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
            Vector3 hallwayPosition = new Vector3(current.x, 0.0005f, current.z);
            Instantiate(hallwayPrefab, hallwayPosition, Quaternion.identity, transform);

            if (!validTilePositions.Contains(hallwayPosition))
            {
                validTilePositions.Add(hallwayPosition);
            }

            current.x += current.x < end.x ? 1 : -1;
        }

        while (Mathf.RoundToInt(current.z) != Mathf.RoundToInt(end.z))
        {
            Vector3 hallwayPosition = new Vector3(current.x, 0.0005f, current.z);
            Instantiate(hallwayPrefab, hallwayPosition, Quaternion.identity, transform);

            if (!validTilePositions.Contains(hallwayPosition))
            {
                validTilePositions.Add(hallwayPosition);
            }

            current.z += current.z < end.z ? 1 : -1;
        }
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

    void Update()
    {
        HandleInput();
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
                if (playerMovement != null && IsValidTile(targetPosition))
                {
                    List<Vector3> path = playerMovement.FindPath(playerMovement.transform.position, targetPosition);
                    HighlightPath(path);
                }
            }
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
}
