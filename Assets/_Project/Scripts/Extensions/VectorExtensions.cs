using UnityEngine;

public static class VectorExtensions
{
    public static Vector3 With(this Vector3 v, float ?x = null, float ?y = null, float ?z = null)
    {
        v.x = x ?? v.x;
        v.y = y ?? v.y;
        v.z = z ?? v.z;

        return v;
    }
}
