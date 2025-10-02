using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public class TelegramU : Telegram
   {

      public TelegramU(string subscriptionItem, Dictionary<string, string> registrationItems)
         : base(subscriptionItem, registrationItems)
      {
         teleType = "U";
      }


      public override Telegram DeepCopy()
      {
         TelegramU telegram = (TelegramU)Telegram.NewInstance("", null, "U");

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
