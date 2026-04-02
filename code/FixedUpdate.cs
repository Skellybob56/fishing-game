using System.Diagnostics;

namespace FishingGame;

static partial class Engine
{
    public const int FixedUpdateIntervalMSec = 50;
    public const double FixedUpdateInterval = FixedUpdateIntervalMSec / 1000d;
    public const float FixedUpdateIntervalF = (float)FixedUpdateInterval;
    static Stopwatch stopwatchFixedUpdate = new();
    static long lastTickTimeFixedMSec;
    static long lastTickTimeSharedMsec;
    public static int CurrentTick { get; private set; } = 0;
    static readonly Lock SharedDataLock = new();

    public static void FixedUpdateLoop()
    {
        stopwatchFixedUpdate.Start();

        lastTickTimeFixedMSec = stopwatchFixedUpdate.ElapsedMilliseconds;
        lock (SharedDataLock)
        {
            lastTickTimeSharedMsec = lastTickTimeFixedMSec;
        }
        while (Running)
        {
            long currentTimeFixedMSec;
            long nextTickTimeFixedMSec;
            int remainingTimeFixedMSec;

            FixedUpdate();

            currentTimeFixedMSec = stopwatchFixedUpdate.ElapsedMilliseconds;
            nextTickTimeFixedMSec = lastTickTimeFixedMSec + FixedUpdateIntervalMSec;
            remainingTimeFixedMSec = (int)(nextTickTimeFixedMSec - currentTimeFixedMSec);
            if (remainingTimeFixedMSec > 0)
            {
                if (remainingTimeFixedMSec > 10)
                { // 10 msec buffer for inaccuracy
                    Thread.Sleep(remainingTimeFixedMSec - 10);
                }

                while (stopwatchFixedUpdate.ElapsedMilliseconds < nextTickTimeFixedMSec) { }
            }

            lastTickTimeFixedMSec = stopwatchFixedUpdate.ElapsedMilliseconds;

            lock (SharedDataLock)
            {
                lastTickTimeSharedMsec = lastTickTimeFixedMSec;
                SaveToSharedData();
                ++CurrentTick;
            }
        }
    }

    static void FixedUpdate()
    {
        controller.FixedUpdate();
        player.actor.FixedUpdate();
    }

    static void SaveToSharedData()
    {
        player.actor.SaveToSharedData();
    }
}
