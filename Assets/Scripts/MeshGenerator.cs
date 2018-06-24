using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class MeshGenerator : MonoBehaviour
{
    [Header("Renderer")]
    public Shader shader;

    private List<MeshInfos> meshList = new List<MeshInfos>();
    private const int verticesMax = 65502;
    private Mesh[] meshArray = null;

    void Start()
    {
        CallPCL.callStart();
    }

    void Update()
    {
        CallPCL.getMesh(ref meshList, verticesMax);
        Generate();
        if (Input.GetKey(KeyCode.R))
        {
            CallPCL.callRegistration();
        }
    }

    void OnDestroy()
    {
        CallPCL.callStop();
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

        //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        //stopwatch.Start();
        //stopwatch.Stop();
        //Debug.Log(stopwatch.Elapsed.TotalMilliseconds);

        for (int i = 0; i < meshList.Count; i++)
        {
            MeshInfos meshInfo = meshList[i];
            int count = meshInfo.vertexCount;

            if (meshArray[i] == null)
            {
                meshArray[i] = new Mesh();
            }
            Mesh mesh = meshArray[i];
            if (mesh.vertexCount != meshInfo.vertices.Length)
            {
                mesh.Clear();
            }
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 16f);
            mesh.vertices = meshInfo.vertices;
            mesh.colors = meshInfo.colors;

            if (mesh.GetIndices(0) != null && mesh.GetIndices(0).Length != count)
            {
                int[] indices = new int[count];
                int[] tris = new int[count * 2];
                Parallel.For(0, count / 6, j => {
                    indices[6 * j + 0] = 6 * j + 0;
                    indices[6 * j + 1] = 6 * j + 1;
                    indices[6 * j + 2] = 6 * j + 2;
                    indices[6 * j + 3] = 6 * j + 3;
                    indices[6 * j + 4] = 6 * j + 4;
                    indices[6 * j + 5] = 6 * j + 5;
                    tris[12 * j + 0] = 6 * j + 0;
                    tris[12 * j + 1] = 6 * j + 5;
                    tris[12 * j + 2] = 6 * j + 3;
                    tris[12 * j + 3] = 6 * j + 3;
                    tris[12 * j + 4] = 6 * j + 4;
                    tris[12 * j + 5] = 6 * j + 1;
                    tris[12 * j + 6] = 6 * j + 5;
                    tris[12 * j + 7] = 6 * j + 2;
                    tris[12 * j + 8] = 6 * j + 4;
                    tris[12 * j + 9] = 6 * j + 3;
                    tris[12 * j + 10] = 6 * j + 5;
                    tris[12 * j + 11] = 6 * j + 4;
                });
                mesh.SetIndices(indices, MeshTopology.Points, 0);
                mesh.SetTriangles(tris, 0);
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
