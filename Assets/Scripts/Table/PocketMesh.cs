using UnityEngine;

public static class PocketMesh
{
    public static Mesh Disc(float radius, int segments, bool facingUp)
    {
        var verts = new Vector3[segments + 1];
        var uvs = new Vector2[segments + 1];
        var tris = new int[segments * 3];
        verts[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);
        for (var i = 0; i < segments; i++)
        {
            var a = i * Mathf.PI * 2f / segments;
            verts[i + 1] = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
            uvs[i + 1] = new Vector2(Mathf.Cos(a) * 0.5f + 0.5f, Mathf.Sin(a) * 0.5f + 0.5f);
            var ni = i + 1 == segments ? 1 : i + 2;
            if (facingUp)
            {
                tris[i * 3] = 0;
                tris[i * 3 + 1] = ni;
                tris[i * 3 + 2] = i + 1;
            }
            else
            {
                tris[i * 3] = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = ni;
            }
        }

        return Finish(verts, uvs, tris);
    }

    public static Mesh Ring(float inner, float outer, int segments)
    {
        var verts = new Vector3[segments * 2];
        var uvs = new Vector2[segments * 2];
        var tris = new int[segments * 6];
        for (var i = 0; i < segments; i++)
        {
            var a = i * Mathf.PI * 2f / segments;
            var c = Mathf.Cos(a);
            var s = Mathf.Sin(a);
            verts[i] = new Vector3(c * inner, 0f, s * inner);
            verts[i + segments] = new Vector3(c * outer, 0f, s * outer);
            uvs[i] = new Vector2(0f, i / (float)segments);
            uvs[i + segments] = new Vector2(1f, i / (float)segments);
            var ni = (i + 1) % segments;
            var t = i * 6;
            tris[t] = i;
            tris[t + 1] = ni;
            tris[t + 2] = i + segments;
            tris[t + 3] = i + segments;
            tris[t + 4] = ni;
            tris[t + 5] = ni + segments;
        }

        return Finish(verts, uvs, tris);
    }

    public static Mesh Tube(float radius, float height, int segments)
    {
        var verts = new Vector3[segments * 2];
        var uvs = new Vector2[segments * 2];
        var tris = new int[segments * 6];
        for (var i = 0; i < segments; i++)
        {
            var a = i * Mathf.PI * 2f / segments;
            var x = Mathf.Cos(a) * radius;
            var z = Mathf.Sin(a) * radius;
            verts[i] = new Vector3(x, 0f, z);
            verts[i + segments] = new Vector3(x, -height, z);
            uvs[i] = new Vector2(i / (float)segments, 1f);
            uvs[i + segments] = new Vector2(i / (float)segments, 0f);
            var ni = (i + 1) % segments;
            var t = i * 6;
            tris[t] = i;
            tris[t + 1] = i + segments;
            tris[t + 2] = ni;
            tris[t + 3] = ni;
            tris[t + 4] = i + segments;
            tris[t + 5] = ni + segments;
        }

        return Finish(verts, uvs, tris);
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
