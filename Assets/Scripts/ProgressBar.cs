using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ProgressBar : MonoBehaviour
{
    private const float DURATION = 1.5f;
    public GameObject subBar;

    private static float beginTime = -1;

    private void Start()
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
        ProgressBar.startTiming();
    }

    public static void startTiming()
    {
        if (beginTime < 0f)
        {
            beginTime = Time.realtimeSinceStartup;
        }
    }
	
	void Update () {
        float currTime = Time.realtimeSinceStartup;

        if (beginTime >= 0f)
        {
            float duration = currTime - beginTime;
            duration = duration / DURATION;
            duration -= Mathf.Floor(duration);
            subBar.transform.localScale = new Vector3(1.05f, duration, 1.05f);
            subBar.transform.localPosition = new Vector3(0f, -0.5f + duration / 2, 0f);
        }
	}
}
