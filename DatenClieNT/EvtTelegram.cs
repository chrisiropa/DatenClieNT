using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public class EvtTelegram : Evt
   {
      private Telegram telegram;

      public Telegram Telegram
      {
         get { return telegram; }
      }

      public EvtTelegram(Telegram telegram)
         : base(Type.Telegram)
      {
         this.telegram = telegram;
      }

      public override string ToString()
      {
         return string.Format("TELEGRAM-EVENT {0}", telegram.TeleType);
      }
   }
}