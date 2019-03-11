using System.Threading;
using UnityEngine;

public abstract class SyncedThread
{
    private Thread childThread = null;
    private EventWaitHandle childThreadHandle = new EventWaitHandle(true, EventResetMode.ManualReset);
    private EventWaitHandle mainThreadHandle = new EventWaitHandle(true, EventResetMode.ManualReset);

    private bool running;

    private bool stopRequested;

    private int loopCounter;

    private float deltaTime;

    private float time;

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
            //if (thread.Name.Equals("ThreadCell2"))
            //    Thread.Sleep(500);
            UpdateThread(time, deltaTime);
            loopCounter++;
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
        loopCounter = 0;
        time = Time.time;
        deltaTime = Time.deltaTime;
        childThreadHandle.Set();
    }
}