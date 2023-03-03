using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DrawOverMaterial
{
    #region METHODS
    public static void DrawOverMaterialByCollisionPoint(Vector3 collisionPoint, Texture2D toPaint, GameObject painted, Vector2 texturePoint, float radius, Texture2D normal = null)
    {
        MeshRenderer mr = painted.GetComponent<MeshRenderer>();
        SkinnedMeshRenderer smr = painted.GetComponent<SkinnedMeshRenderer>();

        Material MatToPaint = null;
        if (mr) MatToPaint = mr.material;
        else if (smr) MatToPaint = smr.material;

        if (!MatToPaint) return;

        Texture2D paintable = MatToPaint.mainTexture as Texture2D;
        Texture2D originalNormal = MatToPaint.GetTexture("_BumpMap") as Texture2D;
        if(!originalNormal) { originalNormal = new Texture2D(1024, 1024); MatToPaint.SetTexture("_BumpMap", originalNormal); }
        if(!paintable) { paintable = new Texture2D(1024, 1024); MatToPaint.mainTexture = paintable; }
        Mesh hittedMesh = null;
        
        MeshFilter mf = painted.GetComponent<MeshFilter>();
        if (!mf) return;

        hittedMesh = mf.sharedMesh;

        if (!hittedMesh) return;

        Vector3 localHitPoint = painted.transform.InverseTransformPoint(collisionPoint);

        int closestVertex = GetClosestVertex(localHitPoint, hittedMesh);

        Vector2 distance2D = new Vector2(localHitPoint.x, localHitPoint.y) - new Vector2(hittedMesh.vertices[closestVertex].x, hittedMesh.vertices[closestVertex].y);
        

        List<Vector2> uvs = new List<Vector2>();
        hittedMesh.GetUVs(0, uvs);

        Vector2 closestVertexUV = texturePoint;

        Vector2 closestVertexPositionInTexture = new Vector2(closestVertexUV.x * paintable.width, closestVertexUV.y * paintable.height);

        //toPaint.Resize((int)size.x, (int)size.y);
        //if (normal) { normal.Resize((int)size.x, (int)size.y); }

        float paintX = toPaint.width;
        float paintY = toPaint.height;

        for(int i = 0; i < paintX; i++)
        {
            int currentX = (int)(closestVertexPositionInTexture.x - (paintX / 2) + i);
            for(int j = 0; j < paintY; j++)
            {
                int currentY = (int)(closestVertexPositionInTexture.y - (paintY / 2) + j);
                Color media = (paintable.GetPixel(currentX, currentY) + toPaint.GetPixel(i, j)) / 2;
                //if(toPaint.GetPixel(currentX, currentY).a > 0)
                //if (DistanceBetweenPixels(closestVertexPositionInTexture, new Vector2(currentY, currentY)) < radius * toPaint.width)
                paintable.SetPixel(currentX, currentY, media);
                if (normal)
                {
                    Color media_normal = (originalNormal.GetPixel(currentX, currentY) + normal.GetPixel(i, j)) / 2;
                    originalNormal.SetPixel(currentX, currentY, media_normal);
                }
            }
        }
        paintable.Apply();
    }

    private static int GetClosestVertex(Vector3 localPoint, Mesh mesh)
    {
        if (!mesh) return 0;

        Vector3[] vertices = mesh.vertices;

        float dist = 0;
        int closestVertex = 0;

        for(int i = 0; i < vertices.Length; i++)
        {
            float _dist = Vector3.Distance(vertices[i], localPoint);
            if(_dist < dist || dist == 0) { dist = _dist; closestVertex = i; }
        }

        return closestVertex;
    }

    static float DistanceBetweenPixels(Vector2 pixel1, Vector2 pixel2)
    {
        float xdist = pixel1.x - pixel2.x;
        float ydist = pixel1.y - pixel2.y;

        return Mathf.Sqrt(xdist * xdist + ydist * ydist);
    }
    #endregion
}
