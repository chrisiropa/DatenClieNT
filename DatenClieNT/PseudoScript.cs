using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace DatenClieNT
{
   public class PseudoScript
   {
      /*
      ScriptPseudovariablen
      --------------------------------------------------------------------------------------------
      ScriptPseudovariablen werden beim anlegen von Aufträgen in das SqlStatement
      mit eingebaut. Siehe auch Tabelle DC_AufträgeTIA Spalte PseudoScript.
      Das Miniscript welches dort hinterlegt werden kann hat folgenden Aufbau:
      Tabellenspalte1 = @Pseudovariable1; Tabellenspalte2 = @Pseudovariable2; usw.
      Beim Anlagen des Auftrags wird überprüft ob die in dem Script angegebenen 
      Tabellenspalten in der Auftragszieltabelle existieren. Falls nicht gilt der 
      Auftrag als ungültig und wird nicht ausgeführt.
      Bei DatenEintragen wird dann die entsprechende Tabellenspalte mit ihrer Pseudovariablen
      gefüllt.
      Beispiel: Man will die AnlagenID in einer Datentabelle mit drin haben; 
                Außerdem will mann einen hochgenauen Zeitstempel mit drin haben:
      1. Dafür sorgen das die Ziel/Datentabelle Felder besitzt, welches die AnlagenID und den  
         Zeitstempel aufnehmen kann.
         Z.B. Spalte 
          AnlagenID              bigint
          HiResolutionTimestamp  bigint
      2. Folgendes Script in der PseudoScript-Spalte von DC_AufträgeTIA hinterlegen:
         AnlagenID = @ANLAGEN_ID; HiResolutionTimestamp = @DC_UTC_HIRES
      3. Der Datenclient "besorgt" beim Schreiben der Daten die Werte der jeweiligen 
         Pseudovariablen. Dazu verwendet er den Kontext in dem er sich gerade befindet.
         D.h. z.B. die @ANLAGEN_ID liefert immer die Anlagen_ID des aktuellen Kontextes.
         
      ACHTUNG: Wenn man hinterher auf die Spalten filtern will, sollten alle Spalten,
               die durch Pseudovariablen gefüllt werden einen INDEX haben !!!!!!!!!!
      */



      public static string [] ValidPseudovars = 
      {
         //TabellenspaltenTyp: bigint
         //Liefert die aktuelle PC-Zeit (UTC) als fortlaufende Zahl
         //mit StartDatum 01.01.1601 in 100ns (Nanosekunden) Auflösung
         //Eine Tabellenspalte muß dafür 8 Byte (bigint) als Datentyp haben
         "@DC_UTC_HIRES", 
         
         //TabellenspaltenTyp: DateTime
         //Liefert die aktuelle PC-Zeit (UTC) im SqlServer DateTime-Format
         //Die Auflösung beträgt allerdings nur 3 bis 4 Millisekunden
         "@DC_UTC_DATETIME", 
         
         //TabellenspaltenTyp: DateTime
         //Liefert die aktuelle PC-Zeit (LOCAL) im SqlServer DateTime-Format
         //Die Auflösung beträgt allerdings nur 3 bis 4 Millisekunden
         "@DC_LOCAL_DATETIME", 
         
         //TabellenspaltenTyp: bigint
         //Liefert die AnlagenID des aktuellen Kontextes des DatenClients
         "@ANLAGEN_ID",
         
         //TabellenspaltenTyp: bigint
         //Liefert die AUFTRAGS_NR des empfangenen Auftrags des DatenClients
         "@AUFTRAGS_NR",
         
         //TabellenspaltenTyp: sql_variant
         //Intern wird die Variable sowieso als object durchgeschliffen,
         //bis sie als object-Parameter dem Command-Objekt übergeben wird.
         //Liefert den Tag des empfangenen Auftrags des DatenClients 
         //Siehe Tag-Spalte in der Tabelle DC_AufträgeTIA seit 23.04.2012
         //So kann ein tag in die Zieltabelle des Auftrags mitgschrieben werden um
         //später den Auftrag identifizieren zu können, der für das Schreiben 
         //verantwortlich war (insert). Dazu würde natürlich auch schon die
         //@AUFTRAGS_NR in Kombination mit der @ANLAGEN_ID ausreichen. Allerdings
         //kann man so eleganter auf Zusatzinformationen in einer weiteren Tabelle
         //verlinken, ohne das dort die Auftragsnummer bekannt sein müßte. 
         "@TAG"

      };
      
         
      private Dictionary<string, string> variablenListe = new Dictionary<string,string>();
   
      private string script;    
      
      public Dictionary<string, string> VariablenListe  
      {
         get { return variablenListe; }
      }
   
      public PseudoScript(string script)
      {
         this.script = script;
      }

      public bool Parse()
      {
         //Beispiel:
         //script = "AnlagenID = @ANLAGEN_ID; HiResolutionTimestamp = @DC_UTC_HIRES"
         
         script = script.Trim(';');
         
         if(script == "")
         {
            return true;
         }
         
         string [] kombis = script.Split(';');

         //kombi1 = "AnlagenID = @ANLAGEN_ID"
         //kombi2 = " HiResolutionTimestamp = @DC_UTC_HIRES"
         
         foreach(string kombi in kombis)
         {
            string [] tabelleVariable = kombi.Split('=');
            
            if(tabelleVariable.Length != 2)
            {
               LogManager.GetSingleton().ZLog("C0239", ELF.ERROR, "Pseudoscript={0} -> Kombination <{1}> ungültig", script, kombi);
               return false;
            }
            
            string tabellenSpalte = tabelleVariable[0].Trim();
            string pseudoVariable = tabelleVariable[1].Trim().ToUpper();
            
            bool valid = false;
            foreach(string var in ValidPseudovars)
            {
               if(var == pseudoVariable)
               {
                  valid = true;
                  break;
               }
            }
            if(!valid)
            {
               //Bisherige gültige Paare löschen !
               variablenListe = new Dictionary<string,string>();

               LogManager.GetSingleton().ZLog("C023A", ELF.ERROR, "Pseudoscript={0} -> Ungültige Pseudovariable = {1}", script, pseudoVariable);
               
               return false;
            }       
            
            if(variablenListe.ContainsKey(tabellenSpalte))
            {
               variablenListe = new Dictionary<string, string>();

               LogManager.GetSingleton().ZLog("C023B", ELF.ERROR, "Pseudoscript={0} -> TabellenSpalte kann nicht mehrfach verwendet werden -> {1}", script, tabellenSpalte);
               
               return false;
            }
            
            variablenListe[tabellenSpalte] = pseudoVariable;     
         }
         
         return true;
      }
   }
}
