using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using Microsoft.VisualBasic;

namespace DatenClieNT
{
   class DatenTIA : Daten
   {
      public DatenTIA()
      {         
      }
   
      public override void HandleTelegramD(TelegramD telegramD)
      {
         base.HandleTelegramD(telegramD);
      }

      public override Boolean HandleTelegramE(TelegramE telegramE, Telegram telegramSend)
      {
         DataAcknoledge.SetSpsReturnCode(SpsReturnCodes.Ok);

         Boolean onlyQuit = true;

         try
         {

            LogManager.GetSingleton().ZLog("C0200", ELF.INFO, "--------------------------------------------------");
            LogManager.GetSingleton().ZLog("C0201", ELF.INFO, "auftragsNummer = {0}", telegramE.Auftragsnummer);
            LogManager.GetSingleton().ZLog("C0203", ELF.INFO, "Index          = {0}", telegramE.IndexTelegramE);
            LogManager.GetSingleton().ZLog("C0207", ELF.INFO, "--------------------------------------------------");

            DataAcknoledge.SetTelegramIndex(telegramE.IndexTelegramD);

            
            if (!spsAuftraege.ContainsKey(Auftrag.AuftragPrimaryKey(telegramE.Auftragsnummer, anlagenID)))
            {
               DataAcknoledge.SetSpsReturnCode(SpsReturnCodes.JobNotFound);

               LogManager.GetSingleton().ZLog("C020B", ELF.WARNING, "Auftrag nicht in der Datenbank gefunden -> AuftragsNummer = {0} AnlagenID = {1}", telegramE.Auftragsnummer, anlagenID);
               return true;
            }

            Auftrag spsAuftrag = spsAuftraege[Auftrag.AuftragPrimaryKey(telegramE.Auftragsnummer, anlagenID)];

            if ((spsAuftrag.FunktionsNummer == FktNr.FN02RdRowEvtSPS) || (spsAuftrag.FunktionsNummer == FktNr.FN20RdRowEvtSPS))
            {
               onlyQuit = false;

               if(!telegramE.ErrorOccured)
               {
                  if (DatenLesenDB(spsAuftrag, telegramE, telegramSend))
                  {
                     DataAcknoledge.SetSpsReturnCode(SpsReturnCodes.Ok);
                  }
               }
               else
               {
                  DataAcknoledge.SetSpsReturnCode(SpsReturnCodes.Sensorfehler);
                  onlyQuit = true;
               }

               
            }
            else
            {
               if (SafeDatenSchreibenDB(spsAuftrag, telegramE.Data))
               {
                  //...Wenn er bis hierhin kommt hat alles geklappt. Quittung auf OK setzen
                  DataAcknoledge.SetSpsReturnCode(SpsReturnCodes.Ok);
               }
               else
               {
                  //Spezieller SetSpsReturnCode wird in der Funkton DatenSchreibenDB gesetzt !
               }
            }
         }
         catch (Exception e)
         {
            DataAcknoledge.SetSpsReturnCode(SpsReturnCodes.UnspecifiedError);
            LogManager.GetSingleton().ZLog("C020C", ELF.ERROR, "HandleTelegramE -> {0}", e.Message);
            return true;
         }


         return onlyQuit;
      }
      
      public override void HandleTelegramA(TelegramA telegramA)
      {
         try
         {
            long auftragsNummer = telegramA.Auftragsnummer;

            Auftrag auftrag = clientAuftraege[Auftrag.AuftragPrimaryKey(auftragsNummer, anlagenID)];

            HiresStopUhr stopUhr = new HiresStopUhr();            
            stopUhr.Start();
            
            
            if(auftrag.FunktionsNummer == FktNr.FN11WrColUpdEvtDC)
            {
               SqlConnection sqlConnection = null;

               try
               {
                  sqlConnection = TheDC.GetSingleton().GetDatabaseConnectionProvider(datenbank).GetOpenDBConnection();

                  if (sqlConnection == null)
                  {
                     throw new Exception("DatenS7.HandleTelegramA: Störung der Datenbankverbindung !");
                  }
                  
                  foreach (AuftragsDetail auftragsDetail in auftrag.AuftragDetails.Values)
                  {
                     SafeDatenSchreibenDB(auftrag, sqlConnection, auftragsDetail, telegramA.Data[auftragsDetail.Id.ToString()]);

							//SafeDatenSchreibenDB(auftrag, sqlConnection, auftragsDetail, telegramA.Data[auftragsDetail);
                  }
               }
               catch (SqlException sqlE)
               {
                  if (sqlE.ErrorCode == (-2146232060))
                  {
                     LogManager.GetSingleton().ZLog("C020F", ELF.ERROR, "DatenEintragen -> Auftrag = {0} -> {1}", auftrag.Bezeichnung, sqlE.Message);
                     LogManager.GetSingleton().ZLog("C0210", ELF.ERROR, "Ist die Tabelle {0} in der Datenbank vorhanden ? Stimmen alle Spaltennamen ? Stimmen alle Datentypen ?", auftrag.TabellenName);
                  }
                  else
                  {
                     LogManager.GetSingleton().ZLog("C0211", ELF.ERROR, "DatenEintragen -> Auftrag = {0}", auftrag.Bezeichnung);
                     LogManager.GetSingleton().ZLog("C0212", ELF.ERROR, "DatenEintragen -> {0}", sqlE.Message);
                  }
               }
               catch (Exception e)
               {
                  LogManager.GetSingleton().ZLog("C0213", ELF.ERROR, "DatenEintragen -> Auftrag = {0}", auftrag.Bezeichnung);
                  LogManager.GetSingleton().ZLog("C0214", ELF.ERROR, "DatenEintragen -> {0}", e.Message);
               }
               finally
               {
                  if (sqlConnection != null)
                  {
                     sqlConnection.Close();
                     sqlConnection = null;
                  }
               }               
            }
            else
            {
               SafeDatenSchreibenDB(auftrag, telegramA.Data);
            } 

            stopUhr.Stop();

            if (auftrag.FunktionsNummer == FktNr.FN11WrColUpdEvtDC)
            {
               LogManager.GetSingleton().ZLog("C0215", ELF.INFO, "DATENEINTRAGEN11 {0} = {1}", auftrag.Bezeichnung, stopUhr.PeriodMilliSeconds);
            }
            else
            {
               LogManager.GetSingleton().ZLog("C0216", ELF.INFO, "DATENEINTRAGEN {0} = {1}", auftrag.Bezeichnung, stopUhr.PeriodMilliSeconds);
            }
         }         
         catch(Exception e)
         {
            LogManager.GetSingleton().ZLog("C0217", ELF.ERROR, "DatenS7.HandleTelegramA -> Auftragsnummer konnte nicht ermittelt werden oder Auftrag nicht gefunden -> telegram = {0} -> Message = {1}", telegramA.TeleType, e.Message);
         }
      }      
   }
}
