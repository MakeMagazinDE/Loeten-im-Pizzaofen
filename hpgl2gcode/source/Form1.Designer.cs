namespace HPGL2GCODE
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxIn = new System.Windows.Forms.TextBox();
            this.gcodeBoxOut = new System.Windows.Forms.TextBox();
            this.readButton = new System.Windows.Forms.Button();
            this.writeGCODE = new System.Windows.Forms.Button();
            this.preCodetextBox = new System.Windows.Forms.TextBox();
            this.finishButton = new System.Windows.Forms.Button();
            this.heighttextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.depthtextBox = new System.Windows.Forms.TextBox();
            this.hpglUnitsPerMMtextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.speedtextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.HPGLcheckBox = new System.Windows.Forms.CheckBox();
            this.cBNoCopper = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.rBStencil = new System.Windows.Forms.RadioButton();
            this.rBBohrung = new System.Windows.Forms.RadioButton();
            this.rBKupfer = new System.Windows.Forms.RadioButton();
            this.label7 = new System.Windows.Forms.Label();
            this.clearButton = new System.Windows.Forms.Button();
            this.convertGCODE = new System.Windows.Forms.Button();
            this.convertSVG = new System.Windows.Forms.Button();
            this.svgBoxOut = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.writeSVG = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.labFileName = new System.Windows.Forms.Label();
            this.bShowNC = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.deltaCNTnumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.deltanumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.passNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.rBUmriss = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.deltaCNTnumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.deltanumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.passNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxIn
            // 
            this.textBoxIn.Location = new System.Drawing.Point(8, 22);
            this.textBoxIn.Multiline = true;
            this.textBoxIn.Name = "textBoxIn";
            this.textBoxIn.ReadOnly = true;
            this.textBoxIn.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxIn.Size = new System.Drawing.Size(150, 218);
            this.textBoxIn.TabIndex = 0;
            // 
            // gcodeBoxOut
            // 
            this.gcodeBoxOut.Location = new System.Drawing.Point(334, 21);
            this.gcodeBoxOut.Multiline = true;
            this.gcodeBoxOut.Name = "gcodeBoxOut";
            this.gcodeBoxOut.ReadOnly = true;
            this.gcodeBoxOut.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.gcodeBoxOut.Size = new System.Drawing.Size(150, 230);
            this.gcodeBoxOut.TabIndex = 1;
            // 
            // readButton
            // 
            this.readButton.Location = new System.Drawing.Point(8, 264);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(110, 23);
            this.readButton.TabIndex = 2;
            this.readButton.Text = "Read HPLG (*.plt)";
            this.readButton.UseVisualStyleBackColor = true;
            this.readButton.Click += new System.EventHandler(this.readButton_Click);
            // 
            // writeGCODE
            // 
            this.writeGCODE.Location = new System.Drawing.Point(358, 288);
            this.writeGCODE.Name = "writeGCODE";
            this.writeGCODE.Size = new System.Drawing.Size(110, 23);
            this.writeGCODE.TabIndex = 3;
            this.writeGCODE.Text = "Write (*.nc)";
            this.writeGCODE.UseVisualStyleBackColor = true;
            this.writeGCODE.Click += new System.EventHandler(this.writeGCODE_Click);
            // 
            // preCodetextBox
            // 
            this.preCodetextBox.Location = new System.Drawing.Point(184, 22);
            this.preCodetextBox.Multiline = true;
            this.preCodetextBox.Name = "preCodetextBox";
            this.preCodetextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.preCodetextBox.Size = new System.Drawing.Size(127, 218);
            this.preCodetextBox.TabIndex = 4;
            // 
            // finishButton
            // 
            this.finishButton.Location = new System.Drawing.Point(503, 520);
            this.finishButton.Name = "finishButton";
            this.finishButton.Size = new System.Drawing.Size(110, 23);
            this.finishButton.TabIndex = 5;
            this.finishButton.Text = "Finish";
            this.finishButton.UseVisualStyleBackColor = true;
            this.finishButton.Click += new System.EventHandler(this.finish_Click);
            // 
            // heighttextBox
            // 
            this.heighttextBox.Location = new System.Drawing.Point(12, 112);
            this.heighttextBox.Name = "heighttextBox";
            this.heighttextBox.Size = new System.Drawing.Size(80, 20);
            this.heighttextBox.TabIndex = 6;
            this.heighttextBox.Text = "1.000";
            this.heighttextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 97);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Height";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 212);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Eindringtiefe";
            // 
            // depthtextBox
            // 
            this.depthtextBox.Location = new System.Drawing.Point(9, 228);
            this.depthtextBox.Name = "depthtextBox";
            this.depthtextBox.Size = new System.Drawing.Size(80, 20);
            this.depthtextBox.TabIndex = 9;
            this.depthtextBox.Text = "-0.1500";
            this.depthtextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // hpglUnitsPerMMtextBox
            // 
            this.hpglUnitsPerMMtextBox.Location = new System.Drawing.Point(12, 72);
            this.hpglUnitsPerMMtextBox.Name = "hpglUnitsPerMMtextBox";
            this.hpglUnitsPerMMtextBox.Size = new System.Drawing.Size(80, 20);
            this.hpglUnitsPerMMtextBox.TabIndex = 10;
            this.hpglUnitsPerMMtextBox.Text = "40,00";
            this.hpglUnitsPerMMtextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 57);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "HPGLUnitsPerMM";
            // 
            // speedtextBox
            // 
            this.speedtextBox.Location = new System.Drawing.Point(12, 32);
            this.speedtextBox.Name = "speedtextBox";
            this.speedtextBox.Size = new System.Drawing.Size(80, 20);
            this.speedtextBox.TabIndex = 12;
            this.speedtextBox.Text = "80.00";
            this.speedtextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Speed";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(5, 3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(36, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "HPGL";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(331, 3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "GCode";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.HPGLcheckBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.heighttextBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.hpglUnitsPerMMtextBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.speedtextBox);
            this.groupBox1.Location = new System.Drawing.Point(501, 309);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(140, 165);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Parameter";
            // 
            // HPGLcheckBox
            // 
            this.HPGLcheckBox.AutoSize = true;
            this.HPGLcheckBox.Location = new System.Drawing.Point(12, 142);
            this.HPGLcheckBox.Name = "HPGLcheckBox";
            this.HPGLcheckBox.Size = new System.Drawing.Size(127, 17);
            this.HPGLcheckBox.TabIndex = 0;
            this.HPGLcheckBox.Text = "HPGL als Kommentar";
            this.HPGLcheckBox.UseVisualStyleBackColor = true;
            // 
            // cBNoCopper
            // 
            this.cBNoCopper.AutoSize = true;
            this.cBNoCopper.Location = new System.Drawing.Point(9, 36);
            this.cBNoCopper.Name = "cBNoCopper";
            this.cBNoCopper.Size = new System.Drawing.Size(81, 17);
            this.cBNoCopper.TabIndex = 29;
            this.cBNoCopper.Text = "Kein Kupfer";
            this.cBNoCopper.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(9, 54);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(46, 13);
            this.label11.TabIndex = 27;
            this.label11.Text = "Abstand";
            // 
            // rBStencil
            // 
            this.rBStencil.AutoSize = true;
            this.rBStencil.Location = new System.Drawing.Point(9, 181);
            this.rBStencil.Name = "rBStencil";
            this.rBStencil.Size = new System.Drawing.Size(57, 17);
            this.rBStencil.TabIndex = 28;
            this.rBStencil.TabStop = true;
            this.rBStencil.Text = "Stencil";
            this.rBStencil.UseVisualStyleBackColor = true;
            // 
            // rBBohrung
            // 
            this.rBBohrung.AutoSize = true;
            this.rBBohrung.Location = new System.Drawing.Point(9, 159);
            this.rBBohrung.Name = "rBBohrung";
            this.rBBohrung.Size = new System.Drawing.Size(65, 17);
            this.rBBohrung.TabIndex = 27;
            this.rBBohrung.Text = "Bohrung";
            this.rBBohrung.UseVisualStyleBackColor = true;
            // 
            // rBKupfer
            // 
            this.rBKupfer.AutoSize = true;
            this.rBKupfer.Checked = true;
            this.rBKupfer.Location = new System.Drawing.Point(9, 17);
            this.rBKupfer.Name = "rBKupfer";
            this.rBKupfer.Size = new System.Drawing.Size(56, 17);
            this.rBKupfer.TabIndex = 25;
            this.rBKupfer.TabStop = true;
            this.rBKupfer.Text = "Kupfer";
            this.rBKupfer.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(181, 3);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "PreCode";
            // 
            // clearButton
            // 
            this.clearButton.Location = new System.Drawing.Point(354, 521);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(110, 23);
            this.clearButton.TabIndex = 18;
            this.clearButton.Text = "Clear ALL";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // convertGCODE
            // 
            this.convertGCODE.Location = new System.Drawing.Point(358, 260);
            this.convertGCODE.Name = "convertGCODE";
            this.convertGCODE.Size = new System.Drawing.Size(110, 23);
            this.convertGCODE.TabIndex = 19;
            this.convertGCODE.Text = "Convert to GCODE";
            this.convertGCODE.UseVisualStyleBackColor = true;
            this.convertGCODE.Click += new System.EventHandler(this.convertGCODE_Click);
            // 
            // convertSVG
            // 
            this.convertSVG.Location = new System.Drawing.Point(15, 520);
            this.convertSVG.Name = "convertSVG";
            this.convertSVG.Size = new System.Drawing.Size(110, 23);
            this.convertSVG.TabIndex = 20;
            this.convertSVG.Text = "Convert to SVG";
            this.convertSVG.UseVisualStyleBackColor = true;
            this.convertSVG.Click += new System.EventHandler(this.convertSVG_Click);
            // 
            // svgBoxOut
            // 
            this.svgBoxOut.Location = new System.Drawing.Point(15, 352);
            this.svgBoxOut.Multiline = true;
            this.svgBoxOut.Name = "svgBoxOut";
            this.svgBoxOut.ReadOnly = true;
            this.svgBoxOut.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.svgBoxOut.Size = new System.Drawing.Size(379, 156);
            this.svgBoxOut.TabIndex = 21;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 328);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(29, 13);
            this.label9.TabIndex = 22;
            this.label9.Text = "SVG";
            // 
            // writeSVG
            // 
            this.writeSVG.Location = new System.Drawing.Point(131, 520);
            this.writeSVG.Name = "writeSVG";
            this.writeSVG.Size = new System.Drawing.Size(110, 23);
            this.writeSVG.TabIndex = 24;
            this.writeSVG.Text = "Write (*.svg)";
            this.writeSVG.UseVisualStyleBackColor = true;
            this.writeSVG.Click += new System.EventHandler(this.writeSVG_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(8, 294);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(76, 13);
            this.label10.TabIndex = 25;
            this.label10.Text = "Aktuelle Datei:";
            // 
            // labFileName
            // 
            this.labFileName.AutoSize = true;
            this.labFileName.Location = new System.Drawing.Point(90, 294);
            this.labFileName.Name = "labFileName";
            this.labFileName.Size = new System.Drawing.Size(58, 13);
            this.labFileName.TabIndex = 26;
            this.labFileName.Text = "Dateiname";
            // 
            // bShowNC
            // 
            this.bShowNC.Location = new System.Drawing.Point(358, 318);
            this.bShowNC.Name = "bShowNC";
            this.bShowNC.Size = new System.Drawing.Size(110, 23);
            this.bShowNC.TabIndex = 27;
            this.bShowNC.Text = "Display GCODE";
            this.bShowNC.UseVisualStyleBackColor = true;
            this.bShowNC.Click += new System.EventHandler(this.bShowNC_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.deltaCNTnumericUpDown);
            this.groupBox2.Controls.Add(this.deltanumericUpDown);
            this.groupBox2.Controls.Add(this.passNumericUpDown);
            this.groupBox2.Controls.Add(this.rBBohrung);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.rBUmriss);
            this.groupBox2.Controls.Add(this.rBStencil);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.cBNoCopper);
            this.groupBox2.Controls.Add(this.rBKupfer);
            this.groupBox2.Controls.Add(this.depthtextBox);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(500, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(140, 300);
            this.groupBox2.TabIndex = 28;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Fräsen";
            // 
            // deltaCNTnumericUpDown
            // 
            this.deltaCNTnumericUpDown.Location = new System.Drawing.Point(6, 97);
            this.deltaCNTnumericUpDown.Name = "deltaCNTnumericUpDown";
            this.deltaCNTnumericUpDown.Size = new System.Drawing.Size(80, 20);
            this.deltaCNTnumericUpDown.TabIndex = 30;
            this.deltaCNTnumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // deltanumericUpDown
            // 
            this.deltanumericUpDown.Location = new System.Drawing.Point(6, 70);
            this.deltanumericUpDown.Name = "deltanumericUpDown";
            this.deltanumericUpDown.Size = new System.Drawing.Size(80, 20);
            this.deltanumericUpDown.TabIndex = 29;
            this.deltanumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // passNumericUpDown
            // 
            this.passNumericUpDown.Location = new System.Drawing.Point(9, 271);
            this.passNumericUpDown.Name = "passNumericUpDown";
            this.passNumericUpDown.Size = new System.Drawing.Size(80, 20);
            this.passNumericUpDown.TabIndex = 1;
            this.passNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.passNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(9, 255);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(119, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "Anzahl Fräsdurchgänge";
            // 
            // rBUmriss
            // 
            this.rBUmriss.AutoSize = true;
            this.rBUmriss.Location = new System.Drawing.Point(9, 137);
            this.rBUmriss.Name = "rBUmriss";
            this.rBUmriss.Size = new System.Drawing.Size(56, 17);
            this.rBUmriss.TabIndex = 26;
            this.rBUmriss.Text = "Umriss";
            this.rBUmriss.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(641, 558);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.bShowNC);
            this.Controls.Add(this.labFileName);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.writeSVG);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.svgBoxOut);
            this.Controls.Add(this.convertSVG);
            this.Controls.Add(this.convertGCODE);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.finishButton);
            this.Controls.Add(this.preCodetextBox);
            this.Controls.Add(this.writeGCODE);
            this.Controls.Add(this.readButton);
            this.Controls.Add(this.gcodeBoxOut);
            this.Controls.Add(this.textBoxIn);
            this.Name = "Form1";
            this.Text = "HPGL2GCODE Version 1.06";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.deltaCNTnumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.deltanumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.passNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxIn;
        private System.Windows.Forms.TextBox gcodeBoxOut;
        private System.Windows.Forms.Button readButton;
        private System.Windows.Forms.Button writeGCODE;
        private System.Windows.Forms.TextBox preCodetextBox;
        private System.Windows.Forms.Button finishButton;
        private System.Windows.Forms.TextBox heighttextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox depthtextBox;
        private System.Windows.Forms.TextBox hpglUnitsPerMMtextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox speedtextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox HPGLcheckBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Button convertGCODE;
        private System.Windows.Forms.Button convertSVG;
        private System.Windows.Forms.TextBox svgBoxOut;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button writeSVG;
        private System.Windows.Forms.RadioButton rBKupfer;
        private System.Windows.Forms.RadioButton rBBohrung;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label labFileName;
        private System.Windows.Forms.RadioButton rBStencil;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox cBNoCopper;
        private System.Windows.Forms.Button bShowNC;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown passNumericUpDown;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.RadioButton rBUmriss;
        private System.Windows.Forms.NumericUpDown deltanumericUpDown;
        private System.Windows.Forms.NumericUpDown deltaCNTnumericUpDown;
    }
}

