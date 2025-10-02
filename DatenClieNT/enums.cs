
using System;
using System.Collections.Generic;
using System.Text;


namespace DatenClieNT
{
   public static class Ascii
   {
      public static char SOH = (char)1;
      public static char STX = (char)2;
      public static char ETX = (char)3;
      public static char US = (char)31;
   }

   public enum TelegramConnectType
   {
      subscription,
      registration
   }
   
   public enum SpsReturnCodes
   {
      //Folgende Codes können an die SPS zurückgegeben werden nach dem 
      //ein Auftrag abgearbeitet wurde.
      
      //ACHTUNG:
      //PC sendet als Quittung die AnforderungsID aus Telegramm D
      //SPS MUSS nicht aktuelle AnforderungsIDs IGNORIEREN
      //Doppelte D-Telegramme -> 2. verwerfen
      //Unterschiedliche ID's dürften dann nur noch direkt nach dem Starten
      //passieren, da auf der SPS die 6 sek. gerade rum sein könnten.
      //Die SPS weiß ja nicht ob ein OPC auf sie zugreift.
      
      //Eventuell das erste initiale D-Telegramm verwerfen. Dann dürfte nie 
      //mehr unterschiedliche ID's kommen 

      //-----------------------------------------------------------------
	   //GRUPPE 0..99 -> Alles in Ordnung und weiter

      //Daten Schreiben/Lesen ohne Fehler ausgeführt               
      Ok                            = 0,

      //Eine Anfrage an die DB lieferte keinen Datensatz zurück      
      //Kein Fehler, SPS macht weiter
      DatabaseStatementNoData       = 1,
      //-----------------------------------------------------------------


      //-----------------------------------------------------------------
	   //GRUPPE 100..199 -> Fehler zur Infozwecken aber trotzdem weiter
      
      //SPS sendet Auftragsnummer, welche nicht konfiguriert ist
      JobNotFound                   = 100,      

      //SyntaxError in Datenbank-Statement
      //SPS macht trotzdem weiter, Nur Fehleranzeige in Visu
      //Kein erneutes Senden des gleichen Telegramms
      DatabaseStatementSyntaxError = 101,

      //Ein oder mehrere Werte von der SPS sind inkonsistent zur
      //korrespondierenden Datenbankspalte. 
      Sps2DbConversionError         = 102,

      //Wenn für ein AuftragsDetail der SPS der defaultWert "0000" 
      //zurückgegeben wird, weil eine Konvertierung fehlgeschlagen 
      //ist sollte dies zusätzlich mit diesem Error quittiert werden
      Db2SpsConversionError         = 103,

      
      //-----------------------------------------------------------------


      //-----------------------------------------------------------------
	   //GRUPPE 200..299 -> Fehler führt zu sofortiger Neusendung der 
      //                   Änderungskennung (Wiederholung)

	   //Unterschiedliche ID's von Anreiz und Daten
      //Die SPS braucht dann nicht mehr die vollen 6 Sekunden warten
      //weil gar nicht quittiert wird (wie früher), sondern er kann
      //den Anreiz z.B. schon nach 100ms nochmal setzen.
      DifferentIds                  = 200,
      //Nicht näher spezifizierter Fehler
      UnspecifiedError              = 201,  //Früher bis 24.10.2013 war dies der Fehler 104 !
      //-----------------------------------------------------------------

      //-----------------------------------------------------------------
	   //GRUPPE 300..399 -> Fehler führt zur zeitverzögerten Neusendung 
      //        			   der Änderungskennung (Wiederholung)

      //Datenbank nicht erreichbar
      DatabaseUnavailable           = 300,
      //-----------------------------------------------------------------

      //OPC Quality BAS
      OpcQualityBad = 301,
      Datenwandlung = 302,  //Vermutlich Wandlungsfehler (Diskrepanz Datentyp Tabellenspalte/SPS-Item)
      Sensorfehler  = 303,  //DatenClientItem/AuftragsItem falsch geschrieben (Achtung: case sensitiv)
      //-----------------------------------------------------------------
   }

   public enum FktNr
   {
      FN00_Keine = 0,

      //Anreiz kommt von der SPS
      //Zeilenweises Schreiben in die Datenbank; 
      //Breite Datentabelle, Feldnamen stehen in DC_AuftragsDetailsTIA
      //Neue Daten werden mit insert geschrieben; Tabelle wird also immer größer
      //ACHTUNG: Wenn die Spalte "UpdateKrit" mit einer DC_AuftragsDetailsTIA gefüllt ist wird 
      //         ein Update auf die Datentabelle gemacht mit "where (DatenbankWert von DC_AuftragsDetailsTIA == SPSWert)
      //         Wenn das Update sich auf 0 Sätze auswirkt, wird ein insert gemacht.
      //         Das UpdateKrit macht natürlich nur Sinn wenn es sich um eine ID (z.B. KastenID o.ä.) handelt.
      //         Sie sollte in der Tabelle eindeutig sein.
      //Beispiel: Kastendaten
      FN01WrRowInsEvtSPS = 1,
      
      //Anreiz kommt von der SPS
      //Spaltenweises Schreiben in DB
      //Schmale Datentabelle in der die AuftragsDetail_ID mit gespeichert wird zum Wiederfinden
      //Neue Daten werden inserted. Tabelle wird also immer größer
      FN05WrColInsEvtSPS = 5,

      //Anreiz kommt von diesem DatenClient (Polling mit "DC_AufträgeTIA.UpdateIntervall")
      //Spaltenweises Schreiben in DB
      //Schmale Datentabelle in der die AuftragsDetail_ID mit gespeichert wird zum Wiederfinden
      //Neue Daten werden upgedatet. Tabelle enthält also immer nur die aktuellen Daten
      //Beispiel: OnlineMesswerte      
      FN11WrColUpdEvtDC = 11,

      //Anreiz kommt von diesem DatenClient (Polling mit "DC_AufträgeTIA.UpdateIntervall")
      //Zeilenweises Schreiben in die Datenbank; 
      //Breite Datentabelle, Feldnamen stehen in DC_AuftragsDetailsTIA
      //Neue Daten werden mit insert geschrieben; Tabelle wird also immer größer
      //UPDATE 14.09.2017 CG: Für Hansberg wurde die Funktion nun doch für updates erweitert.
      //Wenn ein UpdateKriterium angegeben ist und schon ein Eintrag dafür existiert 
      //wird also auch upgedatet !
      //Beispiele: Messwerte1, Induktordaten  
      FN12WrRowInsEvtDC = 12,

      //Anreiz kommt von diesem DatenClient (Polling mit "DC_AufträgeTIA.UpdateIntervall")
      //Spaltenweises Schreiben in DB
      //Schmale Datentabelle in der die AuftragsDetail_ID mit gespeichert wird zum Wiederfinden
      //Neue Daten werden inserted. Tabelle wird also immer größer
      FN13WrColInsEvtDC = 13,

      //LESEN aus Datenbank
      //Zeilenweises Lesen aus DB aus breiter Datentabelle
      //In der Tabelle DC_AufträgeTIA steht in der Spalte "UpdateKriterien"      
      //die Information welche Daten gesucht werden. 
      //OPTIONALE Funktion zu FN02RdRowEvtSPS: Mitloggen (Datenbank) von Daten die von der SPS angefordert wurden
      //Bei Aufträgen mit der Funktionsnummer 2 (FN02RdRowEvtSPS) können die Daten die von der SPS angefordert werden 
      //in eine Datenbanktabelle mitgeloggt werden. Dadurch kann man später nachvollziehen zu welchem Zeitpunkt die SPS 
      //welche Daten angefordert hat. Dazu wird in der Tabelle DC_AufträgeTIA die Spalte LogTableForFN2 für den entsprechenden 
      //Auftrag (Nur FktNr. 2) mit einem Tabellennamen versehen. Diese Tabelle wird beim ersten Start des DatenClieNTs 
      //automatisch angelegt. Und zwar so, daß sie mit der Tabelle oder dem View aus der die Daten geholt werden exakt 
      //übereinstimmt. Datentypen und obligat benötigte Spalten sind dann automatisch vorhanden und die Tabelle ist zu 
      //100% kompatibel. Später können der Tabelle weitere Spalten und vor allem Indizes, Trigger etc. hinzugefügt werden. 
      //Dadurch bleibt die LogTabelle kompatibel. Über die hardcodierte Spalte LOG_DateTime, läßt sich später der Zeitpunkt 
      //des Downloads zur SPS wieder rekonstruieren. Die ebenfalls hardcodierte Spalte LOG_ID, welche die 
      //AutoIndex-Eigenschaft besitzt, stellt den PrimaryKey dar. Es empfiehlt sich den Spalten, nach denen später gefiltert 
      //werden soll, einen Index zu geben, damit die Arbeit des DatenClieNTs nicht unnötig behindert wird.
      //Falls die Funktion in besetehenden Projekten nachträglich eingeführt werden kann muß der DatenClieNT mindestens 
      //in der Version 3.0.2.65 vorliegen. Die evtl. fehlende Spalte in der Tabelle DC_AufträgeTIA kann folgendermaßen nachgerüstet werden.
      //alter table DC_AufträgeTIA add LogTableForFN2 nvarchar(128)
      FN02RdRowEvtSPS = 2,
      
      
      //LESEN wie Funktion 2 nur das hier mehrere Datensätze verarbeitet werden können. Funktion 2 ist auf einen DS beschränkt.
      FN20RdRowEvtSPS = 20,
      
      
   }
   
   
   public enum SpsDatentyp
   {
      TIA = 0,             //Für die TIA-Umstellung

      /*
      DT1_IntKF16Bit = 1,                   //Integer 16 bits (INT, KF)
      DT2_IntDF32Bit = 2,                   //Integer 32 bits (DINT, DF)
      DT3_KH_WertOderString = 3,            //KH (Wert oder String)
      DT4_DH_WertOderString = 4,            //DH (Wert oder String)
      DT5_KM_alsString = 5,                 //KM als String
      DT6_BitMitAngabeBit0_15ausWort = 6,   //Bit mit Angabe Bit 0-15 aus Wort: Siehe auch ANLEITUNG unten
      DT7_SpsFloat = 7,                     //Real S7 / KG S5
      DT8_BYTE = 8,                         //Byte 0-255
      DT9_S7String = 9,                     //String S7 Zei.1+2 wird abgeschnitten (zul.ASCII in Dez. 32,33,35,36,37,40-126)
      DT10_TEXT = 10,                       //Text (PLC_Faktor=Len Bytes)  zul.Ascii wie 9
      DT11_TimeStamp5DW = 11,               //TimeStamp (5 DW  mm/hh/tt/MM/JJJJ))
      DT12_TimeStamp6DW = 12,               //TimeStamp (6 DW  ss/mm/hh/tt/MM/JJJJ))
      DT13_DateTime3DW = 13,                //Datum/Zeit (3 DW-KH JJMMTThhmmss)
      DT14_TimeStampS7 = 14,                //TimeStamp S7 8 byte
      DT15_TimeStampPC = 15,                //TimeStamp PC
      DT16_BitBytebasiert = 16,             //NEU: 19.07.2012 | Nur S7 | Angabe von Blockabgriff und Faktor (Bitposition) so wie Kalle und Norbert es aus der SPS rausholen (Über Excel)
      DT17_CombiTimeStamp,                  //NEU: 28.11.2012 | In der SPS eigentlich zweigeteilt (Datum, Zeit) Hier vereint zu einem Zeitstempel !
      */
   }


   /*
   Anleitung S7: DT6_BitMitAngabeBit0_15ausWort
      
   Die Angabe der Bitpositionen ist immer wortbasiert.
   Zusätzlich sind die beiden Bytes des Wortes im Vergleich zur SPS (Step7) gedreht
   D.h. wenn in der SPS ein Bit auf Adresse 1 und Position 0 liegt (1.0) muß es bei den AuftragsDetails
   mit Adresse 0 und Position 0 angegeben werden !
   Oder mit Adresse 1.8
   Hier einige Beispiele:
   
   SPS      |     DatenClient-Auftrag
   ----------------------------------
   1.0      |     1.8
   24.2     |     24.10
   24.3     |     24.11
   24.0     |     24.8    
   0.0      |     0.8   
   
   
   */

   public static class EnumHelper
   {
      public static string FktNrText(FktNr fktNr)
      {
         switch(fktNr)
         {
            case FktNr.FN00_Keine:         return " 0-Keine                ";
            case FktNr.FN01WrRowInsEvtSPS: return " 1-Insert Row/Anreiz SPS";
            case FktNr.FN05WrColInsEvtSPS: return " 5-Insert Col/Anreiz SPS";
            case FktNr.FN11WrColUpdEvtDC:  return "11-Ins Upd Col/Anreiz DC";
            case FktNr.FN12WrRowInsEvtDC:  return "12-Insert Row/Anreiz DC ";
            case FktNr.FN13WrColInsEvtDC:  return "13-Insert Col/Anreiz DC ";
            case FktNr.FN02RdRowEvtSPS:    return " 2-Read SPS/Anreiz SPS  ";
            case FktNr.FN20RdRowEvtSPS:    return "20-Read SPS/Anreiz SPS  ";
         }
      
         return "";
      }
   }
   
   
}