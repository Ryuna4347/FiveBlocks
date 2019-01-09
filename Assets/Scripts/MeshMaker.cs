using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MeshMaker : MonoBehaviour
{
    public List<Vector3> vert = new List<Vector3>();
    public List<int> triangles = new List<int>();
    private Mesh mesh;

    private void Start()
    {
        MakePlaneMeshes();
    }

    private void MakePlaneMeshes()
    {
        float edgeLen = 0.5125f;

        Mesh plane = new Mesh();
        plane.name = "Round";

        Vector3[] vertices = new Vector3[4];

        vertices[0] = new Vector3(edgeLen / 2, edgeLen / 2, 0.5f);
        vertices[1] = new Vector3(edgeLen / 2, -edgeLen / 2, 0.5f);
        vertices[2] = new Vector3(-edgeLen / 2, -edgeLen / 2, 0.5f);
        vertices[3] = new Vector3(-edgeLen / 2, edgeLen / 2, 0.5f);

        int[] triangles = new int[6];

        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 3;
        triangles[3] = 0;
        triangles[4] = 1;
        triangles[5] = 2;

        plane.vertices = vertices;
        plane.triangles = triangles;
        plane.uv = new Vector2[4];

        plane.RecalculateNormals();

        var savePath = "Assets/" + "Enemy_Rect.asset";
        AssetDatabase.CreateAsset(plane, savePath);

        for (int i = 0; i < 1; i++)
        {
            GameObject meshObj = GameObject.Find("Enemy_Rect_" + i);

            meshObj.GetComponent<MeshFilter>().mesh = plane;
        }
    }

    //원 만들기 용
    //private void MakePlaneMeshes()
    //{
    //    float edgeLen = 0.5125f;

    //    Mesh plane = new Mesh();
    //    plane.name = "Round";

    //    Vector3[] vertices = new Vector3[101];

    //    vertices[0] = new Vector3(0, 0, 0.5f);

    //    for (int i = 1; i < 101; i++)
    //    {
    //        vertices[i] = new Vector3(edgeLen / 2 * Mathf.Cos(Mathf.PI * (3.6f*(i-1))/180), edgeLen / 2 * Mathf.Sin(Mathf.PI * (3.6f * (i - 1))/180), 0.5f);

    //    }

    //    int[] triangles = new int[100*3];

    //    for(int i=0; i < 100; i++)
    //    {
    //        if (i ==99)
    //        {
    //            triangles[i * 3] = 1;
    //        }
    //        else
    //        {
    //            triangles[i * 3] = i + 1;
    //        }
    //        triangles[i * 3+1] = i;
    //        triangles[i * 3+2] = 0;
    //    }

    //    plane.vertices = vertices;
    //    plane.triangles = triangles;
    //    plane.uv = new Vector2[101];

    //    plane.RecalculateNormals();

    //    var savePath = "Assets/" + "Enemy_Round.asset";
    //    AssetDatabase.CreateAsset(plane, savePath);

    //    for (int i = 0; i < 1; i++)
    //    {
    //        GameObject meshObj = GameObject.Find("Enemy_Round_" + i);

    //        meshObj.GetComponent<MeshFilter>().mesh = plane;
    //    }
    //}
}
