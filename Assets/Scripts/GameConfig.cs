using UnityEngine;

public sealed class GameConfig : MonoBehaviour
{
    [SerializeField]
    private int numberOfSubsPlayers = 10;

    [SerializeField]
    private int maxPlanetRadius = 100;

    public int NumberOfSubsPlayers { get => numberOfSubsPlayers; }

    public int MaxPlanetRadius { get => maxPlanetRadius; }

    private void Awake()
    {
        Random.InitState(42);
        Physics.autoSimulation = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Physics.autoSimulation = !Physics.autoSimulation;
        }
    }
}
