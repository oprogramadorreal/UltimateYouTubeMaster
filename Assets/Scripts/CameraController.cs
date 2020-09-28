using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

public sealed class CameraController : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> targets = new List<GameObject>();

    [SerializeField]
    private PlayableDirector timelineToPlay;

    private int currentTargetIndex = 0;
    private int currentCameraType = 0;

    private readonly Dictionary<KeyCode, int> targetsMap = new Dictionary<KeyCode, int>()
    {
        { KeyCode.Alpha1, 0 },
        { KeyCode.Alpha2, 1 },
        { KeyCode.Alpha3, 2 },
        { KeyCode.Alpha4, 3 },
        { KeyCode.Alpha5, 4 },
        { KeyCode.Alpha6, 5 },
        { KeyCode.Alpha7, 6 },
        { KeyCode.Alpha8, 7 },
        { KeyCode.Alpha9, 8 },
        { KeyCode.Alpha0, 9 },
        { KeyCode.Q, 10 },
        { KeyCode.W, 11 },
        { KeyCode.E, 12 },
        { KeyCode.R, 13 },
        { KeyCode.T, 14 },
        { KeyCode.Y, 15 },
    };

    private void Update()
    {
        foreach (var item in targetsMap)
        {
            if (Input.GetKeyDown(item.Key))
            {
                LookAtTarget(item.Value);
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            LookAtTarget((currentTargetIndex + 1) % targets.Count);
        }

        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            SetCameraType((currentCameraType + 1) % 2);
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0.0f)
        {
            ScaleTargetCameraOrbit(currentTargetIndex, currentCameraType, 0.95f);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0.0f)
        {
            ScaleTargetCameraOrbit(currentTargetIndex, currentCameraType, 1.05f);
        }

        if (Input.GetKeyDown(KeyCode.P) && timelineToPlay != null)
        {
            timelineToPlay.Play();
        }

        if (Input.GetMouseButtonDown(0))
        {
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            var newTarget = PickPlayerTarget(mouseRay);

            if (newTarget >= 0 && newTarget < targets.Count)
            {
                LookAtTarget(newTarget);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Cursor.visible = false;
        }

        if (Input.GetMouseButtonUp(1))
        {
            Cursor.visible = true;
        }
    }

    private int PickPlayerTarget(Ray ray)
    {
        var player = PickPlayer(ray);

        if (player != null)
        {
            for (var i = 0; i < targets.Count; ++i)
            {
                if (ReferenceEquals(targets[i], player.gameObject))
                {
                    return i;
                }
            }
        }

        return int.MaxValue;
    }

    private Player PickPlayer(Ray ray)
    {
        Player result = null;

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 10000.0f, GetPlayersLayerMask(), QueryTriggerInteraction.Collide))
        {
            result = hitInfo.collider.GetComponentInParent<Player>();
        }

        return result;
    }

    private static int GetPlayersLayerMask()
    {
        const string layerName = "Players";
        var layerNumber = LayerMask.NameToLayer(layerName);
        return 1 << layerNumber;
    }

    private void ScaleTargetCameraOrbit(int targetIndex, int cameraType, float factor)
    {
        var target = targets[targetIndex];
        var targetCamera = GetCinemachineCamera(target, cameraType);

        if (targetCamera != null)
        {
            if (targetCamera is CinemachineVirtualCamera)
            {
                var virtualCamera = (CinemachineVirtualCamera)targetCamera;
                var transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
                var offset = transposer.m_FollowOffset;
                transposer.m_FollowOffset = new Vector3(offset.x, offset.y, offset.z * factor);
            }
            else
            {
                var freeLookCamera = (CinemachineFreeLook)targetCamera;

                for (var i = 0; i < 3; ++i)
                {
                    var o = freeLookCamera.m_Orbits[i];
                    o.m_Radius *= factor;
                    o.m_Height *= factor;
                    freeLookCamera.m_Orbits[i] = o;
                }
            }
        }
    }

    private void SetCameraType(int newCameraType)
    {
        SetTargetCameraPriority(currentTargetIndex, 10, currentCameraType);
        SetTargetCameraPriority(currentTargetIndex, 11, newCameraType);

        currentCameraType = newCameraType;
    }

    private void LookAtTarget(int newTargetIndex)
    {
        SetTargetCameraPriority(currentTargetIndex, 10, currentCameraType);
        SetTargetCameraPriority(newTargetIndex, 11, currentCameraType);

        currentTargetIndex = newTargetIndex;
    }

    private void SetTargetCameraPriority(int targetIndex, int cameraPriority, int cameraType)
    {
        var target = targets[targetIndex];
        var targetCamera = GetCinemachineCamera(target, cameraType);

        if (targetCamera != null)
        {
            targetCamera.Priority = cameraPriority;
        }
    }

    private static CinemachineVirtualCameraBase GetCinemachineCamera(GameObject obj, int type)
    {
        CinemachineVirtualCameraBase result;

        if (type == 1)
        {
            result = obj.GetComponentInChildren<CinemachineFreeLook>();

            if (result != null)
            {
                return result;
            }
        }

        result = GetVirtualCamera(obj);

        if (result == null)
        {
            result = obj.GetComponentInChildren<CinemachineFreeLook>();
        }

        return result;
    }

    private static CinemachineVirtualCamera GetVirtualCamera(GameObject obj)
    {
        var cams = obj.GetComponentsInChildren<CinemachineVirtualCamera>();

        if (cams.Count() == 0)
        {
            return null;
        }

        return cams.Count() > 1 ? cams.FirstOrDefault(o => o.name == "CM vcam1") : cams[0];
    }
}
