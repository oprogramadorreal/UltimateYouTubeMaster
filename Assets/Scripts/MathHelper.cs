using UnityEngine;

public static class MathHelper
{
    public static Vector3 GetOrthogonal(Vector3 r)
    {
		if (Mathf.Abs(r.x) <= Mathf.Abs(r.y) && Mathf.Abs(r.x) <= Mathf.Abs(r.z))
		{
			return new Vector3(0.0f, -r.z, r.y);
		}

		if (Mathf.Abs(r.y) <= Mathf.Abs(r.x) && Mathf.Abs(r.y) <= Mathf.Abs(r.z))
		{
			return new Vector3(-r.z, 0.0f, r.x);
		}

		return new Vector3(-r.y, r.x, 0.0f);
	}
}
