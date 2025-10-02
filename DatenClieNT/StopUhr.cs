
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace DatenClieNT
{
   public class StopUhr
   {
      private DateTime m_dtStartTime;
      private DateTime m_dtStopTime;

      public void Start()
      {
         m_dtStartTime = DateTime.Now;
      }

      public void Stop()
      {
         m_dtStopTime = DateTime.Now;
      }

      public long GetMilliSeconds()
      {
         return (long)((TimeSpan)(m_dtStopTime - m_dtStartTime)).Ticks / 10000;
      }

      public long GetMicroSeconds()
      {
         return (long)((TimeSpan)(m_dtStopTime - m_dtStartTime)).Ticks / 10;
      }

      public long GetNanoSeconds()
      {
         return (long)((TimeSpan)(m_dtStopTime - m_dtStartTime)).Ticks * 100;
      }

      public long GetIntermediateMilliSeconds()
      {
         DateTime dtStopTime = DateTime.Now;
         return (long)((TimeSpan)(dtStopTime - m_dtStartTime)).Ticks / 10000;
      }

      public long GetIntermediateMicroSeconds()
      {
         DateTime dtStopTime = DateTime.Now;
         return (long)((TimeSpan)(dtStopTime - m_dtStartTime)).Ticks / 10;
      }

      public long GetIntermediateNanoSeconds()
      {
         DateTime dtStopTime = DateTime.Now;
         return (long)((TimeSpan)(dtStopTime - m_dtStartTime)).Ticks * 100;
      }

      public static long GetTicksInMs()
      {
         return DateTime.Now.Ticks / 10000;
      }
   }


   public class HiresStopUhr
   {
      [DllImport("Kernel32.dll")]
      private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

      [DllImport("Kernel32.dll")]
      private static extern bool QueryPerformanceFrequency(out long lpFrequency);

      private long startTime, stopTime, freq;

      public HiresStopUhr()
      {
         startTime = 0;
         stopTime = 0;

         if (QueryPerformanceFrequency(out freq) == false)
         {
            throw new Win32Exception();
         }
      }

      public void Start()
      {
         Thread.Sleep(0);
         QueryPerformanceCounter(out startTime);
      }

      public void Stop()
      {
         QueryPerformanceCounter(out stopTime);
      }

      public double IntermediateSeconds()
      {
         long intermediate = 0;
         QueryPerformanceCounter(out intermediate);

         return ((intermediate - startTime) / (double)freq);
      }

      public long Intermediate100NanoSeconds
      {
         get
         {
            long intermediate = 0;
            QueryPerformanceCounter(out intermediate);

            return (long)(10000000 * ((intermediate - startTime) / ((double)freq)));
         }
      }

      public long IntermediateMicroSeconds
      {
         get
         {
            long intermediate = 0;
            QueryPerformanceCounter(out intermediate);

            return (long)(1000000 * ((intermediate - startTime) / ((double)freq)));
         }
      }

      public long IntermediateMilliSeconds
      {
         get
         {
            long intermediate = 0;
            QueryPerformanceCounter(out intermediate);

            return (long)(1000 * ((intermediate - startTime) / ((double)freq)));
         }
      }

      public double PeriodSeconds
      {
         get
         {
            return (stopTime - startTime) / (double)freq;
         }
      }

      public long PeriodMilliSeconds
      {
         get
         {
            return (long)(1000 * ((stopTime - startTime) / (double)freq));
         }
      }

      public long PeriodMicroSeconds
      {
         get
         {
            return (long)(1000000 * ((stopTime - startTime) / (double)freq));
         }
      }

      public long Period100NanoSeconds
      {
         get
         {
            return (long)(10000000 * ((stopTime - startTime) / (double)freq));
         }
      }
   }

   public class HiresZeitstempel
   {
      private static DateTime startTimeStamp;
      private static HiresStopUhr watch = null;

      public static DateTime Now
      {
         get
         {
            if (watch == null)
            {
               startTimeStamp = DateTime.UtcNow;
               watch = new HiresStopUhr();
               watch.Start();
            }

            return startTimeStamp + new TimeSpan(watch.Intermediate100NanoSeconds);
         }
      }

      public static string LogTimeStamp
      {
         get
         {
            return HiresZeitstempel.Now.ToString("UTC: dd.MM.yy HH:mm:ss.fff ");
         }
      }
   }
}

 
