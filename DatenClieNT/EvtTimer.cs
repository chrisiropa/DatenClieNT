using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public class EvtTimer : Evt
   {
      private event TimerDelegate timerEvent;
      private object tag;

      private bool abo = false;
      private long delay;
      private bool immediate = false;
      private long nextTimeout = 0;
      private long counter = 0;
      
      public bool Abo
      {
         get { return abo; }
      }

      public object Tag
      {
         get { return tag; }
      }

      public TimerDelegate TimerEvent
      {
         get { return timerEvent; }
      }
      
      public long Delay
      {
         get { return delay; }
      }

      private HiresStopUhr stopUhr = new HiresStopUhr();

      public EvtTimer(TimerDelegate timerEvent, object tag, bool abo, long delay, bool immediate)
         : base(Type.Timer)
      {
         this.timerEvent = timerEvent;
         this.tag = tag;
         this.delay = delay;
         this.abo = abo;
         this.immediate = immediate;
         
         stopUhr.Start();
         
         CalcNextTimeout();
      }

      private void CalcNextTimeout()
      {
         nextTimeout = delay * ++counter;
      }

      public override string ToString()
      {
         return string.Format("TIMER-EVENT {0} Delay={1} Abo={2}", tag, delay, abo);
      }

      public bool TimedOut()
      {
         if (immediate)
         {
            immediate = false;
            return true;
         }
         
         if (stopUhr.IntermediateMilliSeconds >= nextTimeout)
         {
            CalcNextTimeout();
            
            return true;
         }

         return false;
      }
   }
}