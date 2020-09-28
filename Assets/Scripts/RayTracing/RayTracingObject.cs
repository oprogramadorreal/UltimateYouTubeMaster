using UnityEngine;

[RequireComponent(typeof(Planet))]
public sealed class RayTracingObject : MonoBehaviour
{
    [SerializeField]
    private Color specularColor = new Color(0.8f, 0.8f, 0.8f);

    private Planet planet = null;

    private void Awake()
    {
        planet = GetComponent<Planet>();
    }

    private void OnEnable()
    {
        RayTracingMaster.RegisterObject(this);
    }

    private void OnDisable()
    {
        RayTracingMaster.UnregisterObject(this);
    }

    public RayTracingSphere GetRayTracingSphere()
    {
        var myPlanet = GetPlanet();
        var color = myPlanet.Color;

        var sphere = new RayTracingSphere();
        sphere.albedo = new Vector3(color.r, color.g, color.b);
        sphere.specular = new Vector3(specularColor.r, specularColor.g, specularColor.b);
        sphere.radius = myPlanet.Radius;
        sphere.position = myPlanet.Center;

        return sphere;
    }

    private Planet GetPlanet()
    {
        return planet != null ? planet : GetComponent<Planet>();
    }
}
