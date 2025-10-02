namespace DatenClieNT_CM
{
   partial class FormDcNetConfigManager
   {
      /// <summary>
      /// Erforderliche Designervariable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Verwendete Ressourcen bereinigen.
      /// </summary>
      /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      //#region Vom Windows Form-Designer generierter Code

      /// <summary>
      /// Erforderliche Methode für die Designerunterstützung.
      /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
      /// </summary>
      private void InitializeComponent()
      {
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDcNetConfigManager));
         this.label1 = new System.Windows.Forms.Label();
         this.label2 = new System.Windows.Forms.Label();
         this.buttonCheckConnection = new System.Windows.Forms.Button();
         this.textBoxPassword = new System.Windows.Forms.TextBox();
         this.label3 = new System.Windows.Forms.Label();
         this.label4 = new System.Windows.Forms.Label();
         this.textBoxUserID = new System.Windows.Forms.TextBox();
         this.label5 = new System.Windows.Forms.Label();
         this.textBoxConnectTimeout = new System.Windows.Forms.TextBox();
         this.label7 = new System.Windows.Forms.Label();
         this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
         this.buttonWriteRegistry = new System.Windows.Forms.Button();
         this.buttonCancel = new System.Windows.Forms.Button();
         this.comboBoxServer = new System.Windows.Forms.ComboBox();
         this.comboBoxDatabase = new System.Windows.Forms.ComboBox();
         this.groupBoxDatabase = new System.Windows.Forms.GroupBox();
         this.groupBoxDatenClient = new System.Windows.Forms.GroupBox();
         this.button1 = new System.Windows.Forms.Button();
         this.textBoxLogFileFolder = new System.Windows.Forms.TextBox();
         this.label8 = new System.Windows.Forms.Label();
         this.folderBrowserDialogLogFile = new System.Windows.Forms.FolderBrowserDialog();
         this.buttonSaveConfig = new System.Windows.Forms.Button();
         this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
         this.buttonLoad = new System.Windows.Forms.Button();
         this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
         this.groupBoxDatabase.SuspendLayout();
         this.groupBoxDatenClient.SuspendLayout();
         this.SuspendLayout();
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.ForeColor = System.Drawing.Color.DimGray;
         this.label1.Location = new System.Drawing.Point(31, 42);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(41, 13);
         this.label1.TabIndex = 1;
         this.label1.Text = "Server:";
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.ForeColor = System.Drawing.Color.DimGray;
         this.label2.Location = new System.Drawing.Point(30, 121);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(63, 13);
         this.label2.TabIndex = 2;
         this.label2.Text = "Datenbank:";
         // 
         // buttonCheckConnection
         // 
         this.buttonCheckConnection.BackColor = System.Drawing.Color.White;
         this.buttonCheckConnection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
         this.buttonCheckConnection.Location = new System.Drawing.Point(334, 39);
         this.buttonCheckConnection.Name = "buttonCheckConnection";
         this.buttonCheckConnection.Size = new System.Drawing.Size(115, 23);
         this.buttonCheckConnection.TabIndex = 3;
         this.buttonCheckConnection.Text = "&Verbindung testen";
         this.buttonCheckConnection.UseVisualStyleBackColor = false;
         this.buttonCheckConnection.Click += new System.EventHandler(this.buttonCheckConnection_Click);
         // 
         // textBoxPassword
         // 
         this.textBoxPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
         this.textBoxPassword.Location = new System.Drawing.Point(98, 92);
         this.textBoxPassword.Name = "textBoxPassword";
         this.textBoxPassword.PasswordChar = '*';
         this.textBoxPassword.Size = new System.Drawing.Size(200, 20);
         this.textBoxPassword.TabIndex = 9;
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.ForeColor = System.Drawing.Color.DimGray;
         this.label3.Location = new System.Drawing.Point(30, 95);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(53, 13);
         this.label3.TabIndex = 8;
         this.label3.Text = "Passwort:";
         // 
         // label4
         // 
         this.label4.AutoSize = true;
         this.label4.ForeColor = System.Drawing.Color.DimGray;
         this.label4.Location = new System.Drawing.Point(31, 69);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(52, 13);
         this.label4.TabIndex = 7;
         this.label4.Text = "Benutzer:";
         // 
         // textBoxUserID
         // 
         this.textBoxUserID.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
         this.textBoxUserID.Location = new System.Drawing.Point(98, 66);
         this.textBoxUserID.Name = "textBoxUserID";
         this.textBoxUserID.Size = new System.Drawing.Size(200, 20);
         this.textBoxUserID.TabIndex = 6;
         // 
         // label5
         // 
         this.label5.AutoSize = true;
         this.label5.ForeColor = System.Drawing.Color.DimGray;
         this.label5.Location = new System.Drawing.Point(30, 148);
         this.label5.Name = "label5";
         this.label5.Size = new System.Drawing.Size(62, 13);
         this.label5.TabIndex = 11;
         this.label5.Text = "Timeout [s]:";
         // 
         // textBoxConnectTimeout
         // 
         this.textBoxConnectTimeout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
         this.textBoxConnectTimeout.Location = new System.Drawing.Point(98, 146);
         this.textBoxConnectTimeout.Name = "textBoxConnectTimeout";
         this.textBoxConnectTimeout.Size = new System.Drawing.Size(32, 20);
         this.textBoxConnectTimeout.TabIndex = 10;
         // 
         // label7
         // 
         this.label7.AutoSize = true;
         this.label7.ForeColor = System.Drawing.Color.DimGray;
         this.label7.Location = new System.Drawing.Point(142, 149);
         this.label7.Name = "label7";
         this.label7.Size = new System.Drawing.Size(50, 13);
         this.label7.TabIndex = 15;
         this.label7.Text = "Sprache:";
         // 
         // comboBoxLanguage
         // 
         this.comboBoxLanguage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
         this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.comboBoxLanguage.FormattingEnabled = true;
         this.comboBoxLanguage.Items.AddRange(new object[] {
            "English",
            "German"});
         this.comboBoxLanguage.Location = new System.Drawing.Point(198, 145);
         this.comboBoxLanguage.Name = "comboBoxLanguage";
         this.comboBoxLanguage.Size = new System.Drawing.Size(100, 21);
         this.comboBoxLanguage.TabIndex = 16;
         // 
         // buttonWriteRegistry
         // 
         this.buttonWriteRegistry.BackColor = System.Drawing.Color.White;
         this.buttonWriteRegistry.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.buttonWriteRegistry.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
         this.buttonWriteRegistry.Location = new System.Drawing.Point(341, 378);
         this.buttonWriteRegistry.Name = "buttonWriteRegistry";
         this.buttonWriteRegistry.Size = new System.Drawing.Size(75, 23);
         this.buttonWriteRegistry.TabIndex = 17;
         this.buttonWriteRegistry.Text = "&Ok";
         this.buttonWriteRegistry.UseVisualStyleBackColor = false;
         this.buttonWriteRegistry.Click += new System.EventHandler(this.buttonWriteRegistry_Click);
         // 
         // buttonCancel
         // 
         this.buttonCancel.BackColor = System.Drawing.Color.White;
         this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.buttonCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
         this.buttonCancel.Location = new System.Drawing.Point(422, 378);
         this.buttonCancel.Name = "buttonCancel";
         this.buttonCancel.Size = new System.Drawing.Size(75, 23);
         this.buttonCancel.TabIndex = 18;
         this.buttonCancel.Text = "&Abbrechen";
         this.buttonCancel.UseVisualStyleBackColor = false;
         this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
         // 
         // comboBoxServer
         // 
         this.comboBoxServer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
         this.comboBoxServer.DropDownHeight = 200;
         this.comboBoxServer.FormattingEnabled = true;
         this.comboBoxServer.IntegralHeight = false;
         this.comboBoxServer.Location = new System.Drawing.Point(98, 39);
         this.comboBoxServer.Name = "comboBoxServer";
         this.comboBoxServer.Size = new System.Drawing.Size(200, 21);
         this.comboBoxServer.TabIndex = 21;
         this.comboBoxServer.SelectedIndexChanged += new System.EventHandler(this.comboBoxServer_SelectedIndexChanged);
         this.comboBoxServer.DropDown += new System.EventHandler(this.comboBoxServer_DropDown);
         // 
         // comboBoxDatabase
         // 
         this.comboBoxDatabase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
         this.comboBoxDatabase.DropDownHeight = 200;
         this.comboBoxDatabase.FormattingEnabled = true;
         this.comboBoxDatabase.IntegralHeight = false;
         this.comboBoxDatabase.Location = new System.Drawing.Point(98, 118);
         this.comboBoxDatabase.Name = "comboBoxDatabase";
         this.comboBoxDatabase.Size = new System.Drawing.Size(200, 21);
         this.comboBoxDatabase.TabIndex = 22;
         this.comboBoxDatabase.DropDown += new System.EventHandler(this.comboBoxDatabase_DropDown);
         // 
         // groupBoxDatabase
         // 
         this.groupBoxDatabase.Controls.Add(this.buttonCheckConnection);
         this.groupBoxDatabase.Controls.Add(this.textBoxPassword);
         this.groupBoxDatabase.Controls.Add(this.comboBoxDatabase);
         this.groupBoxDatabase.Controls.Add(this.label1);
         this.groupBoxDatabase.Controls.Add(this.comboBoxServer);
         this.groupBoxDatabase.Controls.Add(this.label2);
         this.groupBoxDatabase.Controls.Add(this.textBoxUserID);
         this.groupBoxDatabase.Controls.Add(this.label4);
         this.groupBoxDatabase.Controls.Add(this.comboBoxLanguage);
         this.groupBoxDatabase.Controls.Add(this.label3);
         this.groupBoxDatabase.Controls.Add(this.label7);
         this.groupBoxDatabase.Controls.Add(this.textBoxConnectTimeout);
         this.groupBoxDatabase.Controls.Add(this.label5);
         this.groupBoxDatabase.Location = new System.Drawing.Point(12, 12);
         this.groupBoxDatabase.Name = "groupBoxDatabase";
         this.groupBoxDatabase.Size = new System.Drawing.Size(485, 208);
         this.groupBoxDatabase.TabIndex = 23;
         this.groupBoxDatabase.TabStop = false;
         this.groupBoxDatabase.Text = "Datenbank Verbindung";
         // 
         // groupBoxDatenClient
         // 
         this.groupBoxDatenClient.Controls.Add(this.button1);
         this.groupBoxDatenClient.Controls.Add(this.textBoxLogFileFolder);
         this.groupBoxDatenClient.Controls.Add(this.label8);
         this.groupBoxDatenClient.Location = new System.Drawing.Point(12, 253);
         this.groupBoxDatenClient.Name = "groupBoxDatenClient";
         this.groupBoxDatenClient.Size = new System.Drawing.Size(485, 100);
         this.groupBoxDatenClient.TabIndex = 24;
         this.groupBoxDatenClient.TabStop = false;
         this.groupBoxDatenClient.Text = "DatenClieNT_TIA";
         // 
         // button1
         // 
         this.button1.BackColor = System.Drawing.Color.White;
         this.button1.ForeColor = System.Drawing.Color.Blue;
         this.button1.Location = new System.Drawing.Point(452, 25);
         this.button1.Name = "button1";
         this.button1.Size = new System.Drawing.Size(27, 23);
         this.button1.TabIndex = 23;
         this.button1.Text = "...";
         this.button1.UseVisualStyleBackColor = false;
         this.button1.Click += new System.EventHandler(this.button1_Click);
         // 
         // textBoxLogFileFolder
         // 
         this.textBoxLogFileFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
         this.textBoxLogFileFolder.Location = new System.Drawing.Point(145, 27);
         this.textBoxLogFileFolder.Name = "textBoxLogFileFolder";
         this.textBoxLogFileFolder.Size = new System.Drawing.Size(304, 20);
         this.textBoxLogFileFolder.TabIndex = 15;
         // 
         // label8
         // 
         this.label8.AutoSize = true;
         this.label8.ForeColor = System.Drawing.Color.DimGray;
         this.label8.Location = new System.Drawing.Point(31, 30);
         this.label8.Name = "label8";
         this.label8.Size = new System.Drawing.Size(108, 13);
         this.label8.TabIndex = 14;
         this.label8.Text = "Logdatei-Verzeichnis:";
         // 
         // buttonSaveConfig
         // 
         this.buttonSaveConfig.Location = new System.Drawing.Point(29, 378);
         this.buttonSaveConfig.Name = "buttonSaveConfig";
         this.buttonSaveConfig.Size = new System.Drawing.Size(122, 23);
         this.buttonSaveConfig.TabIndex = 25;
         this.buttonSaveConfig.Text = "Speichern unter...";
         this.buttonSaveConfig.UseVisualStyleBackColor = true;
         this.buttonSaveConfig.Click += new System.EventHandler(this.buttonSaveConfig_Click);
         // 
         // buttonLoad
         // 
         this.buttonLoad.Location = new System.Drawing.Point(157, 378);
         this.buttonLoad.Name = "buttonLoad";
         this.buttonLoad.Size = new System.Drawing.Size(122, 23);
         this.buttonLoad.TabIndex = 26;
         this.buttonLoad.Text = "Laden...";
         this.buttonLoad.UseVisualStyleBackColor = true;
         this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
         // 
         // openFileDialog
         // 
         this.openFileDialog.FileName = "openFileDialog1";
         // 
         // FormDcNetConfigManager
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.BackColor = System.Drawing.Color.Gainsboro;
         this.ClientSize = new System.Drawing.Size(509, 409);
         this.Controls.Add(this.buttonLoad);
         this.Controls.Add(this.buttonSaveConfig);
         this.Controls.Add(this.groupBoxDatenClient);
         this.Controls.Add(this.groupBoxDatabase);
         this.Controls.Add(this.buttonCancel);
         this.Controls.Add(this.buttonWriteRegistry);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.Name = "FormDcNetConfigManager";
         this.Text = "IROPA DatenClieNT_TIA Config Manager";
         this.groupBoxDatabase.ResumeLayout(false);
         this.groupBoxDatabase.PerformLayout();
         this.groupBoxDatenClient.ResumeLayout(false);
         this.groupBoxDatenClient.PerformLayout();
         this.ResumeLayout(false);

      }

      //#endregion

      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.Button buttonCheckConnection;
      private System.Windows.Forms.TextBox textBoxPassword;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.TextBox textBoxUserID;
      private System.Windows.Forms.Label label5;
      private System.Windows.Forms.TextBox textBoxConnectTimeout;
      private System.Windows.Forms.Label label7;
      private System.Windows.Forms.ComboBox comboBoxLanguage;
      private System.Windows.Forms.Button buttonWriteRegistry;
      private System.Windows.Forms.Button buttonCancel;
      private System.Windows.Forms.ComboBox comboBoxServer;
      private System.Windows.Forms.ComboBox comboBoxDatabase;
      private System.Windows.Forms.GroupBox groupBoxDatabase;
      private System.Windows.Forms.GroupBox groupBoxDatenClient;
      private System.Windows.Forms.Label label8;
      private System.Windows.Forms.Button button1;
      private System.Windows.Forms.TextBox textBoxLogFileFolder;
      private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogLogFile;
      private System.Windows.Forms.Button buttonSaveConfig;
      private System.Windows.Forms.SaveFileDialog saveFileDialog;
      private System.Windows.Forms.Button buttonLoad;
      private System.Windows.Forms.OpenFileDialog openFileDialog;
   }
}

