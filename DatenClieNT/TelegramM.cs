using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public class TelegramM : Telegram
   {
      
      public TelegramM(string subscriptionItem, Dictionary<string, string> registrationItems)
      : base(subscriptionItem, registrationItems)
      {
         teleType = "M";
      }

      public override Telegram DeepCopy()
      {
         TelegramM telegram = (TelegramM)Telegram.NewInstance("", null, "M");
         
         telegram.Counter = this.Counter;

         telegram.errorText = ErrorText;
         telegram.TeleType = this.TeleType;
         telegram.Active = this.Active;
         telegram.Available = this.Available;
         telegram.Readable = this.Readable;
         telegram.ByteLen = this.ByteLen;

         telegram.SubscriptionItem = this.SubscriptionItem;

         telegram.SetRegistrationItems(this.registrationItems);

         return telegram;
      }
   }
}
