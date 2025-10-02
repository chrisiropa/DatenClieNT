using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;

namespace DatenClieNT_CM
{
   public partial class FormDcNetConfigManager : Form
   {
      private string iropaRegistryPath = "SOFTWARE\\IROPA\\DatenClieNT_TIA";
      //Bei 64 Bit Bedenke WOW6432Node in der Registry unter Localmachine\Software

      private string KeyServer = "Server";
      private string KeyDatabase = "Database";
      private string KeyUserID = "UserID";
      private string KeyPassword = "Password";
      private string KeyLanguage = "Language";
      private string KeyConnectTimeout = "ConnectTimeout";
      public  string KeyLogfilePath = "LogfilePath";
      public string KeyLastConfigFilePath = "LastConfigFilePath";

      private string defaultDatabase = "Z.B. FA3524";
      private string defaultServer = "DEV1";
      private string defaultUserID = "sa";
      private string defaultPassword = "IR_apori72";
      private string defaultLanguage = "German";
      private string defaultConnectTimeout = "10";
      private string defaultLastConfigFilePath = "c:\\Programme\\IROPA\\DCConfig1.dcmTIA";
      
      private string lastConfigFilePath = "";
   
      public FormDcNetConfigManager()
      {
         InitializeComponent();
         
         this.CancelButton = buttonCancel;
         
         ReadRegistrySettings();
      }

      private void ReadRegistrySettings()
      {
         string defaultLogfilePath = @"c:\Programme\Iropa\Log";
         
      
         textBoxLogFileFolder.Text = GetSetValue(KeyLogfilePath, defaultLogfilePath);
         comboBoxServer.Text = GetSetValue(KeyServer, defaultServer);
         comboBoxDatabase.Text = GetSetValue(KeyDatabase, defaultDatabase);
         textBoxUserID.Text = GetSetValue(KeyUserID, defaultUserID);
         textBoxPassword.Text = GetSetValue(KeyPassword, defaultPassword);
         lastConfigFilePath = GetSetValue(KeyLastConfigFilePath, defaultLastConfigFilePath);
         
         string language = GetSetValue(KeyLanguage, defaultLanguage);
         
         if(comboBoxLanguage.Items.Contains(language))
         {
            comboBoxLanguage.Text = language;
         }
         else
         {
            comboBoxLanguage.Text = defaultLanguage;
         }
         
         textBoxConnectTimeout.Text = GetSetValue(KeyConnectTimeout, defaultConnectTimeout);         
      }
      
      private Boolean WriteRegistrySettings()
      {
         if(!Directory.Exists(textBoxLogFileFolder.Text))
         {
            MessageBox.Show(string.Format("Das Verzeichnis für die Log-Dateien existiert nicht !\n{0}", textBoxLogFileFolder.Text), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            textBoxLogFileFolder.Focus();
            return false;
         }
         
         SetValue(KeyServer, comboBoxServer.Text);
         SetValue(KeyDatabase, comboBoxDatabase.Text);
         SetValue(KeyUserID, textBoxUserID.Text);
         SetValue(KeyPassword, textBoxPassword.Text);
         SetValue(KeyLanguage, comboBoxLanguage.Text);
         SetValue(KeyConnectTimeout, textBoxConnectTimeout.Text);
         SetValue(KeyLogfilePath, textBoxLogFileFolder.Text);
         SetValue(KeyLastConfigFilePath, lastConfigFilePath);
         
         return true;
         
      }

      private Boolean SetValue(string key, string value)
      {
         try
         {
            RegistryKey iropaDatenclientKey = Registry.LocalMachine.OpenSubKey(iropaRegistryPath, true);

            if (iropaDatenclientKey == null)
            {
               iropaDatenclientKey = Registry.LocalMachine.CreateSubKey(iropaRegistryPath);
            }
            if (iropaDatenclientKey == null)
            {
               MessageBox.Show(string.Format("Der Wert {0} konnte nicht in die Windows-Registry eingetragen werden !", key), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
               return false;
            }


            iropaDatenclientKey.SetValue(key, value);
            iropaDatenclientKey.Close();
         }
         catch
         {
            MessageBox.Show(string.Format("Der Wert {0} konnte nicht in die Windows-Registry eingetragen werden !", key), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
         }
            
         return true;
      }

      private string GetSetValue(string key, string defaultValue)
      {
      
         try
         {         
            RegistryKey iropaDatenclientKey = Registry.LocalMachine.OpenSubKey(iropaRegistryPath, true);

            if (iropaDatenclientKey == null)
            {
               iropaDatenclientKey = Registry.LocalMachine.CreateSubKey(iropaRegistryPath);
            }
            if (iropaDatenclientKey == null)
            {
               return null;
            }


            string value = (string)iropaDatenclientKey.GetValue(key);
            if (value == null)
            {
               value = defaultValue;
               iropaDatenclientKey.SetValue(key, value);
            }

            iropaDatenclientKey.Close();

            return value;
         }
         catch(Exception)
         {
            MessageBox.Show("Kein Zugriff auf die Registry !\nBitte dieses Programm als Administrator ausführen !\nProgramm wird beendet !", "IROPA DatenClieNT_TIA Config Manager !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            System.Environment.Exit(0);
            return defaultValue;
         }
      }

      private void buttonCheckConnection_Click(object sender, EventArgs e)
      {
         if (comboBoxServer.Text.Length < 1)
         {
            MessageBox.Show("Bitte geben Sie einen Servernamen an !", "Fehler !", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            comboBoxServer.Focus();
            return;
         }

         if (textBoxUserID.Text.Length < 1)
         {
            MessageBox.Show("Bitte geben Sie einen Benutzer an !", "Fehler !", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            textBoxUserID.Focus();
            return;
         }
         
         
         Cursor = Cursors.WaitCursor;
         
         SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();

         csb.DataSource = comboBoxServer.Text;
         csb.InitialCatalog = comboBoxDatabase.Text;
         try
         {
            csb.ConnectTimeout = Convert.ToInt32(textBoxConnectTimeout.Text);
         }
         catch
         {
            csb.ConnectTimeout = 30;
         }
         csb.CurrentLanguage = comboBoxLanguage.Text;
         //csb.PersistSecurityInfo = true;
         csb.UserID = textBoxUserID.Text;
         csb.Password = textBoxPassword.Text;
         csb.ApplicationName = "Iropa DatenClieNT_TIA Config Manager";

         string connectionString = csb.ConnectionString;
         
         SqlConnection sqlConnection = null;

         try
         {
            sqlConnection = new SqlConnection(connectionString);

            sqlConnection.Open();

            MessageBox.Show("Datenbankverbindung erfolgreich !", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
         }
         catch (Exception)
         {
            MessageBox.Show("Datenbankverbindung fehlgeschlagen !", "Verbindungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
         }
         finally
         {
            sqlConnection.Close();
         }

         Cursor = Cursors.Arrow;
      }

      private void buttonWriteRegistry_Click(object sender, EventArgs e)
      {
         if(WriteRegistrySettings())
         {         
            Close();
         }
      }

      private void buttonCancel_Click(object sender, EventArgs e)
      {
         Close();
      }

      private void comboBoxServer_DropDown(object sender, EventArgs e)
      {
         Cursor = Cursors.WaitCursor;
         
         SqlDataSourceEnumerator sqlDataSourceEnumerator = SqlDataSourceEnumerator.Instance;

         DataTable dataTable = sqlDataSourceEnumerator.GetDataSources();
         
         comboBoxServer.Items.Clear();
         
         foreach(DataRow row in dataTable.Rows)
         {
            string server = "";
            string instance = "";

            try
            {
               server = (string) row["ServerName"];
               
               if(row["InstanceName"] != System.DBNull.Value)
               {
                  instance = (string) row["InstanceName"];
               }
            }
            catch
            {
               continue;
            }
            
            
            
            string sqlServer = server;
            
            if(instance != null && instance.Length > 0)
            {
               sqlServer += string.Format("\\{0}", instance);
            }
            
            comboBoxServer.Items.Add(sqlServer);
         }

         Cursor = Cursors.Arrow;
      }

      private void comboBoxDatabase_DropDown(object sender, EventArgs e)
      {
         DatenbankenAuflisten(comboBoxServer.Text, textBoxUserID.Text, textBoxPassword.Text);
      }

      private void DatenbankenAuflisten(string serverName, string userID, string password)
      {
         if(serverName.Length < 1)
         {
            MessageBox.Show("Bitte geben Sie einen Servernamen an !", "Fehler !", MessageBoxButtons.OK, MessageBoxIcon.Warning);            
            comboBoxServer.Focus();
            return;
         }

         if (userID.Length < 1)
         {
            MessageBox.Show("Bitte geben Sie einen Benutzer an !", "Fehler !", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            textBoxUserID.Focus();
            return;
         }
      
         SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();

         csb.DataSource = serverName;
         csb.InitialCatalog = "master";
         csb.ConnectTimeout = 5;
         csb.CurrentLanguage = "German";
         csb.UserID = userID;
         csb.Password = password;
         csb.ApplicationName = "DatenClieNT_TIA Config Manager";
         
         comboBoxDatabase.Items.Clear();
         
         try
         {
            SqlSimpleQuery query = new SqlSimpleQuery(csb.ConnectionString, "select name from sys.databases");
            if (query.QueryResult != null)
            {
               foreach (Dictionary<string, object> kanal in query.QueryResult)
               {
                  string name = "";
                  try
                  {
                     name = (string)kanal["name"];
                     
                     comboBoxDatabase.Items.Add(name);
                  }
                  catch
                  {
                  }
               }
            }
         }
         catch
         {
            MessageBox.Show("Datenbankverbindung fehlgeschlagen !", "Verbindungsfehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);            
         }
      }

      private void button1_Click(object sender, EventArgs e)
      {
         if(folderBrowserDialogLogFile.ShowDialog() == DialogResult.OK)
         {
            textBoxLogFileFolder.Text = folderBrowserDialogLogFile.SelectedPath;
         }
      }

      private void comboBoxServer_SelectedIndexChanged(object sender, EventArgs e)
      {

      }

      private void buttonSaveConfig_Click(object sender, EventArgs e)
      {
         saveFileDialog.Title = "DatenClieNT_TIA Konfiguration speichern";
         saveFileDialog.Filter = "DatenClieNT_TIA Konfiguration (*.dcmTIA)|*.dcmTIA| Alle Dateien (*.*)|*.*";
         saveFileDialog.InitialDirectory = Path.GetDirectoryName(lastConfigFilePath);

         if (saveFileDialog.ShowDialog() == DialogResult.OK)
         {
            SetValue(KeyLastConfigFilePath, saveFileDialog.FileName);
            
            SaveConfig(saveFileDialog.FileName);
         }
      }

      private void SaveConfig(string fileName)
      {
         string entry = "";
         
         entry += comboBoxServer.Text;
         entry += Environment.NewLine;
         entry += textBoxUserID.Text;
         entry += Environment.NewLine;
         entry += textBoxPassword.Text;
         entry += Environment.NewLine;
         entry += comboBoxDatabase.Text;
         entry += Environment.NewLine;
         entry += textBoxConnectTimeout.Text;
         entry += Environment.NewLine;
         entry += comboBoxLanguage.Text;
         entry += Environment.NewLine;
         entry += textBoxLogFileFolder.Text;
         entry += Environment.NewLine;
         
         try
         {
            File.WriteAllText(fileName, entry);
         }
         catch(Exception e)
         {
            MessageBox.Show(string.Format("Fehler beim Speichern der Konfiguration -> {0} !", e.Message), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      private void ReadConfig(string fileName)
      {
         string [] lines = File.ReadAllLines(fileName);
         
         if(lines.Length != 8)
         {
            MessageBox.Show("Ungültige DatenClieNT_TIA-Konfigurationsdatei !", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
         }

         comboBoxServer.Text = lines[0];
         textBoxUserID.Text = lines[1];
         textBoxPassword.Text = lines[2];
         comboBoxDatabase.Text = lines[3];
         textBoxConnectTimeout.Text = lines[4];
         comboBoxLanguage.Text = lines[5];
         textBoxLogFileFolder.Text = lines[6];
      }

      private void buttonLoad_Click(object sender, EventArgs e)
      {
         lastConfigFilePath = GetSetValue(KeyLastConfigFilePath, defaultLastConfigFilePath);

         openFileDialog.Title = "DatenClieNT_TIA Konfiguration laden";
         openFileDialog.InitialDirectory = Path.GetDirectoryName(lastConfigFilePath);
         openFileDialog.FileName = Path.GetFileName(lastConfigFilePath);
         openFileDialog.Filter = "DatenClieNT_TIA Konfiguration (*.dcmTIA)|*.dcmTIA| Alle Dateien (*.*)|*.*";
         
         if(openFileDialog.ShowDialog() == DialogResult.OK)
         {
            ReadConfig(openFileDialog.FileName);
         }         
      }
   }
}
