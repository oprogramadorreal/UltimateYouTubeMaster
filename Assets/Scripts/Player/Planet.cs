using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class Planet : MonoBehaviour, ITimeBody
{
    [SerializeField]
    private Transform translation;

    [SerializeField]
    private Transform scale;

    [SerializeField]
    private float scaleSpeed = 7.0f;

    [SerializeField]
    private float targetRadius = 0.0f;

    [SerializeField]
    private List<string> possibleClosePlanetsLayers = new List<string>();

    [SerializeField]
    private Color color = Color.red;

    private GameConfig config;
    private Rigidbody planetRigidbody;

    private Arena arena;

    public Vector3 Center { get => translation.position; }

    public float Radius { get => scale.localScale.x / 2.0f; }

    public float TargetRadius
    {
        get => targetRadius;
        set => targetRadius = ClampRadius(value);
    }

    private int blackHoleLayerMask = 0;

    public bool IsDead { get; private set; } = false;

    private void Awake()
    {
        {
            const string blackHoleLayerName = "Black Hole";
            var blackHoleLayer = LayerMask.NameToLayer(blackHoleLayerName);
            blackHoleLayerMask = 1 << blackHoleLayer;
            IsBlackHole = blackHoleLayer == gameObject.layer;
        }

        config = FindObjectOfType<GameConfig>();
        planetRigidbody = GetComponentInChildren<Rigidbody>();

        arena = FindArena();

        if (TargetRadius == 0.0f)
        {
            SetRadius(scale.localScale.x / 2.0f);
        }
        else
        {
            SetRadius(TargetRadius);
        }
    }

    public void Die()
    {
        for (var i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        SetRenderingEnabled(false);

        IsDead = true;
    }

    public void EnsureCanAbsorbMySubs()
    {        
        if (!possibleClosePlanetsLayers.Contains("My Subs"))
        {
            possibleClosePlanetsLayers.Add("My Subs");
        }
    }

    private void Revive()
    {
        for (var i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }

        SetRenderingEnabled(true);

        IsDead = false;
    }

    private void SetRenderingEnabled(bool enabled)
    {
        var rayTracing = GetComponent<RayTracingObject>();

        if (rayTracing != null)
        {
            rayTracing.enabled = enabled;
        }
    }

    public Arena FindArena()
    {
        var arena = transform.parent.GetComponentInChildren<Arena>();

        if (arena == null)
        {
            arena = GameObject.Find("Arena")?.GetComponent<Arena>();
        }

        return arena;
    }

    public Color Color { get => color; set => color = value; }

    private void UpdateRigidbody()
    {
        planetRigidbody.mass = scale.localScale.x / 10.0f;
    }

    private void FixedUpdate()
    {
        if (!CanUpdate())
        {
            return;
        }

        var planetRadiusScaleStep = Time.fixedDeltaTime * scaleSpeed;

        if (Radius < TargetRadius)
        {
            var nextRadius = Radius + planetRadiusScaleStep;

            if (nextRadius > TargetRadius)
            {
                nextRadius = TargetRadius;
            }

            UpdateRadius(nextRadius);
        }
    }

    public bool CanUpdate()
    {
        return Physics.autoSimulation
            && !IsDead;
    }

    public void SetCenter(Vector3 newCenter)
    {
        translation.position = newCenter;
    }

    public void SetRadius(float newRadius)
    {
        UpdateRadius(newRadius);
        TargetRadius = newRadius;
    }

    private void UpdateRadius(float newRadius)
    {
        newRadius = ClampRadius(newRadius);
        scale.transform.localScale = new Vector3(newRadius * 2.0f, newRadius * 2.0f, newRadius * 2.0f);

        UpdateRigidbody();
    }

    private float ClampRadius(float value)
    {
        // Black hole has unlimited radius size
        return IsBlackHole ? value : Mathf.Clamp(value, value, config.MaxPlanetRadius);
    }

    public float DistanceTo(Planet other)
    {
        return Vector3.Distance(Center, other.Center) - (Radius + other.Radius);
    }

    public float GetDistanceToTheBlackHole()
    {
        return arena.GetDistanceToBlackHole(this);
    }

    public Planet GetClosestPlanet()
    {
        return GetCollidersOfPossibleClosePlanets()
                .Select(c => c.GetComponentInParent<Planet>())
                .Where(p => CanBeAClosePlanet(p))
                .OrderBy(o => (o.Center - Center).sqrMagnitude)
                .FirstOrDefault();
    }

    private IEnumerable<Collider> GetCollidersOfPossibleClosePlanets()
    {
        var finalLayerMask = 0;

        foreach (var layerName in possibleClosePlanetsLayers)
        {
            finalLayerMask |= 1 << LayerMask.NameToLayer(layerName);
        }

        return Physics.OverlapSphere(Center, arena.Radius * 2.0f, finalLayerMask);
    }

    private bool CanBeAClosePlanet(Planet p)
    {
        return p.isActiveAndEnabled
            && !p.IsDead
            && !ReferenceEquals(this, p);
    }

    public bool IsOccludedByBlackHole(Planet other)
    {
        var thisToOtherRay = new Ray(Center, Vector3.Normalize(other.Center - Center));
        var distanceToOther = DistanceTo(other) + Radius;

        return Physics.Raycast(thisToOtherRay, distanceToOther, blackHoleLayerMask, QueryTriggerInteraction.Collide);
    }

    public bool IsBlackHole { get; private set; }

    void ITimeBody.RewindStarted()
    {
        if (!IsBlackHole)
        {
            planetRigidbody.isKinematic = true;
        }
    }

    void ITimeBody.RewindStopped()
    {
        if (!IsBlackHole)
        {
            planetRigidbody.isKinematic = false;
        }
    }

    IMemento ITimeBody.CreateMemento()
    {
        return new Memento
        {
            Center = Center,
            Velocity = planetRigidbody.velocity,
            Radius = Radius,
            TargetRadius = targetRadius,
            ScaleSpeed = scaleSpeed,
            Color = color,
            IsDead = IsDead,
            PossibleClosePlanetsLayers = CloneList(possibleClosePlanetsLayers)
        };
    }

    void ITimeBody.RestoreMemento(IMemento o)
    {
        var memento = (Memento)o;
        SetCenter(memento.Center);
        planetRigidbody.velocity = memento.Velocity;
        scale.localScale = new Vector3(memento.Radius * 2.0f, memento.Radius * 2.0f, memento.Radius * 2.0f);
        targetRadius = memento.TargetRadius;
        scaleSpeed = memento.ScaleSpeed;
        color = memento.Color;
        possibleClosePlanetsLayers = CloneList(memento.PossibleClosePlanetsLayers);

        if (IsDead != memento.IsDead)
        {
            if (IsDead)
            {
                Revive();
            }
            else
            {
                Die();
            }
        }
    }

    private static List<T> CloneList<T>(List<T> list) where T: ICloneable
    {
        return list.Select(i => (T)i.Clone()).ToList();
    }

    private sealed class Memento : IMemento
    {
        public Vector3 Center { get; set; }
        public Vector3 Velocity { get; set; }
        public float Radius { get; set; }
        public float TargetRadius { get; set; }
        public float ScaleSpeed { get; set; }
        public Color Color { get; set; }
        public bool IsDead { get; set; }
        public List<string> PossibleClosePlanetsLayers { get; set; }
    }
}
