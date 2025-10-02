using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public class TelegramF : Telegram
   {

      public TelegramF(string subscriptionItem, Dictionary<string, string> registrationItems)
         : base(subscriptionItem, registrationItems)
      {
         teleType = "F";
      }


      public override Telegram DeepCopy()
      {
         TelegramF telegram = (TelegramF)Telegram.NewInstance("", null, "F");

         telegram.Counter = this.Counter;

         telegram.TeleType = this.TeleType;
         telegram.Active = this.Active;
         telegram.Available = this.Available;
         telegram.Readable = this.Readable;
         telegram.ByteLen = this.ByteLen;
         telegram.errorText = ErrorText;

         telegram.SubscriptionItem = this.SubscriptionItem;

         telegram.SetRegistrationItems(this.registrationItems);

         return telegram;
      }

      public override void SetRxData(Dictionary<string, object> keyValues, string quality, DateTime timestamp)
      {
         base.SetRxData(keyValues, quality, timestamp);
      }
   }
}
