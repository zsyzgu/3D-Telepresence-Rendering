using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

public class CallPCL : MonoBehaviour
{
    const int POINT_BYTES = 16;

    [DllImport("pc-recog", EntryPoint = "callStart")]
    public static extern void callStart();

    [DllImport("pc-recog", EntryPoint = "callUpdate")]
    public static extern IntPtr callUpdate();

    [DllImport("pc-recog", EntryPoint = "callRegistration")]
    public static extern void callRegistration();

    [DllImport("pc-recog", EntryPoint = "callSetBackground")]
    public static extern void callSetBackground();

    [DllImport("pc-recog", EntryPoint = "callSaveScene")]
    public static extern void callSaveScene();

    [DllImport("pc-recog", EntryPoint = "callStop")]
    public static extern void callStop();

    public static void getMesh(ref List<MeshInfos> meshList, int vMax)
    {
        unsafe
        {
            byte* ptr = (byte*)callUpdate();
            int size = *((int*)ptr) * 3;
            ptr = ptr + 4;

            int meshId = 0;
            for (int st = 0; st < size; st += vMax, meshId++)
            {
                if (meshId >= meshList.Count)
                {
                    meshList.Add(new MeshInfos());
                }
                MeshInfos mesh = meshList[meshId];

                int len = Math.Min(size - st, vMax);
                if (mesh.vertexCount != len)
                {
                    mesh.vertexCount = len;
                    mesh.vertices = new Vector3[len];
                    mesh.colors = new Color[len];
                }

                Parallel.For(0, len, i => {
                    int id = (st + i) * POINT_BYTES;

                    byte* p = ptr + id;
                    mesh.vertices[i] = *((Vector3*)p + 0);
                    mesh.colors[i].r = (float)(*(p + 12)) / 255;
                    mesh.colors[i].g = (float)(*(p + 13)) / 255;
                    mesh.colors[i].b = (float)(*(p + 14)) / 255;
                });
            }

            while (meshId < meshList.Count)
            {
                meshList.Remove(meshList[meshId]);
            }
        }
    }
}
