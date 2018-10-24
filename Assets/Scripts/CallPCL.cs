using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

public class CallPCL : MonoBehaviour
{
    const int POINT_BYTES = 20;
    private static unsafe byte* ptr;

    [DllImport("3D-Telepresence", EntryPoint = "callStart")]
    public static extern void callStart();

    [DllImport("3D-Telepresence", EntryPoint = "callUpdate")]
    public static extern IntPtr callUpdate();

    [DllImport("3D-Telepresence", EntryPoint = "callStop")]
    public static extern void callStop();

    public static void kernelUpdate()
    {
        unsafe
        {
            ptr = (byte*)callUpdate();
        }
    }

    public static void getMesh(ref List<MeshInfos> meshList, int vMax)
    {
        unsafe
        {
            if (ptr == null)
            {
                return;
            }
            int size = *((int*)ptr) * 3;
            ptr = ptr + 4;

            int meshId = 0;
            for (int st = 0; st < size; st += vMax / 2, meshId++)
            {
                if (meshId >= meshList.Count)
                {
                    MeshInfos newMesh = new MeshInfos();
                    newMesh.vertexCount = vMax;
                    newMesh.vertices = new Vector3[vMax];
                    newMesh.colors = new Color[vMax];
                    meshList.Add(newMesh);
                }
                MeshInfos mesh = meshList[meshId];

                int len = Math.Max(0, Math.Min(size - st, vMax / 2));
                Parallel.For(0, len / 3, i => {
                    byte* p0 = ptr + (st + i * 3) * POINT_BYTES;
                    byte* p1 = p0 + POINT_BYTES;
                    byte* p2 = p1 + POINT_BYTES;
                    int i6 = i * 6;
                    mesh.vertices[i6 + 0] = *((Vector3*)p0);
                    mesh.vertices[i6 + 1] = *((Vector3*)p1);
                    mesh.vertices[i6 + 2] = *((Vector3*)p2);
                    mesh.vertices[i6 + 3] = (mesh.vertices[i6 + 0] + mesh.vertices[i6 + 1]) * 0.5f;
                    mesh.vertices[i6 + 4] = (mesh.vertices[i6 + 1] + mesh.vertices[i6 + 2]) * 0.5f;
                    mesh.vertices[i6 + 5] = (mesh.vertices[i6 + 2] + mesh.vertices[i6 + 0]) * 0.5f;
                    mesh.colors[i6 + 0].r = (float)(*(p0 + 12)) / 255;
                    mesh.colors[i6 + 0].g = (float)(*(p0 + 13)) / 255;
                    mesh.colors[i6 + 0].b = (float)(*(p0 + 14)) / 255;
                    mesh.colors[i6 + 3].r = (float)(*(p0 + 16)) / 255;
                    mesh.colors[i6 + 3].g = (float)(*(p0 + 17)) / 255;
                    mesh.colors[i6 + 3].b = (float)(*(p0 + 18)) / 255;
                    mesh.colors[i6 + 1].r = (float)(*(p1 + 12)) / 255;
                    mesh.colors[i6 + 1].g = (float)(*(p1 + 13)) / 255;
                    mesh.colors[i6 + 1].b = (float)(*(p1 + 14)) / 255;
                    mesh.colors[i6 + 4].r = (float)(*(p1 + 16)) / 255;
                    mesh.colors[i6 + 4].g = (float)(*(p1 + 17)) / 255;
                    mesh.colors[i6 + 4].b = (float)(*(p1 + 18)) / 255;
                    mesh.colors[i6 + 2].r = (float)(*(p2 + 12)) / 255;
                    mesh.colors[i6 + 2].g = (float)(*(p2 + 13)) / 255;
                    mesh.colors[i6 + 2].b = (float)(*(p2 + 14)) / 255;
                    mesh.colors[i6 + 5].r = (float)(*(p2 + 16)) / 255;
                    mesh.colors[i6 + 5].g = (float)(*(p2 + 17)) / 255;
                    mesh.colors[i6 + 5].b = (float)(*(p2 + 18)) / 255;
                });
                Parallel.For(len * 2, vMax, i => {
                    mesh.vertices[i].Set(0, 0, 0); //mesh.colors[i] = new Color();
                });
                mesh.exist = true;
            }
            for (int id = meshId; id < meshList.Count; id++)
            {
                MeshInfos mesh = meshList[id];
                if (mesh.exist)
                {
                    Parallel.For(0, vMax, i => {
                        mesh.vertices[i].Set(0, 0, 0); //mesh.colors[i] = new Color();
                    });
                    mesh.exist = false;
                }
            }
        }
    }
}
