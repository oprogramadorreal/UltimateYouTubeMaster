using UnityEngine;

[RequireComponent(typeof(Planet))]
[RequireComponent(typeof(Player))]
public sealed class Player_Sub : MonoBehaviour
{
    private Planet planet;
    private Player player;

    private void Awake()
    {
        planet = GetComponent<Planet>();
        player = GetComponent<Player>();
    }

    private void Start()
    {
        player.Respawn(player.GetValidRandomPositionInsideArena(), planet.Radius);
    }
}
