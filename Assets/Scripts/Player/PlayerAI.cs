using UnityEngine;

[RequireComponent(typeof(Planet))]
[RequireComponent(typeof(Player))]
public sealed class PlayerAI : MonoBehaviour
{
    [SerializeField]
    private bool canAttack = true;

    private Planet planet;
    private Player player;

    private void Awake()
    {
        planet = GetComponent<Planet>();
        player = GetComponent<Player>();

        player.WasSwallowed += Player_WasSwallowed;
    }

    private void Player_WasSwallowed(object sender, System.EventArgs e)
    {
        
    }

    private void FixedUpdate()
    {
        if (!planet.CanUpdate())
        {
            return;
        }

        var movement = CalculatePlayerMovement();

        if (movement != Vector3.zero)
        {
            MovePlayer(movement);
        }
    }

    private Vector3 CalculatePlayerMovement()
    {
        var movement = CalculateForceToRunAwatFromTheBlackHole() * Vector3.Normalize(planet.Center - player.GetArena().Center);

        var closePlanetInfo = player.GetClosestPlayer();

        if (closePlanetInfo != null)
        {
            movement += CalculatePlayerMovementFor(closePlanetInfo);
        }

        return movement;
    }

    private float CalculateForceToRunAwatFromTheBlackHole()
    {
        const float maxForce = 50.0f;
        var distance = planet.GetDistanceToTheBlackHole();

        if (distance < 1.0f)
        {
            return maxForce;
        }

        return maxForce / (distance * distance);
    }

    private Vector3 CalculatePlayerMovementFor(ClosePlayerInfo otherPlayer)
    {
        var movement = Vector3.zero;

        var otherPlanet = otherPlayer.Planet;

        if (player.CanSwallow(otherPlayer))
        {
            if (canAttack) // move closer
            {
                if (otherPlayer.IsOccludedByBlackHole)
                {
                    movement += CalculateSphericalMovementTo(otherPlanet);
                }
                else
                {
                    movement += Vector3.Normalize(otherPlanet.Center - planet.Center);
                }
                
            }
        }
        else if (!otherPlayer.IsOccludedByBlackHole)
        {
            //if (planet.DistanceTo(otherPlanet) < Random.Range(1.0f, 4.0f) * otherPlanet.Radius)
            {
                // move away
                movement += Vector3.Normalize(planet.Center - otherPlanet.Center);
            }
        }

        return movement;
    }

    private Vector3 CalculateSphericalMovementTo(Planet otherPlanet)
    {
        var movement = Vector3.zero;

        var normalOnBlackHoleSurface = Vector3.Normalize(planet.Center - player.GetArena().BlackHoleCenter);
        var thisToOtherDir = Vector3.Normalize(otherPlanet.Center - planet.Center);

        if (Mathf.Abs(Vector3.Dot(normalOnBlackHoleSurface, thisToOtherDir)) < 0.01f)
        {
            movement += Vector3.Normalize(MathHelper.GetOrthogonal(normalOnBlackHoleSurface));
        }
        else
        {
            movement += Vector3.Normalize(Vector3.ProjectOnPlane(thisToOtherDir, normalOnBlackHoleSurface));
        }

        return movement;
    }

    private void MovePlayer(Vector3 direction)
    {
        player.Move(direction + Random.insideUnitSphere * 0.3f);
    }
}
