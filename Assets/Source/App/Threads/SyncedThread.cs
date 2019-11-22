using System.Threading;
using UnityEngine;

public abstract class SyncedThread
{
    private Thread childThread = null;
    private readonly EventWaitHandle childThreadHandle = new EventWaitHandle(true, EventResetMode.ManualReset);
    private readonly EventWaitHandle mainThreadHandle = new EventWaitHandle(true, EventResetMode.ManualReset);

    private bool running = default;
    private bool stopRequested = default;
    private float deltaTime = default;
    private float time = default;

    protected abstract void UpdateThread(float time, float deltaTime);

    public abstract void PreSyncThread();

    public abstract void SyncThread();

    public SyncedThread(out EventWaitHandle threadWait)
    {
        threadWait = mainThreadHandle;
    }

    private void ChildThreadLoop()
    {
        childThreadHandle.Reset();
        childThreadHandle.WaitOne();
        while (running)
        {
            childThreadHandle.Reset();
            UpdateThread(time, deltaTime);
            WaitHandle.SignalAndWait(mainThreadHandle, childThreadHandle); //Signals one WaitHandle and waits on another.
        }
        WaitHandle.SignalAndWait(mainThreadHandle, childThreadHandle);
    }

    public void SyncStopRequest()
    {
        running = !stopRequested;
    }

    public void RequestStopThread()
    {
        stopRequested = true;
    }

    public void StartThread()
    {
        if (childThread == null)
        {
            running = true;
            stopRequested = false;
            childThread = new Thread(ChildThreadLoop);
            childThread.Start();
        }
        else
        {
            ResumeThread();
        }
    }

    public void ResumeThread()
    {
        running = true;
        stopRequested = false;
        mainThreadHandle.WaitOne();
        mainThreadHandle.Reset();
        time = Time.time;
        deltaTime = Time.deltaTime;
        childThreadHandle.Set();
    }
}