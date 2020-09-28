using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Based on
/// - http://blog.three-eyed-games.com/2018/05/03/gpu-ray-tracing-in-unity-part-1/
/// - https://youtu.be/Cp5WWtMoeKg
/// </summary>
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public sealed class RayTracingMaster : MonoBehaviour
{
    [SerializeField]
    private ComputeShader rayTracingShader;

    [SerializeField]
    private Texture skyboxTexture;

    [SerializeField]
    private RenderTexture uiTexture;

    [SerializeField]
    private Light lightSource;

    [SerializeField]
    [Range(1, 10)]
    private int lightBounceLimit = 8;

    [SerializeField]
    [Range(0.0f, 40.0f)]
    private float blendFactor = 15.0f;

    private RenderTexture target;

    private Material addMaterial;

    private ComputeBuffer sphereBuffer;

    private static List<RayTracingObject> rayTracingObjects = new List<RayTracingObject>();

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        var spheres = CreateSpheres();

        if (spheres.Count > 0)
        {
            SetUpSphereBuffer(spheres);
            SetShaderParameters();
            Render(source, destination);
        }
        else
        {
            // Nothing to draw with ray tracing. Do regular stuff.
            Graphics.Blit(source, destination);
        }
    }

    private void OnDisable()
    {
        if (sphereBuffer != null)
        {
            sphereBuffer.Release();
        }
    }

    private void SetShaderParameters()
    {
        var currentCamera = Camera.current;
        rayTracingShader.SetMatrix("_CameraToWorld", currentCamera.cameraToWorldMatrix);
        rayTracingShader.SetMatrix("_CameraInverseProjection", currentCamera.projectionMatrix.inverse);

        rayTracingShader.SetTexture(0, "_SkyboxTexture", skyboxTexture);

        switch (lightSource.type)
        {
            case LightType.Directional:
            {
                var l = lightSource.transform.forward;
                rayTracingShader.SetVector("_Light", new Vector4(l.x, l.y, l.z, lightSource.intensity));
                rayTracingShader.SetFloat("_IsPointLight", 0.0f);
                break;
            }

            case LightType.Point:
            {
                var l = lightSource.transform.position;
                rayTracingShader.SetVector("_Light", new Vector4(l.x, l.y, l.z, lightSource.intensity));
                rayTracingShader.SetFloat("_IsPointLight", 1.0f);
                break;
            }

            default:
            {
                throw new System.Exception("Light type not supported.");
            }
        }

        rayTracingShader.SetBuffer(0, "_Spheres", sphereBuffer);

        rayTracingShader.SetInt("_LightBounceLimit", lightBounceLimit);

        rayTracingShader.SetFloat("_BlendFactor", blendFactor);
    }

    private void Render(RenderTexture source, RenderTexture destination)
    {
        // Make sure we have a current render target
        InitRenderTexture();

        // Set the source and target textures
        rayTracingShader.SetTexture(0, "Source", source);
        rayTracingShader.SetTexture(0, "Result", target);

        // Dispatch the compute shader
        var threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        var threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        rayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        // Blit the result texture to the screen
        if (Application.isPlaying)
        {
            if (addMaterial == null)
            {
                addMaterial = new Material(Shader.Find("Hidden/AddShader"));
            }

            addMaterial.SetTexture("_UiTex", uiTexture);

            Graphics.Blit(target, destination, addMaterial);
        }
        else
        {
            Graphics.Blit(target, destination);
        }
    }

    private void InitRenderTexture()
    {
        if (target == null || target.width != Screen.width || target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (target != null)
            {
                target.Release();
            }

            // Get a render target for Ray Tracing
            target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();
        }
    }

    private void SetUpSphereBuffer(List<RayTracingSphere> spheres)
    {
        if (sphereBuffer != null)
        {
            sphereBuffer.Release();
        }

        // Assign to compute buffer
        const int rayTracingSphereSizeInBytes = 40; // This is the size of the struct RayTracingSphere
        sphereBuffer = new ComputeBuffer(spheres.Count, rayTracingSphereSizeInBytes);
        sphereBuffer.SetData(spheres);
    }

    private static List<RayTracingSphere> CreateSpheres()
    {
        return GetAllRayTracingObjects()
            .Where(p => p.isActiveAndEnabled)
            .Select(p => p.GetRayTracingSphere())
            .ToList();
    }

    private static IEnumerable<RayTracingObject> GetAllRayTracingObjects()
    {
        return Application.isPlaying ? rayTracingObjects : FindObjectsOfType<RayTracingObject>().ToList();
    }

    public static void RegisterObject(RayTracingObject obj)
    {
        rayTracingObjects.Add(obj);
    }

    public static void UnregisterObject(RayTracingObject obj)
    {
        rayTracingObjects.Remove(obj);
    }
}
