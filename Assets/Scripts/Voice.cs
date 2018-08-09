using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Voice : MonoBehaviour
{
    private const float DURATION = 4f;
    private static float beginTime = -1;
    private float lastDuration = 0;
    public AudioClip[] clips;
    public AudioSource audioSource;

    void Start()
    {
        bool valid = false;
        if (File.Exists("Bar.cfg"))
        {
            string[] strs = File.ReadAllLines("Bar.cfg");
            if (strs[0][0] == '1')
            {
                valid = true;
            }
        }
        gameObject.SetActive(valid);
    }

    public static void startTiming()
    {
        if (beginTime < 0f)
        {
            beginTime = Time.realtimeSinceStartup;
        }
    }

    void Update()
    {
        float currTime = Time.realtimeSinceStartup;

        if (beginTime >= 0f)
        {
            float duration = currTime - beginTime;
            duration = duration / DURATION;
            duration -= Mathf.Floor(duration);
            if (duration >= 0.5 && lastDuration < 0.5)
            {
                audioSource.PlayOneShot(clips[0]);
            }
            if (duration >= 0.75 && lastDuration < 0.75)
            {
                audioSource.PlayOneShot(clips[0]);
            }
            if (duration < lastDuration)
            {
                audioSource.PlayOneShot(clips[1]);
            }
            lastDuration = duration;
        }
    }


}
