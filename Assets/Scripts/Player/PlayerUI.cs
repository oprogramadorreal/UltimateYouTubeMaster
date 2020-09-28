using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(Planet))]
public sealed class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private Camera targetCamera;

    private Planet planet;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        GetComponent<Canvas>().worldCamera = targetCamera;
        planet = GetComponentInParent<Planet>();
    }

    private void Start()
    {
        // For subscribers, names will be set only in "Awake" method of PlayersManager.
        // Then here in "Start" is guaranteed that we already have a name.
        GetComponentInChildren<Text>().text = planet.transform.gameObject.name;
    }
}
