using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public delegate void EventDelegate(Evt evt);

   public class Evt : Object
   {
      public enum Type
      {
         Unknown = -1,
         Alive = 0,
         Timer = 1,
         Telegram = 2,
         Terminate = 3,
         SuspendPiping = 4,
         ResumePiping = 5
      }
      
      private Type type = Type.Unknown;
      
      private DateTime createStamp = DateTime.MinValue;
      private DateTime pipeStamp = DateTime.MinValue;

      public DateTime CreateStamp
      {
         get
         {
            return createStamp;
         }
      }

      public DateTime PipeStamp
      {
         get
         {
            return pipeStamp;
         }
         
         set
         {
            pipeStamp = value;
         }
      }
      
      public int PipeDelay
      {
         get
         {
            return (int)(DateTime.UtcNow - pipeStamp).TotalMilliseconds;
         }
      }

      public Type EvtType
      {
         get { return type; }
      }
      
      protected Evt(Type type)
      {
         this.type = type;
         
         this.createStamp = DateTime.Now;
      }

      public override string ToString()
      {
         return string.Format("Evt.Type = {0} (ToString() nicht überschrieben)", type);
      }

      public static EvtSuspend NewSuspendEvt()
      {
         EvtSuspend evt = new EvtSuspend();

         return evt;
      }

      public static EvtResume NewResumeEvt()
      {
         EvtResume evt = new EvtResume();

         return evt;
      }

      public static EvtTimer NewTimerEvt(TimerDelegate timerEvent, object tag, bool abo, long delay, bool imediate)
      {
         EvtTimer evt = new EvtTimer(timerEvent, tag, abo, delay, imediate);
         
         return evt;
      }

      public static EvtTelegram NewTelegramEvt(Telegram telegram)
      {
         EvtTelegram evt = new EvtTelegram(telegram);
         
         return evt;
      }

      public static EvtTerminate NewTerminateEvt()
      {
         EvtTerminate evt = new EvtTerminate();

         return evt;
      }
   }
}
