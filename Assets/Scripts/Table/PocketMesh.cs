using UnityEngine;

public static class PocketMesh
{
    public static Mesh Sector(float radius, float start, float end, int segments, bool facingUp)
    {
        Wrap(ref start, ref end);
        var verts = new Vector3[segments + 2];
        var uvs = new Vector2[segments + 2];
        var tris = new int[segments * 3];
        verts[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);
        for (var i = 0; i <= segments; i++)
        {
            var a = Mathf.Lerp(start, end, i / (float)segments);
            var c = Mathf.Cos(a);
            var s = Mathf.Sin(a);
            verts[i + 1] = new Vector3(c * radius, 0f, s * radius);
            uvs[i + 1] = new Vector2(c * 0.5f + 0.5f, s * 0.5f + 0.5f);
            if (i == segments)
            {
                continue;
            }

            if (facingUp)
            {
                tris[i * 3] = 0;
                tris[i * 3 + 1] = i + 2;
                tris[i * 3 + 2] = i + 1;
            }
            else
            {
                tris[i * 3] = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = i + 2;
            }
        }

        return Finish(verts, uvs, tris);
    }

    public static Mesh ArcRing(float inner, float outer, float start, float end, int segments)
    {
        Wrap(ref start, ref end);
        var verts = new Vector3[(segments + 1) * 2];
        var uvs = new Vector2[verts.Length];
        var tris = new int[segments * 6];
        for (var i = 0; i <= segments; i++)
        {
            var a = Mathf.Lerp(start, end, i / (float)segments);
            var c = Mathf.Cos(a);
            var s = Mathf.Sin(a);
            verts[i] = new Vector3(c * inner, 0f, s * inner);
            verts[i + segments + 1] = new Vector3(c * outer, 0f, s * outer);
            uvs[i] = new Vector2(0f, i / (float)segments);
            uvs[i + segments + 1] = new Vector2(1f, i / (float)segments);
            if (i == segments)
            {
                continue;
            }

            var t = i * 6;
            var o = segments + 1;
            tris[t] = i;
            tris[t + 1] = i + 1;
            tris[t + 2] = i + o;
            tris[t + 3] = i + o;
            tris[t + 4] = i + 1;
            tris[t + 5] = i + 1 + o;
        }

        return Finish(verts, uvs, tris);
    }

    public static Mesh ArcTube(float radius, float height, float start, float end, int segments)
    {
        Wrap(ref start, ref end);
        var verts = new Vector3[(segments + 1) * 2];
        var uvs = new Vector2[verts.Length];
        var tris = new int[segments * 6];
        for (var i = 0; i <= segments; i++)
        {
            var a = Mathf.Lerp(start, end, i / (float)segments);
            var x = Mathf.Cos(a) * radius;
            var z = Mathf.Sin(a) * radius;
            verts[i] = new Vector3(x, 0f, z);
            verts[i + segments + 1] = new Vector3(x, -height, z);
            uvs[i] = new Vector2(i / (float)segments, 1f);
            uvs[i + segments + 1] = new Vector2(i / (float)segments, 0f);
            if (i == segments)
            {
                continue;
            }

            var t = i * 6;
            var o = segments + 1;
            tris[t] = i;
            tris[t + 1] = i + o;
            tris[t + 2] = i + 1;
            tris[t + 3] = i + 1;
            tris[t + 4] = i + o;
            tris[t + 5] = i + 1 + o;
        }

        return Finish(verts, uvs, tris);
    }

    static void Wrap(ref float start, ref float end)
    {
        if (end < start)
        {
            end += Mathf.PI * 2f;
        }
    }

    static Mesh Finish(Vector3[] verts, Vector2[] uvs, int[] tris)
    {
        var mesh = new Mesh { name = "PocketMesh" };
        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
