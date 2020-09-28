using Cinemachine;
using UnityEngine;

// https://forum.unity.com/threads/how-do-i-make-a-cinemachinefreelook-orbiting-camera-that-only-orbits-when-the-mouse-key-is-down.527634/#post-3468444
public sealed class CMFreelookOnlyWhenRightMouseDown : MonoBehaviour
{
    private void Awake()
    {
        CinemachineCore.GetInputAxis = GetAxisCustom;
    }

    public static float GetAxisCustom(string axisName)
    {
        if (axisName == "Mouse X")
        {
            if (Input.GetMouseButton(1))
            {
                return Input.GetAxis("Mouse X");
            }
            else
            {
                return 0;
            }
        }
        else if (axisName == "Mouse Y")
        {
            if (Input.GetMouseButton(1))
            {
                return Input.GetAxis("Mouse Y");
            }
            else
            {
                return 0;
            }
        }

        return Input.GetAxis(axisName);
    }
}