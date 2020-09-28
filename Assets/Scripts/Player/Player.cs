using Cinemachine;
using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Planet))]
public sealed class Player : MonoBehaviour, ITimeBody
{
    [SerializeField]
    private float moveSpeed = 0.1f;

    [SerializeField]
    private GameObject swallowedSoundPrefab;

    [SerializeField]
    private GameObject swallowedExplosionPrefab;

    [SerializeField]
    private float explosionScale = 1.0f;

    private Planet planet;
    private Rigidbody planetRigidbody;
    private Arena arena;

    private CinemachineVirtualCamera fixedCamera;
    private CinemachineFreeLook freeLookCamera;

    private float score = 0.0f;

    public event EventHandler SwallowedOther;
    public event EventHandler WasSwallowed;

    private void Awake()
    {
        planet = GetComponent<Planet>();
        planetRigidbody = GetComponentInChildren<Rigidbody>();
        arena = planet.FindArena();

        fixedCamera = GetComponentsInChildren<CinemachineVirtualCamera>().FirstOrDefault(o => o.name == "CM vcam1");
        freeLookCamera = GetComponentInChildren<CinemachineFreeLook>();

        WasSwallowed += Player_WasSwallowed;
    }

    private void Player_WasSwallowed(object sender, EventArgs e)
    {
        PlaySwallowedSound();
        PlaySwallowedExplosion(sender as Player);

        planet.Die();
    }

    private void PlaySwallowedSound()
    {
        if (swallowedSoundPrefab != null)
        {
            var sound = Instantiate(swallowedSoundPrefab, planet.Center, Quaternion.identity);
            sound.GetComponent<AudioSource>().Play();
            Destroy(sound, 2.0f);
        }
    }

    private void PlaySwallowedExplosion(Player playerThatSwallowed)
    {
        if (swallowedExplosionPrefab != null && playerThatSwallowed != null)
        {
            var intersectionPoint = planet.Center + planet.Radius * (playerThatSwallowed.planet.Center - planet.Center).normalized;
            var scale = planet.Radius * explosionScale;

            var explosion = Instantiate(swallowedExplosionPrefab, intersectionPoint, Quaternion.identity, playerThatSwallowed.transform);

            SetToLayerRecursively(explosion.transform, LayerMask.NameToLayer("UI"));

            var particleMain = explosion.GetComponent<ParticleSystem>().main;
            particleMain.startColor = planet.Color;

            explosion.transform.localScale = new Vector3(scale, scale, scale);

            Destroy(explosion, 2.0f);
        }
    }

    private static void SetToLayerRecursively(Transform root, int layer)
    {
        root.gameObject.layer = layer;

        foreach (Transform child in root)
        {
            SetToLayerRecursively(child, layer);
        }
    }

    public Arena GetArena()
    {
        return arena;
    }

    public Vector3 GetValidRandomPositionInsideArena()
    {
        return arena.GetValidRandomPointFor(planet.Radius);
    }

    public void RespawnRandom()
    {
        Respawn(GetValidRandomPositionInsideArena(), UnityEngine.Random.Range(1.0f, 10.0f));
    }

    public void Respawn(Vector3 position, float planetRadius)
    {
        gameObject.SetActive(true);
        planet.SetRadius(planetRadius);
        planet.SetCenter(position);
        planetRigidbody.velocity = Vector3.zero;
    }

    public void Move(Vector3 direction)
    {
        var correctedSpeed = moveSpeed / (planetRigidbody.mass * 0.06f);
        planetRigidbody.AddForce(direction * correctedSpeed, ForceMode.VelocityChange);
        planetRigidbody.AddForce(-Vector3.ProjectOnPlane(planetRigidbody.velocity, direction.normalized));
    }

    public bool HasCurrentActiveCamera()
    {
        return (freeLookCamera != null && freeLookCamera.Priority == 11)
            || (fixedCamera != null && fixedCamera.Priority == 11);
    }

    private void FixedUpdate()
    {
        if (!planet.CanUpdate())
        {
            return;
        }

        var closePlayer = GetClosestPlayer();

        if (closePlayer != null)
        {
            TryToSwallow(closePlayer);
        }
    }

    public ClosePlayerInfo GetClosestPlayer()
    {
        ClosePlayerInfo result = null;

        var closePlanet = planet.GetClosestPlanet();

        if (closePlanet != null)
        {
            var closePlayer = closePlanet.GetComponentInParent<Player>();

            if (closePlayer != null)
            {
                result = new ClosePlayerInfo(closePlayer, closePlanet, closePlanet.IsOccludedByBlackHole(planet));;
            }
        }

        return result;
    }

    private void TryToSwallow(ClosePlayerInfo closePlayer)
    {
        if (CanSwallow(closePlayer) && planet.DistanceTo(closePlayer.Planet) <= (closePlayer.Planet.Radius * -0.5f))
        {
            Swallow(closePlayer);
        }
    }

    public bool CanSwallow(ClosePlayerInfo closePlayer)
    {
        return closePlayer != null
            && !closePlayer.Planet.IsBlackHole
            && CalculateScore() > closePlayer.Player.CalculateScore();
    }

    private void Swallow(ClosePlayerInfo closePlayer)
    {
        if (!planet.IsBlackHole)
        {
            planet.TargetRadius += closePlayer.Planet.Radius; // grow this
            IncorporateColor(closePlayer.Planet);
        }

        score = CalculateScore() + closePlayer.Player.CalculateScore();

        SwallowedOther?.Invoke(this, EventArgs.Empty);
        closePlayer.Player.WasSwallowed?.Invoke(this, EventArgs.Empty);
    }

    private void IncorporateColor(Planet otherPlanet)
    {
        var t = otherPlanet.Radius / (planet.Radius + otherPlanet.Radius);
        planet.Color = Color.Lerp(planet.Color, otherPlanet.Color, t);
    }

    public float CalculateScore()
    {
        return score == 0.0f ? planet.Radius : score;
    }

    void ITimeBody.RewindStarted() { }

    void ITimeBody.RewindStopped() { }

    IMemento ITimeBody.CreateMemento()
    {
        return new Memento
        {
            MoveSpeed = moveSpeed,
            Score = score
        };
    }

    void ITimeBody.RestoreMemento(IMemento o)
    {
        var memento = (Memento)o;
        moveSpeed = memento.MoveSpeed;
        score = memento.Score;
    }

    private sealed class Memento : IMemento
    {
        public float MoveSpeed { get; set; }
        public float Score { get; set; }
    }
}
