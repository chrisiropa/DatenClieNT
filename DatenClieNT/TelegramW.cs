using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public class TelegramW : Telegram
   {
      

      public TelegramW(string subscriptionItem, Dictionary<string, string> registrationItems)
         : base(subscriptionItem, registrationItems)
      {
         teleType = "W";

         
      }

      public override Telegram DeepCopy()
      {
         TelegramW telegram = (TelegramW) Telegram.NewInstance("", null, "W");

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

   }
}
