using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DatenClieNT_AuftragsGenerator
{
   public partial class MainForm : Form
   {
      public static string IropaRegistryPath = "SOFTWARE\\IROPA\\DatenClieNT_AuftragsGenerator";

      private static string KeyTabellenname = "Tabellenname";
      private static string KeyDateTimeSpalte = "DateTimeSpalte";
      private static string KeySubstSpaltennameAlt = "SubstSpaltennameAlt";
      private static string KeySubstSpaltennameNeu = "SubstSpaltennameNeu";
      private static string KeyAutoIndex = "AutoIndex";
      private static string KeyTrennzeichen = "Trennzeichen";
      
      private static string KeyAuftragsID = "AuftragsID";
      private static string KeyAuftragsNummer = "AuftragsNummer";
      private static string KeyDatenClientID = "DatenClientID";
      private static string KeyAuftragsBezeichnung = "AuftragsBezeichnung";
      private static string KeyAuftragsTabelle = "AuftragsTabelle";
      private static string KeyPseudoScript = "PseudoScript";
      private static string KeyFunktionsnummer = "Funktionsnummer";

      private static string KeyBlockTag = "BlockTag";
      private static string KeyUpdateKriterium = "UpdateKriterium";
      private static string KeyDetailPrefix = "DetailPrefix";
      private static string KeyUpdateIntervall = "UpdateIntervall";
      private static string KeyLogForTableFN2 = "LogForTableFN2";
      private static string KeyTagValues = "TagValues";
      private static string KeyExceldaten = "Exceldaten";
      
      private static string defaultTabellenname = "PROD_SPS_";
      private static string defaultDateTimeSpalte = "DC_DateTime";
      private static string defaultSubstSpaltennameAlt = "";
      private static string defaultSubstSpaltennameNeu = "";
      private static string defaultAutoIndex = "1";
      private static string defaultTrennzeichen = string.Format("\t");

      private static string defaultAuftragsID = "101";
      private static string defaultAuftragsNummer = "101";
      private static string defaultDatenClientID = "1";
      private static string defaultAuftragsBezeichnung = "Z.B. Induktorwerte";
      private static string defaultAuftragsTabelle = "Z.B. PROD_SPS_";
      private static string defaultPseudoScript = "DC_DateTime=@DC_LOCAL_DATETIME";
      private static string defaultFunktionsnummer = "1";
      private static string defaultBlockTag = "";
      private static string defaultUpdateKriterium = "";
      private static string defaultDetailPrefix = "ns=1;s=FA_HWS.DataBlocks.";
      private static string defaultUpdateIntervall = "";
      private static string defaultLogForTableFN2 = "";
      private static string defaultTagValues = "";
      private static string defaultExceldaten = "0.0  Spaltenname int   Bezeichnung";

      private string currentFunktionsnummerText = "Bitte wählen sie eine Funktionsnummer aus !";

      private ToolTip toolTip = new ToolTip();
   
      public MainForm()
      {
         InitializeComponent();
         
         toolTip.ToolTipTitle = "INFO";
         toolTip.AutoPopDelay = 32000;
         


         textBoxEingabe.Text = GetSetValue(KeyExceldaten, defaultExceldaten);
         textBoxTabellenname.Text = GetSetValue(KeyTabellenname, defaultTabellenname);
         textBoxDatumZeitSpalte.Text = GetSetValue(KeyDateTimeSpalte, defaultDateTimeSpalte);
         textBoxSubstAlt.Text = GetSetValue(KeySubstSpaltennameAlt, defaultSubstSpaltennameAlt);
         textBoxSubstNeu.Text = GetSetValue(KeySubstSpaltennameNeu, defaultSubstSpaltennameNeu);
         
         string autoIndex = GetSetValue(KeyAutoIndex, defaultAutoIndex);
         if(autoIndex == "1")
         {
            checkBoxAutoPK.Checked = true;
         }
         else
         {
            checkBoxAutoPK.Checked = false;
         }

         textBoxTrennzeichen.Text = GetSetValue(KeyTrennzeichen, defaultTrennzeichen);

         textBoxID.Text = GetSetValue(KeyAuftragsID, defaultAuftragsID);
         textBoxAuftragsnummer.Text = GetSetValue(KeyAuftragsNummer, defaultAuftragsNummer);
         textBoxDatenClientID.Text = GetSetValue(KeyDatenClientID, defaultDatenClientID);

         textBoxBezeichnung.Text = GetSetValue(KeyAuftragsBezeichnung, defaultAuftragsBezeichnung);
         textBoxTabelleViewStatement.Text = GetSetValue(KeyAuftragsTabelle, defaultAuftragsTabelle);
         textBoxPseudoScript.Text = GetSetValue(KeyPseudoScript, defaultPseudoScript);
         comboBoxFunktionsnummer.SelectedItem = GetSetValue(KeyFunktionsnummer, defaultFunktionsnummer);
         
         textBoxBlockTag.Text = GetSetValue(KeyBlockTag, defaultBlockTag);
         textBoxUpdateKriterium.Text = GetSetValue(KeyUpdateKriterium, defaultUpdateKriterium);
         textBoxDetailPrefix.Text = GetSetValue(KeyDetailPrefix, defaultDetailPrefix);
         textBoxUpdateIntervall.Text = GetSetValue(KeyUpdateIntervall, defaultUpdateIntervall);
         textBoxLogTableForFN2.Text = GetSetValue(KeyLogForTableFN2, defaultLogForTableFN2);
         textBoxTagValues.Text = GetSetValue(KeyTagValues, defaultTagValues);
      }

      private void buttonGenerate_Click(object sender, EventArgs e)
      {
         SaveInRegistry();

         if (textBoxTabellenname.Text == "")
         {
            MessageBox.Show("Bitte geben Sie an, wie die Tabelle heißen soll !", "IROPA DatenClieNT_TIA AuftragsGenerator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
         }

         if (!checkBoxAutoPK.Checked)
         {
            MessageBox.Show("Tabelle ohne automatische AutoIndex-Spalte ist noch nicht implementiert !", "IROPA DatenClieNT_TIA AuftragsGenerator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
         }
         
         char trennzeichen = '\t';
         
         if(textBoxTrennzeichen.Text.Length != 1)
         {
            if(textBoxTrennzeichen.Text.ToUpper() != "TAB")
            {
               MessageBox.Show("Bitte geben Sie ein gültiges Trennzeichen ein !", "IROPA DatenClieNT_TIA AuftragsGenerator", MessageBoxButtons.OK, MessageBoxIcon.Error);
               return;
            }
         }

         if (textBoxTrennzeichen.Text.ToUpper() != "TAB")
         {
            trennzeichen = textBoxTrennzeichen.Text[0];
         }
         else
         {
            trennzeichen = defaultTrennzeichen[0];
         }

         StatementTabelle statementTabelle = new StatementTabelle(textBoxEingabe.Text, textBoxTabellenname.Text, trennzeichen, textBoxSubstAlt.Text, textBoxSubstNeu.Text, checkBoxAutoPK.Checked, textBoxDatumZeitSpalte.Text, textBoxID.Text, textBoxDetailPrefix.Text);
         StatementAuftrag statementAuftrag = new StatementAuftrag(textBoxID.Text, textBoxAuftragsnummer.Text, textBoxDatenClientID.Text, textBoxBezeichnung.Text, textBoxTabelleViewStatement.Text, textBoxPseudoScript.Text, (string) comboBoxFunktionsnummer.SelectedItem, textBoxBlockTag.Text, textBoxUpdateKriterium.Text, textBoxUpdateIntervall.Text, textBoxLogTableForFN2.Text, textBoxTagValues.Text, textBoxDetailPrefix.Text);
   
         Clipboard.SetDataObject(string.Format("{0}\n\n\n{1}\n\n\n{2}", statementTabelle.Statement, statementAuftrag.Statement, statementTabelle.StatementAuftragDetails));

         MessageBox.Show("Erfolgreich ! Das Script befindet sich nun in der Zwischenablage !", "IROPA DatenClieNT_TIA AuftragsGenerator", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }

      private void SaveInRegistry()
      {
         SetValue(KeyExceldaten, textBoxEingabe.Text);
         
         
         SetValue(KeyTabellenname, textBoxTabellenname.Text);
         SetValue(KeyDateTimeSpalte, textBoxDatumZeitSpalte.Text);

         SetValue(KeySubstSpaltennameAlt, textBoxSubstAlt.Text);
         SetValue(KeySubstSpaltennameNeu, textBoxSubstNeu.Text);

         if (checkBoxAutoPK.Checked)
         {
            SetValue(KeyAutoIndex, "1");
         }
         else
         {
            SetValue(KeyAutoIndex, "0");
         }

         SetValue(KeyTrennzeichen, textBoxTrennzeichen.Text);

         SetValue(KeyAuftragsID, textBoxID.Text);
         SetValue(KeyAuftragsNummer, textBoxAuftragsnummer.Text);
         SetValue(KeyDatenClientID, textBoxDatenClientID.Text);

         SetValue(KeyAuftragsBezeichnung, textBoxBezeichnung.Text);
         SetValue(KeyAuftragsTabelle, textBoxTabelleViewStatement.Text);
         SetValue(KeyPseudoScript, textBoxPseudoScript.Text);
         SetValue(KeyFunktionsnummer, (string) comboBoxFunktionsnummer.SelectedItem);

         SetValue(KeyBlockTag, textBoxBlockTag.Text);
         SetValue(KeyUpdateKriterium, textBoxUpdateKriterium.Text);
         SetValue(KeyDetailPrefix, textBoxDetailPrefix.Text);
         SetValue(KeyUpdateIntervall, textBoxUpdateIntervall.Text);
         SetValue(KeyLogForTableFN2, textBoxLogTableForFN2.Text);
         SetValue(KeyTagValues, textBoxTagValues.Text);
      }

      private void button1_Click(object sender, EventArgs e)
      {
         string anweisung = "Dieses Programm generiert ein SQL-Statement um eine IROPA-Tabelle, wie sie üblicherweise in Excel definiert vorliegt, über das SQLServer Managementstudio zu erzeugen.";
         anweisung += Environment.NewLine;
         anweisung += "Zeitgleich wird ein Script für den DatenClieNT-Auftrag und die dazugehörigen Detals generiert !";
         anweisung += Environment.NewLine;
         anweisung += "Das Programm geht davon aus, daß als erstes der Abgriff, dann der Spaltenname, dann der Datentyp und dann optional ein Kommentar kommt.";
         anweisung += Environment.NewLine;
         anweisung += "1. Diese Excel-Definitionen komplett in die Zwischenablage kopieren";
         anweisung += Environment.NewLine;
         anweisung += "2. In die große Textbox einfügen";
         anweisung += Environment.NewLine;
         anweisung += "3. Den gewünschten Tabellenname eingeben";
         anweisung += Environment.NewLine;
         anweisung += "4. Optional ein anderes Trennzeichen eingeben (Nicht empfohlen)";
         anweisung += Environment.NewLine;
         anweisung += "5. Optional kann eine Teilsubstitution für den Spaltennamen angegeben werden, wenn z.B. alle Spalten mit 'Bunker.ETW.' anfangen kann dieses so wegsubstituiert werden";
         anweisung += Environment.NewLine;
         anweisung += "6. Auf den grünen Button drücken";
         anweisung += Environment.NewLine;
         anweisung += "7. Im SQLServer Managementstudio eine neue Abfrage aufmachen und dort einfach CTRL-V drücken.";
         anweisung += Environment.NewLine;
         anweisung += "8. Ausführen -> Fertig !";

         MessageBox.Show(anweisung, "IROPA DatenClieNT_TIA AuftragsGenerator", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }

      private void comboBoxFunktionsnummer_SelectedIndexChanged(object sender, EventArgs e)
      {
         UpdateFunktionsnummerBeschreibung();
      }

      private void UpdateFunktionsnummerBeschreibung()
      {
         string funktionsNummer = (string) comboBoxFunktionsnummer.SelectedItem;
      
         switch(funktionsNummer)
         {
            case "1": currentFunktionsnummerText = string.Format("Anreiz kommt von der SPS\nZeilenweises Schreiben in die Datenbank\nBreite Datentabelle, Feldnamen stehen in DC_AuftragsDetailsTIA\nNeue Daten werden mit insert geschrieben. Tabelle wird also immer größer\nACHTUNG: Wenn die Spalte 'UpdateKrit' mit einer DC_AuftragsDetail-ID gefüllt ist wird\nein Update auf die Datentabelle gemacht mit 'where (DatenbankWert von DC_AuftragsDetails == SPSWert)'\nWenn das Update sich auf 0 Sätze auswirkt, wird ein insert gemacht.\nDas UpdateKrit macht natürlich nur Sinn wenn es sich um eine ID (z.B. KastenID o.ä.) handelt.\nSie sollte in der Tabelle eindeutig sein.\nBeispiel: Kastendaten"); break;
            case "5": currentFunktionsnummerText = string.Format("Anreiz kommt von der SPS\nSpaltenweises Schreiben in DB\nSchmale Datentabelle in der die AuftragsDetail_ID mit gespeichert wird zum Wiederfinden\nNeue Daten werden inserted. Tabelle wird also immer größer"); break;
            case "11": currentFunktionsnummerText = string.Format("Anreiz kommt von diesem DatenClient (Polling mit 'DC_AufträgeTIA.UpdateIntervall')\nMIT BLOCKTAG\nSpaltenweises Schreiben in DB\nSchmale Datentabelle in der die AuftragsDetail_ID mit gespeichert wird zum Wiederfinden\nNeue Daten werden upgedatet. Tabelle enthält also immer nur die aktuellen Daten\nBeispiel: OnlineMesswerte"); break;
            case "12": currentFunktionsnummerText = string.Format("Anreiz kommt von diesem DatenClient (Polling mit 'DC_AufträgeTIA.UpdateIntervall')\nMIT BLOCKTAG\nZeilenweises Schreiben in die Datenbank;\nBreite Datentabelle, Feldnamen stehen in DC_AuftragsDetails\nNeue Daten werden mit insert geschrieben; Tabelle wird also immer größer\nBeispiele: Messwerte1, Induktordaten\nWICHTIG: Bei der Addressierung des Blocktags muß man 'KA' dahinter hängen.\nZ.B. 'DB800.S12.74KA'"); break;
            case "13": currentFunktionsnummerText = string.Format("Anreiz kommt von diesem DatenClient (Polling mit 'DC_AufträgeTIA.UpdateIntervall')\nMIT BLOCKTAG\nSpaltenweises Schreiben in DB\nSchmale Datentabelle in der die AuftragsDetail_ID mit gespeichert wird zum Wiederfinden\nNeue Daten werden inserted. Tabelle wird also immer größer\nWICHTIG: Bei der Addressierung des Blocktags muß man 'KA' dahinter hängen.\nZ.B. 'DB800.S12.74KA'"); break;
            case "2": currentFunktionsnummerText = string.Format("LESEN aus Datenbank\nZeilenweises Lesen aus DB aus breiter Datentabelle\nIn der Tabelle DC_AufträgeTIA steht in der Spalte 'UpdateKriterien'\ndie Information welche Daten gesucht werden.\nOPTIONALE Funktion zu FN02RdRowEvtSPS: Mitloggen (Datenbank) von Daten die von der SPS angefordert wurden\nBei Aufträgen mit der Funktionsnummer 2 (FN02RdRowEvtSPS) können die Daten die von der SPS angefordert werden\nin eine Datenbanktabelle mitgeloggt werden. Dadurch kann man später nachvollziehen zu welchem Zeitpunkt die SPS\nwelche Daten angefordert hat. Dazu wird in der Tabelle DC_Aufträge die Spalte LogTableForFN2 für den entsprechenden\nAuftrag (Nur FktNr. 2) mit einem Tabellennamen versehen. Diese Tabelle wird beim ersten Start des DatenClieNTs\nautomatisch angelegt. Und zwar so, daß sie mit der Tabelle oder dem View aus der die Daten geholt werden exakt\nübereinstimmt. Datentypen und obligat benötigte Spalten sind dann automatisch vorhanden und die Tabelle ist zu \n100% kompatibel. Später können der Tabelle weitere Spalten und vor allem Indizes, Trigger etc. hinzugefügt werden. \nDadurch bleibt die LogTabelle kompatibel. Über die hardcodierte Spalte LOG_DateTime, läßt sich später der Zeitpunkt \ndes Downloads zur SPS wieder rekonstruieren. Die ebenfalls hardcodierte Spalte LOG_ID, welche die \nAutoIndex-Eigenschaft besitzt, stellt den PrimaryKey dar. Es empfiehlt sich den Spalten, nach denen später gefiltert \nwerden soll, einen Index zu geben, damit die Arbeit des DatenClieNTs nicht unnötig behindert wird.\nFalls die Funktion in besetehenden Projekten nachträglich eingeführt werden kann muß der DatenClieNT mindestens \nin der Version 3.0.2.65 vorliegen. Die evtl. fehlende Spalte in der Tabelle DC_Aufträge kann folgendermaßen nachgerüstet werden.\nalter table DC_Aufträge add LogTableForFN2 nvarchar(128)"); break;
            case "20": currentFunktionsnummerText = string.Format("LESEN wie 2 nur das beim Finden mehrerer Datensätze alle gefundenen Datensätze zur SPS geschrieben werden. Und zwar direkt hintereinander."); break;
            default: currentFunktionsnummerText = "Bitte wählen sie eine Funktionsnummer aus !"; break;
         }
         
         
         textBoxUpdateIntervall.Enabled = false;
         labelUpdateIntervall.Enabled = false;
         
         textBoxUpdateKriterium.Enabled = false;
         labelUpdateKriterium.Enabled = false;
         
         textBoxLogTableForFN2.Enabled = false;
         labelLogForTableFN2.Enabled = false;
         
         textBoxTagValues.Enabled = false;
         labelTag.Enabled = false;
         
         textBoxBlockTag.Enabled = false;
         labelBlockTag.Enabled = false;

         switch (funktionsNummer)
         {
            case "1":
               textBoxUpdateKriterium.Enabled = true;
               labelUpdateKriterium.Enabled = true;

               textBoxTagValues.Enabled = true;
               labelTag.Enabled = true;
            break;
            case "5":
               textBoxTagValues.Enabled = true;
               labelTag.Enabled = true;
            break;
            case "11":
               textBoxUpdateIntervall.Enabled = true;
               labelUpdateIntervall.Enabled = true;
               textBoxTagValues.Enabled = true;
               labelTag.Enabled = true;
               textBoxBlockTag.Enabled = true;
               labelBlockTag.Enabled = true;
            break;
            case "12":
               textBoxUpdateIntervall.Enabled = true;
               labelUpdateIntervall.Enabled = true;
               textBoxTagValues.Enabled = true;
               labelTag.Enabled = true;
               textBoxBlockTag.Enabled = true;
               labelBlockTag.Enabled = true;
            break;
            case "13":
               textBoxUpdateIntervall.Enabled = true;
               labelUpdateIntervall.Enabled = true;
               textBoxTagValues.Enabled = true;
               labelTag.Enabled = true;
               textBoxBlockTag.Enabled = true;
               labelBlockTag.Enabled = true; 
            break;
            case "2":
            case "20":
               textBoxUpdateKriterium.Enabled = true;
               labelUpdateKriterium.Enabled = true;
               textBoxLogTableForFN2.Enabled = true;
               labelLogForTableFN2.Enabled = true;
            break;
         }
      }

      private static string GetSetValue(string key, string defaultValue)
      {
         RegistryKey regKey = Registry.LocalMachine.OpenSubKey(IropaRegistryPath, false);

         if (regKey == null)
         {
            try
            {
               regKey = Registry.LocalMachine.CreateSubKey(IropaRegistryPath);
               regKey.SetValue(key, defaultValue);
               regKey.Close();
               return defaultValue;
            }
            catch
            {
               return defaultValue;
            }
         }

         string value = (string)regKey.GetValue(key);
         if (value == null)
         {
            value = defaultValue;

            try
            {
               regKey.Close();
               regKey = Registry.LocalMachine.OpenSubKey(IropaRegistryPath, true);
               regKey.SetValue(key, value);
               regKey.Close();
               return value;
            }
            catch
            {
            }
         }

         return value;
      }

      private Boolean SetValue(string key, string value)
      {
         try
         {
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(IropaRegistryPath, true);

            if (regKey == null)
            {
               regKey = Registry.LocalMachine.CreateSubKey(IropaRegistryPath);
            }
            if (regKey == null)
            {
               MessageBox.Show(string.Format("Der Wert {0} konnte nicht in die Windows-Registry eingetragen werden !", key), "IROPA DatenClieNT_TIA AuftragsGenerator", MessageBoxButtons.OK, MessageBoxIcon.Error);
               return false;
            }


            regKey.SetValue(key, value);
            regKey.Close();
         }
         catch
         {
            MessageBox.Show(string.Format("Der Wert {0} konnte nicht in die Windows-Registry eingetragen werden !", key), "IROPA DatenClieNT_TIA AuftragsGenerator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
         }

         return true;
      }

      private void textBoxTabellenname_MouseHover(object sender, EventArgs e)
      {
         toolTip.SetToolTip((Control)sender, "Name der zu erzeugenden Tabelle. Z.B. PROD_SPS_Strahlen oder PROD_ONL_Temperaturen");
         
      }

      private void textBoxDatumZeitSpalte_MouseHover(object sender, EventArgs e)
      {
         toolTip.SetToolTip((Control)sender, "Name der Zeitstempel-Spalte in der Tabelle. Es wird automatisch ein Index auf diese Spalte gelegt !");
      }

      private void comboBoxFunktionsnummer_MouseHover(object sender, EventArgs e)
      {
         toolTip.SetToolTip((Control)sender, currentFunktionsnummerText);
      }

      private void textBoxID_MouseHover(object sender, EventArgs e)
      {
         toolTip.SetToolTip((Control)sender, "ID des Auftrages. Dieser wird für die Verknüpfung mit der Tabelle DC_AuftragDetailsTIA benötigt.\nDiese Zahl muß eindeutig sein, im Gegensatz zur Auftragsnummer !");
      }

      private void textBoxAuftragsnummer_MouseHover(object sender, EventArgs e)
      {
         toolTip.SetToolTip((Control)sender, "Nummer des Auftrages. Diese Zahl wird bei SPS angereizten Aufträgen (Funktionsnummer 1, 5, 2) von der SPS mitgeschickt und muß daher kompatibel mit dem SPS-Auftrag sein.\nFür die vom DatenClieNT angereizten Aufträge spielt die Nummer nur eine untergeordnete Rolle.");
      }

      private void textBoxDatenClientID_MouseHover(object sender, EventArgs e)
      {
         toolTip.SetToolTip((Control)sender, "ID des DatenClieNT-Strangs welcher für die gewünschte SPS zuständig ist.\nSiehe Tabelle DC_DatenClients");
      }

      private void textBoxBezeichnung_MouseHover(object sender, EventArgs e)
      {
         toolTip.SetToolTip((Control)sender, "Bezeichnen sie den Auftrag um später eine Vorstellung davon zu haben, wofür er angelegt wurde !");
      }

      private void textBoxTabelleViewStatement_MouseHover(object sender, EventArgs e)
      {
         toolTip.SetToolTip((Control)sender, "Geben sie hier eine Tabelle, ein View oder ein select-Statement an um die Datenquelle festzulegen.\nEin loses Statement MUSS in normale Klammern eingebettet werden !\nDie verknüpften Datensätze in der Tabelle DC_AuftragsDetailsTIA beziehen sich auf diesen Eintrag !");
      }

      private void textBoxPseudoScript_MouseHover(object sender, EventArgs e)
      {
         toolTip.SetToolTip((Control)sender, "Hier besteht die Möglichkeit, Spalten in der Zieltabelle zu setzen, ohne das die SPS dafür Daten liefern muß.\nStattdessen können interne Variablen des DatenClieNTs eingetragen werden.\nZur Zeit gibt es folgende Variablen:\n@DC_UTC_HIRES -> Hochauflösender UTC Zeitstempel (100ns seit 01.01.1601); Spaltentyp muß bigint sein\n@DC_UTC_DATETIME -> Weniger hochauflösender UTC Zeitstempel (3..4 ms); Dafür kann der Spaltentyp DateTime sein\n@DC_LOCAL_DATETIME -> Weniger hochauflösender lokaler Zeitstempel (3..4 ms); Dafür kann der Spaltentyp DateTime sein\n@ANLAGEN_ID -> Liefert die AnlagenID des aktuellen Kontextes des DatenClients; Spaltentyp: bigint\n@AUFTRAGS_NR -> Liefert die Auftragsnummer des aktuellen Kontextes des DatenClients; Spaltentyp: bigint\n@TAG -> Spezialvariable, die den bei diesem Auftrag konfigurierbaren Tag zurückliefert; Spaltentyp: sql_variant\nBeispiel für die Verwendung von Pseudovariablen:\nDC_DateTime=@DC_LOCAL_DATETIME\nEs können auch mehrere Variablen durch Semikolon getrennt verwendet werden:\nDC_DateTime=@DC_LOCAL_DATETIME; AnlagenID=@ANLAGEN_ID");
      }

      private void textBoxBlockTag_MouseHover(object sender, EventArgs e)
      {
         toolTip.SetToolTip((Control)sender, "Für die Funktionsnummern 11, 12 und 13 muß ein BlockTag angegeben werden.\nDie Daten werden dann durch einen Direktzugriff aus dem entsprechenden Datenbaustein zyklisch geholt.\nDie Zykluszeit wird in ms unter UpdateIntervall angegeben !\nBeispiel: DB800S298.34KA");
      }

      private void textBoxUpdateIntervall_MouseHover(object sender, EventArgs e)
      {
         toolTip.SetToolTip((Control)sender, "Für die Funktionsnummern 11, 12 und 13 muß ein UpdateIntervall in Millisekunden angegeben werden.\nDie Daten werden dann durch einen Direktzugriff aus dem entsprechenden Datenbaustein (BlockTag) zyklisch geholt.\n");
      }

      private void textBoxUpdateKriterium_MouseHover(object sender, EventArgs e)
      {
         toolTip.SetToolTip((Control)sender, "Für die Funktionsnummern 1 und 2 ist das UpdateKriterium relevant.\nEs handelt sich hier im eine ID aus der Tabelle DC_AuftragsDetailsTIA.\nFunktionsnummer 1:\nWird die ID in den Daten gefunden wird ein update auf die entsprechende Zeile gemacht. Ansonsten ein insert.\nFunktionsnummer 2:\nDas UpdateKriterium fungiert hier als Suchkriterium wenn die SPS Daten anfordert.");
      }

      private void textBoxLogTableForFN2_MouseHover(object sender, EventArgs e)
      {
         toolTip.SetToolTip((Control)sender, "OPTIONALE Funktion zu FN02RdRowEvtSPS und FN20RdRowEvtSPS: Mitloggen (Datenbank) von Daten die von der SPS angefordert wurden\nBei Aufträgen mit der Funktionsnummer 2 (FN02RdRowEvtSPS) können die Daten die von der SPS angefordert werden\nin eine Datenbanktabelle mitgeloggt werden. Dadurch kann man später nachvollziehen zu welchem Zeitpunkt die SPS\nwelche Daten angefordert hat. Dazu wird in der Tabelle DC_AufträgeTIA die Spalte LogTableForFN2 für den entsprechenden\nAuftrag (Nur FktNr. 2) mit einem Tabellennamen versehen. Diese Tabelle wird beim ersten Start des DatenClieNTs\nautomatisch angelegt. Und zwar so, daß sie mit der Tabelle oder dem View aus der die Daten geholt werden exakt\nübereinstimmt. Datentypen und obligat benötigte Spalten sind dann automatisch vorhanden und die Tabelle ist zu \n100% kompatibel. Später können der Tabelle weitere Spalten und vor allem Indizes, Trigger etc. hinzugefügt werden. \nDadurch bleibt die LogTabelle kompatibel. Über die hardcodierte Spalte LOG_DateTime, läßt sich später der Zeitpunkt \ndes Downloads zur SPS wieder rekonstruieren. Die ebenfalls hardcodierte Spalte LOG_ID, welche die \nAutoIndex-Eigenschaft besitzt, stellt den PrimaryKey dar. Es empfiehlt sich den Spalten, nach denen später gefiltert \nwerden soll, einen Index zu geben, damit die Arbeit des DatenClieNTs nicht unnötig behindert wird.\nFalls die Funktion in besetehenden Projekten nachträglich eingeführt werden kann muß der DatenClieNT mindestens \nin der Version 3.0.2.65 vorliegen. Die evtl. fehlende Spalte in der Tabelle DC_Aufträge kann folgendermaßen nachgerüstet werden.\nalter table DC_Aufträge add LogTableForFN2 nvarchar(128)");
      }

      private void textBoxTagValues_MouseHover(object sender, EventArgs e)
      {
         toolTip.SetToolTip((Control)sender, "Dieser Tag 'kommt' mit jedem Auftrag und kann einer Spalte in der Zieltabelle über die Pseudovariable @TAG wieder zugeordnet werden.\nDies ist eine elegante Methode um später zu wissen, welcher Auftrag einen entsprechenden Datensatz geschrieben hat, denn es können mehrere Aufträge in die gleiche Tabelle schreiben !");
      }

      private void textBox1_TextChanged(object sender, EventArgs e)
      {

      }

      private void labelUpdateKriterium_Click(object sender, EventArgs e)
      {

      }
   }
}


      
      

      
      