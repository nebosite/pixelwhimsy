using System;
using System.Runtime.InteropServices;
using System.Threading;

/// --------------------------------------------------------------------------
/// <summary>
/// A very simle high performance timer based on QueryPerformanceCounter
/// </summary>
/// --------------------------------------------------------------------------
public class HiPerfTimer
{
    [DllImport("Kernel32.dll")]
    private static extern bool QueryPerformanceCounter(
        out long lpPerformanceCount);

    [DllImport("Kernel32.dll")]
    private static extern bool QueryPerformanceFrequency(
        out long lpFrequency);

    private long startTime = 0;
    private long stopTime = 0;
    private long freq;

    /// --------------------------------------------------------------------------
    /// <summary>
    /// Constructor
    /// </summary>
    /// --------------------------------------------------------------------------
    public HiPerfTimer()
    {
        startTime = 0;
        stopTime = 0;

        if (!QueryPerformanceFrequency(out freq) )
            throw new ApplicationException("high-performance counter not supported");
    }

    /// --------------------------------------------------------------------------
    /// <summary>
    /// Start the Timer
    /// </summary>
    /// --------------------------------------------------------------------------
    public void Start()
    {
        Thread.Sleep(0); // Let other waiting threads execute
        QueryPerformanceCounter(out startTime);
    }
    
    /// --------------------------------------------------------------------------
    /// <summary>
    /// Get the # of seconds elapsed since start was called
    /// </summary>
    /// --------------------------------------------------------------------------
    public double ElapsedSeconds
    {
        get
        {
            QueryPerformanceCounter(out stopTime);
            return (double)(stopTime - startTime) / (double)freq;
        }
    }
}

