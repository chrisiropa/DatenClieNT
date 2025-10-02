using System;
using System.Collections.Generic;
using System.Text;
using Opc.Ua;
using Opc.Ua.Client;

namespace DatenClieNT
{
   public class Telegram
   {
      protected string teleType; //A..Z
      protected string subscriptionItem;
      protected Dictionary<string, string> registrationItems = null;
      protected Dictionary<string, string> registrationValues = null;
      private long counter;
      protected string errorText = "";

      private static long nextCounter = 0;

      private bool active;

      public Dictionary<string, string> RegistrationItems
      {
         get
         {
            return registrationItems;
         }
      }

      public long Counter
      {
         get
         {
            return counter;
         }

         set
         {
            counter = value;
         }
      }

      public Dictionary<string, string> RegistrationValues
      {
         get
         {
            return registrationValues;
         }

         set
         {
            registrationValues = value;
         }
      }

      public Dictionary<string, object> Data
      {
         get
         {
            return data;
         }
      }

      //Empfangsdaten / Sendedaten
      private object subscribtionValue;
      private object txBuffer;
      private string quality;
      private DateTime timeStamp;

      private Dictionary<string, object> data;


      private bool available = false;
      private bool readable = false;
      
      //Visualisierung / Logging
      private string visuData;
      private string sendVisuData;
      
      //Initialisierung
      private int byteLen = 0;
      
      
      public string TeleType
      {
         get { return teleType; }
         set { teleType = value; }
      }
      
      public string ErrorText
      {
         get { return errorText; }
         set { errorText = value; }
      }

      public Boolean ErrorOccured
      {
         get { return errorText != ""; }
      }
     

      public virtual string SubscriptionItem
      {
         get { return subscriptionItem; }
         set { subscriptionItem = value; }
      }

      public bool Active
      {
         get { return active; }
         set { active = value; }
      }

      public bool Readable
      {
         get { return readable; }
         set { readable = value; }
      }

      public bool Available
      {
         get { return available; }
         set { available = value; }
      }

      public int ByteLen
      {
         get { return byteLen; }
         set { byteLen = value; }
      }
      
      public object SubscribtionValue
      {
         get { return subscribtionValue; }
      }

      public object TxBuffer
      {
         get { return txBuffer; }
      }

      public string Quality
      {
         get { return quality; }
      }

      public DateTime TimeStamp
      {
         get { return timeStamp; }
      }  
      
      public string VisuData
      {
         set { visuData = value; }
         get { return visuData; }
      }

      public string SendVisuData
      {
         set { sendVisuData = value; }
         get { return sendVisuData; }
      }

      
      public static Telegram NewInstance(string subscriptionItem, Dictionary<string, string> registrationItems,  string teleType)
      {
         
         switch (teleType[0])
         {
            case 'W': return new TelegramW(subscriptionItem, registrationItems);
            case 'S': return new TelegramS(subscriptionItem, registrationItems);
            case 'T': return new TelegramT(subscriptionItem, registrationItems);
            case 'U': return new TelegramU(subscriptionItem, registrationItems);
            case 'D': return new TelegramD(subscriptionItem, registrationItems);
            case 'E': return new TelegramE(subscriptionItem, registrationItems);
            case 'F': return new TelegramF(subscriptionItem, registrationItems);
            case 'A': return new TelegramA(subscriptionItem, registrationItems);
            case 'M': return new TelegramM(subscriptionItem, registrationItems);

         }

         return new Telegram(subscriptionItem, registrationItems);
      }

           
      public Telegram(string subscriptionItem, Dictionary<string, string> registrationItems)
      {
         this.counter = ++nextCounter;
         this.subscriptionItem = subscriptionItem;
         this.registrationItems = new Dictionary<string, string>();
         this.registrationValues = new Dictionary<string, string>();

         if (registrationItems != null)
         {
            foreach (string key in registrationItems.Keys)
            {
               this.registrationItems[key] = registrationItems[key];
            }
         }
      }


      public void SetRegistrationItems(Dictionary<string, string> registrationItems)
      {
         this.registrationItems = new Dictionary<string, string>();

         if (registrationItems != null)
         {
            foreach (string key in registrationItems.Keys)
            {
               this.registrationItems[key] = registrationItems[key];
            }
         }

         if (teleType == "M")
         {
         }
      }

      public virtual Telegram DeepCopy()
      {
         throw new Exception("DeepCopy in den abgeleiteten Klassen lösen....");
      }
      
      
      
      public void SetRxData(object buffer, string quality, DateTime timeStamp)
      {
         this.subscribtionValue = buffer;
         this.quality = quality;
         this.timeStamp = timeStamp;
      }

      public void SetTxData(object buffer)
      {
         this.txBuffer = buffer;
      }

      public virtual void SetRxData(Dictionary<string, object> daten, string quality, DateTime timeStamp)
      {
         this.quality = quality;
         this.timeStamp = timeStamp;

         this.data = daten;
      }
   }
}
