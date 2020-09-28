using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public sealed class Arena : MonoBehaviour
{
    private Material material;
    private BlackHole blackHole;
    private GameConfig config;

    private void Awake()
    {
        material = GetComponent<MeshRenderer>().material;
        blackHole = transform.parent.GetComponentInChildren<BlackHole>();
        config = FindObjectOfType<GameConfig>();
    }

    private void Start()
    {
        Reset();
    }

    private void Reset()
    {
        if (blackHole != null)
        {
            blackHole.SetRadius(50.0f);
            blackHole.TargetRadius = Radius - config.MaxPlanetRadius * 2.2f;
        }
    }

    public void UpdateShader(Planet planet)
    {
        material.SetVector("_PlayerPosition", planet.Center);
        material.SetFloat("_PlayerPlanetRadius", planet.Radius);
    }

    public float Radius { get => transform.localScale.x / 2.0f; }

    public Vector3 Center { get => transform.position; }

    public Vector3 BlackHoleCenter { get => blackHole != null ? blackHole.Planet.Center : Vector3.zero; }

    public float GetDistanceToBlackHole(Planet planet)
    {
        if (blackHole == null)
        {
            return float.MaxValue;
        }

        return planet.DistanceTo(blackHole.Planet);
    }

    public Vector3 GetValidRandomPointFor(float planetRadius)
    {
        var randomDir = Random.insideUnitSphere.normalized;

        var blackHoleCenter = Vector3.zero;
        var blackHoleRadius = 0.0f;

        if (blackHole != null)
        {
            blackHoleCenter = blackHole.Center;
            blackHoleRadius = blackHole.Radius;
        }

        var minPoint = blackHoleCenter + randomDir * (blackHoleRadius + planetRadius);
        var maxPoint = blackHoleCenter + randomDir * (Radius - planetRadius);

        return minPoint + (maxPoint - minPoint) * Random.Range(0.0f, 1.0f);
    }
}
