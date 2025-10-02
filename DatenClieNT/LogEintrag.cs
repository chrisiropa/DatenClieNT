using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace DatenClieNT
{
   public enum ELF
   {
      INFO        = 0x0001,
      WARNING     = 0x0002,
      ERROR       = 0x0004,
      DEVELOPER   = 0x0008,
      TELE        = 0x0010,
      STATUS      = 0x0020,
      
   }
   
   public struct LogEintrag
   {
      private ELF logFlags;
      private string text;
      private DateTime zeitStempel;
      private string threadInfo;
      private string identifier;
      
      public LogEintrag(string identifier, ELF logFlags, string text, DateTime zeitStempel, string threadInfo)
      {
         this.logFlags = logFlags;
         this.text = text;
         this.zeitStempel = zeitStempel;
         this.threadInfo = threadInfo;
         this.identifier = identifier;
      }

      public ELF LogFlags
      {
         get { return logFlags; }
      }

      public string LogFlagAsText
      {
         get
         {
            string timeZoneInfo = "";
            
            switch (logFlags)
            {
               case ELF.INFO: return string.Format("INF{0}", timeZoneInfo);
               case ELF.WARNING: return string.Format("WAR{0}", timeZoneInfo);
               case ELF.DEVELOPER: return string.Format("DEV{0}", timeZoneInfo);
               case ELF.ERROR: return string.Format("ERR{0}", timeZoneInfo);
               case ELF.TELE: return string.Format("TEL{0}", timeZoneInfo);
               case ELF.STATUS: return string.Format("STS{0}", timeZoneInfo);
            }
            return "UNK";
         }
      }

      public string Text
      {
         get { return text; }
      }

      public DateTime ZeitStempel
      {
         get { return zeitStempel; }
      }

      public string ThreadInfo
      {
         get { return threadInfo; }
      }

      public string Identifier
      {
         get { return identifier; }
      }
      
      public ConsoleColor GetColor(ELF elf)
      {
         switch(elf)
         {
            case ELF.STATUS: return ConsoleColor.DarkGreen;
            case ELF.DEVELOPER: return ConsoleColor.DarkCyan;
            case ELF.ERROR: return ConsoleColor.Red;
            case ELF.INFO: return Console.ForegroundColor;
            case ELF.TELE: return ConsoleColor.Gray;
            case ELF.WARNING: return ConsoleColor.Yellow;
         }
         
         return Console.ForegroundColor;
      }
   }
}
