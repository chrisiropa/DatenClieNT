using System;
using System.Collections.Generic;

namespace DatenClieNT
{
   public class TelegramT : Telegram
   {

		private int indexTelegramS = -1;	//Veränderungskennung von der SPS

      public TelegramT(string subscriptionItem, Dictionary<string, string> registrationItems)
         : base(subscriptionItem, registrationItems)
      {
         teleType = "T";
      }

		public int IndexTelegramS
      {
         //indexTelegramD ist richtig ! Es handelt sich hier um den Anreiz, der
         //mit dem D-Telegramm gekommen ist und hier gespeichert wird.
         set { indexTelegramS = value; }
         get { return indexTelegramS; }
      }


      public override Telegram DeepCopy()
      {
         TelegramT telegram = (TelegramT)Telegram.NewInstance("", null, "T");

         telegram.Counter = this.Counter;
			indexTelegramS = this.IndexTelegramS;

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
         //durchrouten der values bis in Alarming und dann da zerpflücken und in DB schreiben vorbereiten
         base.SetRxData(keyValues, quality, timestamp);

         
      }
   }
}
