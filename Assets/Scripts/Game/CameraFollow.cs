using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    public Transform player;
    public Vector3 pivotOffset = new Vector3(0, 0, 0);
    public float smoothSpeed = 0.125f;

    private Transform mainCamera;

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

        // Find the Main Camera as a child of this pivot
        mainCamera = transform.Find("Main Camera");
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found as a child of Pivot Camera.");
        }
    }

    void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogWarning("Player transform is not assigned to CameraFollow.");
            return;
        }

        // Smoothly follow the player with the pivot
        Vector3 desiredPosition = player.position + pivotOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    public void setPlayer(Transform newPlayer)
    {
        player = newPlayer;
        Debug.Log("Camera is now following: " + player.name);
    }
}
