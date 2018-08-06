using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System;

public class MeshGenerator : MonoBehaviour
{
    [Header("Renderer")]
    public Shader shader;

    private List<MeshInfos> meshList = new List<MeshInfos>();
    private const int VERTICES_MAX = 65502;
    private Mesh[] meshArray = null;

    private int[] indicesMax = new int[VERTICES_MAX];
    private int[] trisMax = new int[VERTICES_MAX * 2];

    private Thread thread;

    void Start()
    {
        CallPCL.callStart();

        Parallel.For(0, VERTICES_MAX / 6, j => {
            indicesMax[6 * j + 0] = 6 * j + 0;
            indicesMax[6 * j + 1] = 6 * j + 1;
            indicesMax[6 * j + 2] = 6 * j + 2;
            indicesMax[6 * j + 3] = 6 * j + 3;
            indicesMax[6 * j + 4] = 6 * j + 4;
            indicesMax[6 * j + 5] = 6 * j + 5;
            trisMax[12 * j + 0] = 6 * j + 0;
            trisMax[12 * j + 1] = 6 * j + 5;
            trisMax[12 * j + 2] = 6 * j + 3;
            trisMax[12 * j + 3] = 6 * j + 3;
            trisMax[12 * j + 4] = 6 * j + 4;
            trisMax[12 * j + 5] = 6 * j + 1;
            trisMax[12 * j + 6] = 6 * j + 5;
            trisMax[12 * j + 7] = 6 * j + 2;
            trisMax[12 * j + 8] = 6 * j + 4;
            trisMax[12 * j + 9] = 6 * j + 3;
            trisMax[12 * j + 10] = 6 * j + 5;
            trisMax[12 * j + 11] = 6 * j + 4;
        });

        thread = new Thread(subThread);
        thread.Start();
    }

    void subThread()
    {
        while (true)
        {
            CallPCL.kernelUpdate();
            lock (this)
            {
                CallPCL.getMesh(ref meshList, VERTICES_MAX);
            }
            ShowFPS.udpateFrame();
        }
    }

    void Update()
    {
        lock (this)
        {
            Generate();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            CallPCL.callRegistration();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            CallPCL.callSaveBackground();
        }
    }

    void OnDestroy()
    {
        thread.Abort();
        while (thread.IsAlive)
        {
            Thread.Sleep(1);
        }
        CallPCL.callStop();
        Debug.Log("Stop");
    }

    public void Generate()
    {
        if (meshArray == null || meshArray.Length != meshList.Count)
        {
            if (meshArray != null)
            {
                foreach (Mesh mesh in meshArray)
                {
                    if (mesh != null)
                    {
                        Destroy(mesh);
                    }
                }
            }
            meshArray = new Mesh[meshList.Count];
        }

        for (int i = 0; i < meshList.Count; i++)
        {
            if (meshArray[i] == null)
            {
                meshArray[i] = new Mesh();
            }

            MeshInfos meshInfo = meshList[i];
            Mesh mesh = meshArray[i];

            if (mesh.vertexCount != meshInfo.vertices.Length)
            {
                mesh.Clear();
                mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 16f);
                mesh.vertices = meshInfo.vertices;
                mesh.colors = meshInfo.colors;
                mesh.SetIndices(indicesMax, MeshTopology.Points, 0);
                mesh.SetTriangles(trisMax, 0);
            }
            else
            {
                mesh.vertices = meshInfo.vertices;
                mesh.colors = meshInfo.colors;
            }
        }

        if (transform.childCount == meshArray.Length)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                child.GetComponent<MeshFilter>().mesh = meshArray[i];
            }
        }
        else
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                DestroyImmediate(child.gameObject);
            }
            for (int i = 0; i < meshArray.Length; i++)
            {
                CreateGameObjectWithMesh(meshArray[i], gameObject.name + "_" + i, transform);
            }
        }
    }

    public GameObject CreateGameObjectWithMesh(Mesh mesh, string name = "GeneratedMesh", Transform parent = null)
    {
        GameObject meshGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        DestroyImmediate(meshGameObject.GetComponent<Collider>());
        meshGameObject.GetComponent<MeshFilter>().mesh = mesh;
        Material material = new Material(shader);
        meshGameObject.GetComponent<Renderer>().sharedMaterial = material;
        meshGameObject.name = name;
        meshGameObject.transform.parent = parent;
        meshGameObject.transform.localPosition = Vector3.zero;
        meshGameObject.transform.localRotation = Quaternion.identity;
        meshGameObject.transform.localScale = Vector3.one;
        return meshGameObject;
    }
}

/*
System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
stopwatch.Start();

stopwatch.Stop();
Debug.Log(stopwatch.Elapsed.TotalMilliseconds);
*/
