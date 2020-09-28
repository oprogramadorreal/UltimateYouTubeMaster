using UnityEngine;

[RequireComponent(typeof(Planet))]
[RequireComponent(typeof(Player))]
public sealed class PlayerArenaBoundsEnforcer : MonoBehaviour
{
    private Planet planet;
    private Player player;
    private Rigidbody playerRigidbody;
    private Arena arena;

    private void Awake()
    {
        planet = GetComponent<Planet>();
        player = GetComponent<Player>();
        playerRigidbody = GetComponentInChildren<Rigidbody>();
    }

    private void Start()
    {
        arena = player.GetArena();
    }

    private void Update()
    {
        if (!planet.CanUpdate())
        {
            return;
        }

        if (player.HasCurrentActiveCamera())
        {
            arena.UpdateShader(planet);
        }
    }

    private void FixedUpdate()
    {
        if (!planet.CanUpdate())
        {
            return;
        }

        UpdatePlayerVelocity();
    }

    private void UpdatePlayerVelocity()
    {
        var toCenterOfArenaDir = arena.Center - planet.Center;

        if (!Mathf.Approximately(toCenterOfArenaDir.magnitude, 0.0f))
        {
            if (toCenterOfArenaDir.magnitude + planet.Radius > arena.Radius)
            {
                if (Vector3.Dot(playerRigidbody.velocity, -toCenterOfArenaDir) > 0.0f)
                {
                    playerRigidbody.velocity = Vector3.Reflect(playerRigidbody.velocity, toCenterOfArenaDir.normalized);
                    playerRigidbody.velocity *= 0.9f;
                }

                player.Move(toCenterOfArenaDir.normalized);
            }

            // always drag into the center of the arena
            //playerRigidbody.AddForce(toCenterOfArenaDir.normalized * (1000.0f / toCenterOfArenaDir.magnitude));
        }
    }
}
