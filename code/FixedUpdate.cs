using System.Diagnostics;

namespace FishingGame;

static partial class Engine
{
    public const int FixedUpdateIntervalMSec = 50;
    public const double FixedUpdateInterval = FixedUpdateIntervalMSec / 1000d;
    public const float FixedUpdateIntervalF = (float)FixedUpdateInterval;
    static long currentTick = 0;

    public static void FixedUpdateLoop()
    {
        Stopwatch stopwatchFixedUpdate = new Stopwatch();
        stopwatchFixedUpdate.Start();

        Console.WriteLine(Running);
        while (Running)
        {
            long currentTimeFixedMSec;
            long nextTickTimeFixedMSec;
            long tickStartTimeFixedMSec;
            int remainingTimeFixedMSec;

            tickStartTimeFixedMSec = stopwatchFixedUpdate.ElapsedMilliseconds;
            FixedUpdate();
            ++currentTick;

            currentTimeFixedMSec = stopwatchFixedUpdate.ElapsedMilliseconds;
            nextTickTimeFixedMSec = tickStartTimeFixedMSec + FixedUpdateIntervalMSec;
            remainingTimeFixedMSec = (int)(nextTickTimeFixedMSec - currentTimeFixedMSec);
            if (remainingTimeFixedMSec > 0)
            {
                if (remainingTimeFixedMSec > 10)
                { // 10 msec buffer for inaccuracy
                    Thread.Sleep(remainingTimeFixedMSec - 10);
                }

                while (stopwatchFixedUpdate.ElapsedMilliseconds < nextTickTimeFixedMSec) { }
            }
        }
    }

    static void FixedUpdate()
    {
        player.FixedUpdate();
    }
}
