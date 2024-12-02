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

        mainCamera = transform.Find("Main Camera");
    }

    void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        Vector3 desiredPosition = player.position + pivotOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }
}
