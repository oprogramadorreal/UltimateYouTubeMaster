using UnityEngine;

[RequireComponent(typeof(Planet))]
[RequireComponent(typeof(Player))]
public sealed class BlackHole : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        Planet = GetComponent<Planet>();
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (!Planet.CanUpdate())
        {
            return;
        }

        if (Radius >= TargetRadius)
        {
            EndOfTheArena();
        }
    }

    public Planet Planet { get; private set; }

    public Vector3 Center { get => Planet.Center; }

    public float Radius { get => Planet.Radius; }

    public void SetRadius(float radius)
    {
        Planet.SetRadius(radius);
    }

    public float TargetRadius
    {
        get => Planet.TargetRadius;
        set => Planet.TargetRadius = value;
    }

    private void EndOfTheArena()
    {
        //arena.Reset();

        //var agent = GetComponentsInTrainingUnit<PlayerAI_ML>(true)
        //    .FirstOrDefault(p => !ReferenceEquals(this, p));

        //var allOtherPlayers = GetComponentsInTrainingUnit<Player>(true)
        //    .Where(p => !p.isBlackHole
        //        && !ReferenceEquals(this, p)
        //        && (agent == null || !ReferenceEquals(agent.gameObject, p.gameObject))
        //    ).ToList();

        //foreach (var p in allOtherPlayers)
        //{
        //    p.RespawnRandom();
        //}

        //agent?.EndEpisode();
    }
}
