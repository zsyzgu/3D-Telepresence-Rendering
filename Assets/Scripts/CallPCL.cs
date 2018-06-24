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
    
    [DllImport("3D-Telepresence", EntryPoint = "callStart")]
    public static extern void callStart();

    [DllImport("3D-Telepresence", EntryPoint = "callUpdate")]
    public static extern IntPtr callUpdate();

    [DllImport("3D-Telepresence", EntryPoint = "callRegistration")]
    public static extern void callRegistration();

    [DllImport("3D-Telepresence", EntryPoint = "callStop")]
    public static extern void callStop();

    public static void getMesh(ref List<MeshInfos> meshList, int vMax)
    {
        unsafe
        {
            byte* ptr = (byte*)callUpdate();
            int size = *((int*)ptr) * 3;
            ptr = ptr + 4;

            int meshId = 0;
            for (int st = 0; st < size; st += vMax / 2, meshId++)
            {
                if (meshId >= meshList.Count)
                {
                    meshList.Add(new MeshInfos());
                }
                MeshInfos mesh = meshList[meshId];

                int len = Math.Min(size - st, vMax / 2);
                if (mesh.vertexCount != len * 2)
                {
                    mesh.vertexCount = len * 2;
                    mesh.vertices = new Vector3[len * 2];
                    mesh.colors = new Color[len * 2];
                }

                Parallel.For(0, len / 3, i => {
                    int id = (st + i * 3) * POINT_BYTES;
                    byte* p0 = ptr + id;
                    byte* p1 = ptr + id + POINT_BYTES;
                    byte* p2 = ptr + id + POINT_BYTES * 2;
                    mesh.vertices[i * 6 + 0] = *((Vector3*)p0);
                    mesh.vertices[i * 6 + 1] = *((Vector3*)p1);
                    mesh.vertices[i * 6 + 2] = *((Vector3*)p2);
                    mesh.vertices[i * 6 + 3] = (mesh.vertices[i * 6 + 0] + mesh.vertices[i * 6 + 1]) * 0.5f;
                    mesh.vertices[i * 6 + 4] = (mesh.vertices[i * 6 + 1] + mesh.vertices[i * 6 + 2]) * 0.5f;
                    mesh.vertices[i * 6 + 5] = (mesh.vertices[i * 6 + 2] + mesh.vertices[i * 6 + 0]) * 0.5f;
                    mesh.colors[i * 6 + 0].r = (float)(*(p0 + 12)) / 255;
                    mesh.colors[i * 6 + 0].g = (float)(*(p0 + 13)) / 255;
                    mesh.colors[i * 6 + 0].b = (float)(*(p0 + 14)) / 255;
                    mesh.colors[i * 6 + 1].r = (float)(*(p1 + 12)) / 255;
                    mesh.colors[i * 6 + 1].g = (float)(*(p1 + 13)) / 255;
                    mesh.colors[i * 6 + 1].b = (float)(*(p1 + 14)) / 255;
                    mesh.colors[i * 6 + 2].r = (float)(*(p2 + 12)) / 255;
                    mesh.colors[i * 6 + 2].g = (float)(*(p2 + 13)) / 255;
                    mesh.colors[i * 6 + 2].b = (float)(*(p2 + 14)) / 255;
                    mesh.colors[i * 6 + 3].r = (float)(*(p0 + 16)) / 255;
                    mesh.colors[i * 6 + 3].g = (float)(*(p0 + 17)) / 255;
                    mesh.colors[i * 6 + 3].b = (float)(*(p0 + 18)) / 255;
                    mesh.colors[i * 6 + 4].r = (float)(*(p1 + 16)) / 255;
                    mesh.colors[i * 6 + 4].g = (float)(*(p1 + 17)) / 255;
                    mesh.colors[i * 6 + 4].b = (float)(*(p1 + 18)) / 255;
                    mesh.colors[i * 6 + 5].r = (float)(*(p2 + 16)) / 255;
                    mesh.colors[i * 6 + 5].g = (float)(*(p2 + 17)) / 255;
                    mesh.colors[i * 6 + 5].b = (float)(*(p2 + 18)) / 255;
                });
            }

            while (meshId < meshList.Count)
            {
                meshList.Remove(meshList[meshId]);
            }
        }
    }
}
