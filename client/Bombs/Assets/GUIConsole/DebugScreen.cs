using UnityEngine;
using System.Collections;

[System.Reflection.Obfuscation(Exclude = true)]
public class DebugScreen : MonoBehaviour
{
    public int FPS = 30;
    public bool DebugFPS = false;
	private GUIStyle S = new GUIStyle();

    // Use this for initialization
    void Awake()
    {
		S.normal.background = null;
		S.fontSize = 40;
        Application.targetFrameRate = 30;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    // Update is called once per frame
    void Update()
    {
        if (DebugFPS)
        UpdateTick();
    }

    void OnGUI()
    {
        if (DebugFPS)
        DrawFps();
    }

    private void DrawFps()
    {
        if (mLastFps > 50)
        {
            GUI.color = Color.red;
        }
        else if (mLastFps > 40)
        {
            GUI.color = Color.red;
        }
        else
        {
            GUI.color = Color.red;
        }

        GUI.Label(new Rect(50, 300, 100, 100), "fps: " + mLastFps,S);

    }

    private long mFrameCount = 0;
    private long mLastFrameTime = 0;
    static long mLastFps = 0;
    private void UpdateTick()
    {
        if (true)
        {
            mFrameCount++;
            long nCurTime = TickToMilliSec(System.DateTime.Now.Ticks);
            if (mLastFrameTime == 0)
            {
                mLastFrameTime = TickToMilliSec(System.DateTime.Now.Ticks);
            }

            if ((nCurTime - mLastFrameTime) >= 1000)
            {
                long fps = (long)(mFrameCount * 1.0f / ((nCurTime - mLastFrameTime) / 1000.0f));

                mLastFps = fps;

                mFrameCount = 0;

                mLastFrameTime = nCurTime;
            }
        }
    }
    public static long TickToMilliSec(long tick)
    {
        return tick / (10 * 1000);
    }
}