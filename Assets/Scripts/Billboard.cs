using UnityEngine;

/// <summary>
/// Based on https://youtu.be/BLfNP4Sc_iA
/// </summary>
public sealed class Billboard : MonoBehaviour
{
    [SerializeField]
    private Camera targetCamera;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + targetCamera.transform.forward);
    }
}
