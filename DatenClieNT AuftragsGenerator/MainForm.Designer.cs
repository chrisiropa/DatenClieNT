namespace DatenClieNT_AuftragsGenerator
{
   partial class MainForm
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

      #region Vom Windows Form-Designer generierter Code

      /// <summary>
      /// Erforderliche Methode für die Designerunterstützung.
      /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
      /// </summary>
      private void InitializeComponent()
      {
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
         this.textBoxEingabe = new System.Windows.Forms.TextBox();
         this.buttonGenerate = new System.Windows.Forms.Button();
         this.textBoxTabellenname = new System.Windows.Forms.TextBox();
         this.label1 = new System.Windows.Forms.Label();
         this.label2 = new System.Windows.Forms.Label();
         this.textBoxSubstAlt = new System.Windows.Forms.TextBox();
         this.label3 = new System.Windows.Forms.Label();
         this.label4 = new System.Windows.Forms.Label();
         this.textBoxSubstNeu = new System.Windows.Forms.TextBox();
         this.checkBoxAutoPK = new System.Windows.Forms.CheckBox();
         this.label5 = new System.Windows.Forms.Label();
         this.textBoxTrennzeichen = new System.Windows.Forms.TextBox();
         this.button1 = new System.Windows.Forms.Button();
         this.label6 = new System.Windows.Forms.Label();
         this.textBoxDatumZeitSpalte = new System.Windows.Forms.TextBox();
         this.groupBoxAuftrag = new System.Windows.Forms.GroupBox();
         this.labelTag = new System.Windows.Forms.Label();
         this.textBoxTagValues = new System.Windows.Forms.TextBox();
         this.labelLogForTableFN2 = new System.Windows.Forms.Label();
         this.textBoxLogTableForFN2 = new System.Windows.Forms.TextBox();
         this.labelUpdateKriterium = new System.Windows.Forms.Label();
         this.textBoxUpdateKriterium = new System.Windows.Forms.TextBox();
         this.labelUpdateIntervall = new System.Windows.Forms.Label();
         this.textBoxUpdateIntervall = new System.Windows.Forms.TextBox();
         this.labelBlockTag = new System.Windows.Forms.Label();
         this.textBoxBlockTag = new System.Windows.Forms.TextBox();
         this.label13 = new System.Windows.Forms.Label();
         this.textBoxTabelleViewStatement = new System.Windows.Forms.TextBox();
         this.textBoxPseudoScript = new System.Windows.Forms.TextBox();
         this.label12 = new System.Windows.Forms.Label();
         this.label11 = new System.Windows.Forms.Label();
         this.comboBoxFunktionsnummer = new System.Windows.Forms.ComboBox();
         this.textBoxBezeichnung = new System.Windows.Forms.TextBox();
         this.label10 = new System.Windows.Forms.Label();
         this.textBoxDatenClientID = new System.Windows.Forms.TextBox();
         this.label9 = new System.Windows.Forms.Label();
         this.textBoxAuftragsnummer = new System.Windows.Forms.TextBox();
         this.label8 = new System.Windows.Forms.Label();
         this.textBoxID = new System.Windows.Forms.TextBox();
         this.label7 = new System.Windows.Forms.Label();
         this.groupBoxTabelle = new System.Windows.Forms.GroupBox();
         this.groupBox1 = new System.Windows.Forms.GroupBox();
         this.textBox1 = new System.Windows.Forms.TextBox();
         this.label14 = new System.Windows.Forms.Label();
         this.textBoxDetailPrefix = new System.Windows.Forms.TextBox();
         this.groupBoxAuftrag.SuspendLayout();
         this.groupBoxTabelle.SuspendLayout();
         this.groupBox1.SuspendLayout();
         this.SuspendLayout();
         // 
         // textBoxEingabe
         // 
         this.textBoxEingabe.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.textBoxEingabe.Location = new System.Drawing.Point(12, 19);
         this.textBoxEingabe.Multiline = true;
         this.textBoxEingabe.Name = "textBoxEingabe";
         this.textBoxEingabe.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.textBoxEingabe.Size = new System.Drawing.Size(1149, 287);
         this.textBoxEingabe.TabIndex = 0;
         // 
         // buttonGenerate
         // 
         this.buttonGenerate.BackColor = System.Drawing.Color.Lime;
         this.buttonGenerate.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.buttonGenerate.ForeColor = System.Drawing.Color.Blue;
         this.buttonGenerate.Location = new System.Drawing.Point(12, 570);
         this.buttonGenerate.Name = "buttonGenerate";
         this.buttonGenerate.Size = new System.Drawing.Size(868, 50);
         this.buttonGenerate.TabIndex = 1;
         this.buttonGenerate.Text = "Statement generieren und in Zwischenablage kopieren !";
         this.buttonGenerate.UseVisualStyleBackColor = false;
         this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
         // 
         // textBoxTabellenname
         // 
         this.textBoxTabellenname.Location = new System.Drawing.Point(102, 29);
         this.textBoxTabellenname.Name = "textBoxTabellenname";
         this.textBoxTabellenname.Size = new System.Drawing.Size(231, 20);
         this.textBoxTabellenname.TabIndex = 2;
         this.textBoxTabellenname.MouseHover += new System.EventHandler(this.textBoxTabellenname_MouseHover);
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(7, 32);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(74, 13);
         this.label1.TabIndex = 3;
         this.label1.Text = "Tabellenname";
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(7, 86);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(145, 13);
         this.label2.TabIndex = 5;
         this.label2.Text = "Substitution für Spaltenname:";
         // 
         // textBoxSubstAlt
         // 
         this.textBoxSubstAlt.Location = new System.Drawing.Point(182, 83);
         this.textBoxSubstAlt.Name = "textBoxSubstAlt";
         this.textBoxSubstAlt.Size = new System.Drawing.Size(48, 20);
         this.textBoxSubstAlt.TabIndex = 4;
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(156, 86);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(19, 13);
         this.label3.TabIndex = 6;
         this.label3.Text = "Alt";
         // 
         // label4
         // 
         this.label4.AutoSize = true;
         this.label4.Location = new System.Drawing.Point(251, 86);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(27, 13);
         this.label4.TabIndex = 8;
         this.label4.Text = "Neu";
         // 
         // textBoxSubstNeu
         // 
         this.textBoxSubstNeu.Location = new System.Drawing.Point(284, 83);
         this.textBoxSubstNeu.Name = "textBoxSubstNeu";
         this.textBoxSubstNeu.Size = new System.Drawing.Size(49, 20);
         this.textBoxSubstNeu.TabIndex = 7;
         // 
         // checkBoxAutoPK
         // 
         this.checkBoxAutoPK.AutoSize = true;
         this.checkBoxAutoPK.Location = new System.Drawing.Point(10, 118);
         this.checkBoxAutoPK.Name = "checkBoxAutoPK";
         this.checkBoxAutoPK.Size = new System.Drawing.Size(129, 17);
         this.checkBoxAutoPK.TabIndex = 9;
         this.checkBoxAutoPK.Text = "AutoIndex ID anlegen";
         this.checkBoxAutoPK.UseVisualStyleBackColor = true;
         // 
         // label5
         // 
         this.label5.AutoSize = true;
         this.label5.Location = new System.Drawing.Point(145, 118);
         this.label5.Name = "label5";
         this.label5.Size = new System.Drawing.Size(127, 13);
         this.label5.TabIndex = 10;
         this.label5.Text = "Trennzeichen (TAB=Std.)";
         // 
         // textBoxTrennzeichen
         // 
         this.textBoxTrennzeichen.Location = new System.Drawing.Point(278, 118);
         this.textBoxTrennzeichen.Name = "textBoxTrennzeichen";
         this.textBoxTrennzeichen.Size = new System.Drawing.Size(55, 20);
         this.textBoxTrennzeichen.TabIndex = 11;
         // 
         // button1
         // 
         this.button1.BackColor = System.Drawing.Color.Teal;
         this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.button1.ForeColor = System.Drawing.Color.White;
         this.button1.Location = new System.Drawing.Point(886, 570);
         this.button1.Name = "button1";
         this.button1.Size = new System.Drawing.Size(142, 50);
         this.button1.TabIndex = 12;
         this.button1.Text = "Gebrauchsanweisung";
         this.button1.UseVisualStyleBackColor = false;
         this.button1.Click += new System.EventHandler(this.button1_Click);
         // 
         // label6
         // 
         this.label6.AutoSize = true;
         this.label6.Location = new System.Drawing.Point(7, 58);
         this.label6.Name = "label6";
         this.label6.Size = new System.Drawing.Size(89, 13);
         this.label6.TabIndex = 14;
         this.label6.Text = "DatumZeit-Spalte";
         // 
         // textBoxDatumZeitSpalte
         // 
         this.textBoxDatumZeitSpalte.Location = new System.Drawing.Point(102, 55);
         this.textBoxDatumZeitSpalte.Name = "textBoxDatumZeitSpalte";
         this.textBoxDatumZeitSpalte.Size = new System.Drawing.Size(231, 20);
         this.textBoxDatumZeitSpalte.TabIndex = 13;
         this.textBoxDatumZeitSpalte.Text = "DC_DateTime";
         this.textBoxDatumZeitSpalte.MouseHover += new System.EventHandler(this.textBoxDatumZeitSpalte_MouseHover);
         // 
         // groupBoxAuftrag
         // 
         this.groupBoxAuftrag.Controls.Add(this.label14);
         this.groupBoxAuftrag.Controls.Add(this.textBoxDetailPrefix);
         this.groupBoxAuftrag.Controls.Add(this.labelTag);
         this.groupBoxAuftrag.Controls.Add(this.textBoxTagValues);
         this.groupBoxAuftrag.Controls.Add(this.labelLogForTableFN2);
         this.groupBoxAuftrag.Controls.Add(this.textBoxLogTableForFN2);
         this.groupBoxAuftrag.Controls.Add(this.labelUpdateKriterium);
         this.groupBoxAuftrag.Controls.Add(this.textBoxUpdateKriterium);
         this.groupBoxAuftrag.Controls.Add(this.labelUpdateIntervall);
         this.groupBoxAuftrag.Controls.Add(this.textBoxUpdateIntervall);
         this.groupBoxAuftrag.Controls.Add(this.labelBlockTag);
         this.groupBoxAuftrag.Controls.Add(this.textBoxBlockTag);
         this.groupBoxAuftrag.Controls.Add(this.label13);
         this.groupBoxAuftrag.Controls.Add(this.textBoxTabelleViewStatement);
         this.groupBoxAuftrag.Controls.Add(this.textBoxPseudoScript);
         this.groupBoxAuftrag.Controls.Add(this.label12);
         this.groupBoxAuftrag.Controls.Add(this.label11);
         this.groupBoxAuftrag.Controls.Add(this.comboBoxFunktionsnummer);
         this.groupBoxAuftrag.Controls.Add(this.textBoxBezeichnung);
         this.groupBoxAuftrag.Controls.Add(this.label10);
         this.groupBoxAuftrag.Controls.Add(this.textBoxDatenClientID);
         this.groupBoxAuftrag.Controls.Add(this.label9);
         this.groupBoxAuftrag.Controls.Add(this.textBoxAuftragsnummer);
         this.groupBoxAuftrag.Controls.Add(this.label8);
         this.groupBoxAuftrag.Controls.Add(this.textBoxID);
         this.groupBoxAuftrag.Controls.Add(this.label7);
         this.groupBoxAuftrag.Location = new System.Drawing.Point(361, 15);
         this.groupBoxAuftrag.Name = "groupBoxAuftrag";
         this.groupBoxAuftrag.Size = new System.Drawing.Size(816, 147);
         this.groupBoxAuftrag.TabIndex = 15;
         this.groupBoxAuftrag.TabStop = false;
         this.groupBoxAuftrag.Text = "Auftrag";
         // 
         // labelTag
         // 
         this.labelTag.AutoSize = true;
         this.labelTag.Location = new System.Drawing.Point(537, 66);
         this.labelTag.Name = "labelTag";
         this.labelTag.Size = new System.Drawing.Size(26, 13);
         this.labelTag.TabIndex = 24;
         this.labelTag.Text = "Tag";
         // 
         // textBoxTagValues
         // 
         this.textBoxTagValues.Location = new System.Drawing.Point(583, 63);
         this.textBoxTagValues.Name = "textBoxTagValues";
         this.textBoxTagValues.Size = new System.Drawing.Size(72, 20);
         this.textBoxTagValues.TabIndex = 23;
         this.textBoxTagValues.MouseHover += new System.EventHandler(this.textBoxTagValues_MouseHover);
         // 
         // labelLogForTableFN2
         // 
         this.labelLogForTableFN2.AutoSize = true;
         this.labelLogForTableFN2.Location = new System.Drawing.Point(496, 38);
         this.labelLogForTableFN2.Name = "labelLogForTableFN2";
         this.labelLogForTableFN2.Size = new System.Drawing.Size(87, 13);
         this.labelLogForTableFN2.TabIndex = 22;
         this.labelLogForTableFN2.Text = "LogForTableFN2";
         // 
         // textBoxLogTableForFN2
         // 
         this.textBoxLogTableForFN2.Location = new System.Drawing.Point(583, 35);
         this.textBoxLogTableForFN2.Name = "textBoxLogTableForFN2";
         this.textBoxLogTableForFN2.Size = new System.Drawing.Size(72, 20);
         this.textBoxLogTableForFN2.TabIndex = 21;
         this.textBoxLogTableForFN2.MouseHover += new System.EventHandler(this.textBoxLogTableForFN2_MouseHover);
         // 
         // labelUpdateKriterium
         // 
         this.labelUpdateKriterium.AutoSize = true;
         this.labelUpdateKriterium.Location = new System.Drawing.Point(337, 121);
         this.labelUpdateKriterium.Name = "labelUpdateKriterium";
         this.labelUpdateKriterium.Size = new System.Drawing.Size(82, 13);
         this.labelUpdateKriterium.TabIndex = 20;
         this.labelUpdateKriterium.Text = "UpdateKriterium";
         this.labelUpdateKriterium.Click += new System.EventHandler(this.labelUpdateKriterium_Click);
         // 
         // textBoxUpdateKriterium
         // 
         this.textBoxUpdateKriterium.Location = new System.Drawing.Point(420, 118);
         this.textBoxUpdateKriterium.Name = "textBoxUpdateKriterium";
         this.textBoxUpdateKriterium.Size = new System.Drawing.Size(72, 20);
         this.textBoxUpdateKriterium.TabIndex = 19;
         this.textBoxUpdateKriterium.MouseHover += new System.EventHandler(this.textBoxUpdateKriterium_MouseHover);
         // 
         // labelUpdateIntervall
         // 
         this.labelUpdateIntervall.AutoSize = true;
         this.labelUpdateIntervall.Location = new System.Drawing.Point(337, 94);
         this.labelUpdateIntervall.Name = "labelUpdateIntervall";
         this.labelUpdateIntervall.Size = new System.Drawing.Size(79, 13);
         this.labelUpdateIntervall.TabIndex = 18;
         this.labelUpdateIntervall.Text = "UpdateIntervall";
         // 
         // textBoxUpdateIntervall
         // 
         this.textBoxUpdateIntervall.Location = new System.Drawing.Point(420, 91);
         this.textBoxUpdateIntervall.Name = "textBoxUpdateIntervall";
         this.textBoxUpdateIntervall.Size = new System.Drawing.Size(72, 20);
         this.textBoxUpdateIntervall.TabIndex = 17;
         this.textBoxUpdateIntervall.MouseHover += new System.EventHandler(this.textBoxUpdateIntervall_MouseHover);
         // 
         // labelBlockTag
         // 
         this.labelBlockTag.AutoSize = true;
         this.labelBlockTag.Location = new System.Drawing.Point(337, 66);
         this.labelBlockTag.Name = "labelBlockTag";
         this.labelBlockTag.Size = new System.Drawing.Size(53, 13);
         this.labelBlockTag.TabIndex = 16;
         this.labelBlockTag.Text = "BlockTag";
         // 
         // textBoxBlockTag
         // 
         this.textBoxBlockTag.Location = new System.Drawing.Point(420, 63);
         this.textBoxBlockTag.Name = "textBoxBlockTag";
         this.textBoxBlockTag.Size = new System.Drawing.Size(72, 20);
         this.textBoxBlockTag.TabIndex = 15;
         this.textBoxBlockTag.MouseHover += new System.EventHandler(this.textBoxBlockTag_MouseHover);
         // 
         // label13
         // 
         this.label13.AutoSize = true;
         this.label13.Location = new System.Drawing.Point(3, 91);
         this.label13.Name = "label13";
         this.label13.Size = new System.Drawing.Size(123, 13);
         this.label13.TabIndex = 14;
         this.label13.Text = "Tabelle/View/Statement";
         // 
         // textBoxTabelleViewStatement
         // 
         this.textBoxTabelleViewStatement.Location = new System.Drawing.Point(132, 88);
         this.textBoxTabelleViewStatement.Name = "textBoxTabelleViewStatement";
         this.textBoxTabelleViewStatement.Size = new System.Drawing.Size(198, 20);
         this.textBoxTabelleViewStatement.TabIndex = 13;
         this.textBoxTabelleViewStatement.MouseHover += new System.EventHandler(this.textBoxTabelleViewStatement_MouseHover);
         // 
         // textBoxPseudoScript
         // 
         this.textBoxPseudoScript.Location = new System.Drawing.Point(79, 112);
         this.textBoxPseudoScript.Name = "textBoxPseudoScript";
         this.textBoxPseudoScript.Size = new System.Drawing.Size(251, 20);
         this.textBoxPseudoScript.TabIndex = 12;
         this.textBoxPseudoScript.Text = "DC_DateTime=@DC_LOCAL_DATETIME";
         this.textBoxPseudoScript.MouseHover += new System.EventHandler(this.textBoxPseudoScript_MouseHover);
         // 
         // label12
         // 
         this.label12.AutoSize = true;
         this.label12.Location = new System.Drawing.Point(3, 115);
         this.label12.Name = "label12";
         this.label12.Size = new System.Drawing.Size(70, 13);
         this.label12.TabIndex = 11;
         this.label12.Text = "PseudoScript";
         // 
         // label11
         // 
         this.label11.AutoSize = true;
         this.label11.Location = new System.Drawing.Point(338, 35);
         this.label11.Name = "label11";
         this.label11.Size = new System.Drawing.Size(90, 13);
         this.label11.TabIndex = 9;
         this.label11.Text = "Funktionsnummer";
         // 
         // comboBoxFunktionsnummer
         // 
         this.comboBoxFunktionsnummer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.comboBoxFunktionsnummer.FormattingEnabled = true;
         this.comboBoxFunktionsnummer.Items.AddRange(new object[] {
            "1",
            "5",
            "11",
            "12",
            "13",
            "2",
            "20"});
         this.comboBoxFunktionsnummer.Location = new System.Drawing.Point(430, 32);
         this.comboBoxFunktionsnummer.Name = "comboBoxFunktionsnummer";
         this.comboBoxFunktionsnummer.Size = new System.Drawing.Size(51, 21);
         this.comboBoxFunktionsnummer.TabIndex = 8;
         this.comboBoxFunktionsnummer.SelectedIndexChanged += new System.EventHandler(this.comboBoxFunktionsnummer_SelectedIndexChanged);
         this.comboBoxFunktionsnummer.MouseHover += new System.EventHandler(this.comboBoxFunktionsnummer_MouseHover);
         // 
         // textBoxBezeichnung
         // 
         this.textBoxBezeichnung.Location = new System.Drawing.Point(78, 63);
         this.textBoxBezeichnung.Name = "textBoxBezeichnung";
         this.textBoxBezeichnung.Size = new System.Drawing.Size(252, 20);
         this.textBoxBezeichnung.TabIndex = 7;
         this.textBoxBezeichnung.MouseHover += new System.EventHandler(this.textBoxBezeichnung_MouseHover);
         // 
         // label10
         // 
         this.label10.AutoSize = true;
         this.label10.Location = new System.Drawing.Point(3, 63);
         this.label10.Name = "label10";
         this.label10.Size = new System.Drawing.Size(69, 13);
         this.label10.TabIndex = 6;
         this.label10.Text = "Bezeichnung";
         // 
         // textBoxDatenClientID
         // 
         this.textBoxDatenClientID.Location = new System.Drawing.Point(293, 32);
         this.textBoxDatenClientID.Name = "textBoxDatenClientID";
         this.textBoxDatenClientID.Size = new System.Drawing.Size(38, 20);
         this.textBoxDatenClientID.TabIndex = 5;
         this.textBoxDatenClientID.MouseHover += new System.EventHandler(this.textBoxDatenClientID_MouseHover);
         // 
         // label9
         // 
         this.label9.AutoSize = true;
         this.label9.Location = new System.Drawing.Point(217, 35);
         this.label9.Name = "label9";
         this.label9.Size = new System.Drawing.Size(72, 13);
         this.label9.TabIndex = 4;
         this.label9.Text = "DatenclientID";
         // 
         // textBoxAuftragsnummer
         // 
         this.textBoxAuftragsnummer.Location = new System.Drawing.Point(161, 32);
         this.textBoxAuftragsnummer.Name = "textBoxAuftragsnummer";
         this.textBoxAuftragsnummer.Size = new System.Drawing.Size(50, 20);
         this.textBoxAuftragsnummer.TabIndex = 3;
         this.textBoxAuftragsnummer.MouseHover += new System.EventHandler(this.textBoxAuftragsnummer_MouseHover);
         // 
         // label8
         // 
         this.label8.AutoSize = true;
         this.label8.Location = new System.Drawing.Point(76, 35);
         this.label8.Name = "label8";
         this.label8.Size = new System.Drawing.Size(83, 13);
         this.label8.TabIndex = 2;
         this.label8.Text = "Auftragsnummer";
         // 
         // textBoxID
         // 
         this.textBoxID.Location = new System.Drawing.Point(24, 32);
         this.textBoxID.Name = "textBoxID";
         this.textBoxID.Size = new System.Drawing.Size(51, 20);
         this.textBoxID.TabIndex = 1;
         this.textBoxID.MouseHover += new System.EventHandler(this.textBoxID_MouseHover);
         // 
         // label7
         // 
         this.label7.AutoSize = true;
         this.label7.Location = new System.Drawing.Point(6, 35);
         this.label7.Name = "label7";
         this.label7.Size = new System.Drawing.Size(18, 13);
         this.label7.TabIndex = 0;
         this.label7.Text = "ID";
         // 
         // groupBoxTabelle
         // 
         this.groupBoxTabelle.Controls.Add(this.textBoxDatumZeitSpalte);
         this.groupBoxTabelle.Controls.Add(this.label6);
         this.groupBoxTabelle.Controls.Add(this.textBoxSubstNeu);
         this.groupBoxTabelle.Controls.Add(this.textBoxTrennzeichen);
         this.groupBoxTabelle.Controls.Add(this.label5);
         this.groupBoxTabelle.Controls.Add(this.textBoxSubstAlt);
         this.groupBoxTabelle.Controls.Add(this.label2);
         this.groupBoxTabelle.Controls.Add(this.checkBoxAutoPK);
         this.groupBoxTabelle.Controls.Add(this.label3);
         this.groupBoxTabelle.Controls.Add(this.label4);
         this.groupBoxTabelle.Controls.Add(this.label1);
         this.groupBoxTabelle.Controls.Add(this.textBoxTabellenname);
         this.groupBoxTabelle.Location = new System.Drawing.Point(12, 15);
         this.groupBoxTabelle.Name = "groupBoxTabelle";
         this.groupBoxTabelle.Size = new System.Drawing.Size(343, 147);
         this.groupBoxTabelle.TabIndex = 16;
         this.groupBoxTabelle.TabStop = false;
         this.groupBoxTabelle.Text = "Tabelle anlegen";
         // 
         // groupBox1
         // 
         this.groupBox1.Controls.Add(this.textBoxEingabe);
         this.groupBox1.Location = new System.Drawing.Point(10, 242);
         this.groupBox1.Name = "groupBox1";
         this.groupBox1.Size = new System.Drawing.Size(1167, 322);
         this.groupBox1.TabIndex = 17;
         this.groupBox1.TabStop = false;
         this.groupBox1.Text = "AuftragDetails";
         // 
         // textBox1
         // 
         this.textBox1.BackColor = System.Drawing.Color.Red;
         this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.textBox1.ForeColor = System.Drawing.Color.White;
         this.textBox1.Location = new System.Drawing.Point(33, 168);
         this.textBox1.Multiline = true;
         this.textBox1.Name = "textBox1";
         this.textBox1.ReadOnly = true;
         this.textBox1.Size = new System.Drawing.Size(1144, 68);
         this.textBox1.TabIndex = 15;
         this.textBox1.Text = resources.GetString("textBox1.Text");
         this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
         // 
         // label14
         // 
         this.label14.AutoSize = true;
         this.label14.Location = new System.Drawing.Point(514, 96);
         this.label14.Name = "label14";
         this.label14.Size = new System.Drawing.Size(100, 13);
         this.label14.TabIndex = 26;
         this.label14.Text = "Auftragsdetail Prefix";
         // 
         // textBoxDetailPrefix
         // 
         this.textBoxDetailPrefix.Location = new System.Drawing.Point(510, 119);
         this.textBoxDetailPrefix.Name = "textBoxDetailPrefix";
         this.textBoxDetailPrefix.Size = new System.Drawing.Size(300, 20);
         this.textBoxDetailPrefix.TabIndex = 25;
         // 
         // MainForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(1189, 635);
         this.Controls.Add(this.textBox1);
         this.Controls.Add(this.groupBox1);
         this.Controls.Add(this.groupBoxTabelle);
         this.Controls.Add(this.groupBoxAuftrag);
         this.Controls.Add(this.button1);
         this.Controls.Add(this.buttonGenerate);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.Name = "MainForm";
         this.Text = "Tabellen - und Auftragsgenerator für den IROPA DatenClieNT (TIA-Version)";
         this.groupBoxAuftrag.ResumeLayout(false);
         this.groupBoxAuftrag.PerformLayout();
         this.groupBoxTabelle.ResumeLayout(false);
         this.groupBoxTabelle.PerformLayout();
         this.groupBox1.ResumeLayout(false);
         this.groupBox1.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.TextBox textBoxEingabe;
      private System.Windows.Forms.Button buttonGenerate;
      private System.Windows.Forms.TextBox textBoxTabellenname;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.TextBox textBoxSubstAlt;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.TextBox textBoxSubstNeu;
      private System.Windows.Forms.CheckBox checkBoxAutoPK;
      private System.Windows.Forms.Label label5;
      private System.Windows.Forms.TextBox textBoxTrennzeichen;
      private System.Windows.Forms.Button button1;
      private System.Windows.Forms.Label label6;
      private System.Windows.Forms.TextBox textBoxDatumZeitSpalte;
      private System.Windows.Forms.GroupBox groupBoxAuftrag;
      private System.Windows.Forms.TextBox textBoxAuftragsnummer;
      private System.Windows.Forms.Label label8;
      private System.Windows.Forms.TextBox textBoxID;
      private System.Windows.Forms.Label label7;
      private System.Windows.Forms.TextBox textBoxDatenClientID;
      private System.Windows.Forms.Label label9;
      private System.Windows.Forms.TextBox textBoxBezeichnung;
      private System.Windows.Forms.Label label10;
      private System.Windows.Forms.Label label11;
      private System.Windows.Forms.ComboBox comboBoxFunktionsnummer;
      private System.Windows.Forms.TextBox textBoxPseudoScript;
      private System.Windows.Forms.Label label12;
      private System.Windows.Forms.GroupBox groupBoxTabelle;
      private System.Windows.Forms.Label label13;
      private System.Windows.Forms.TextBox textBoxTabelleViewStatement;
      private System.Windows.Forms.Label labelBlockTag;
      private System.Windows.Forms.TextBox textBoxBlockTag;
      private System.Windows.Forms.Label labelUpdateIntervall;
      private System.Windows.Forms.TextBox textBoxUpdateIntervall;
      private System.Windows.Forms.GroupBox groupBox1;
      private System.Windows.Forms.Label labelUpdateKriterium;
      private System.Windows.Forms.TextBox textBoxUpdateKriterium;
      private System.Windows.Forms.Label labelTag;
      private System.Windows.Forms.TextBox textBoxTagValues;
      private System.Windows.Forms.Label labelLogForTableFN2;
      private System.Windows.Forms.TextBox textBoxLogTableForFN2;
      private System.Windows.Forms.TextBox textBox1;
      private System.Windows.Forms.Label label14;
      private System.Windows.Forms.TextBox textBoxDetailPrefix;
   }
}

