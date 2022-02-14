using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace HPGL2GCODE
{
    public partial class Form1 : Form
    {
        enum spindleStatus
        {
            up, down, dontknow
        }
        enum outputType
        {
            svg, gcode, dontknow
        }
        IniFile ini = new IniFile();
        static public string inipath = @"C:\HPGL";
        static public string ininame = @"\HPGL.ini";
        string filePath = string.Empty;
        Boolean error = false;
        string errorText = string.Empty;
        int errorLine = 0;
        int errorNbr = 0;
        int firstLine2Repeat;
        spindleStatus spindle;
        bool saveNeccessary;
        RadioButton selectedRadioButton;
        string strDepthKupfer = "-0.1500";
        string strDepthUmriss = "-1.3000";
        string strDepthBohrung = "-1.6000";
        string strDepthStencil = "-0.0000";
        string strDeltaNoCopper = "0.1000";
        string strCNTDeltaNoCopper = "1";
        decimal decPassesKupfer = 1;
        decimal decPassesUmriss = 5;
        decimal decPassesBohrung = 1;
        decimal decPassesStencil = 1;

        struct outlineStruct
        {
            public PointF oldValue;
            public PointF newValue;
            public float distance;
        }
        outlineStruct[] outlineArr;
        outlineStruct outlineTmp;
        int outlineArrNbr;

        public static PointF[] allPolygons;
        float maxX;
        float maxY;
        public static float factor;

        struct outlineDescStruct
        {
            public int firstLineOutlineArr;
            public int lastLineOutlineArr;
            public int MinValueLine;
        }
        outlineDescStruct outlineDesc;

        struct inlineStruct
        {
            public string hpgl;
            public bool used;
        }

        Polygon polygon;

        public Form1()
        {
            InitializeComponent();
            //
            allPolygons = new PointF[1];
            maxX = 0.0f;
            maxY = 0.0f;
            selectedRadioButton = rBKupfer;
            rBKupfer.CheckedChanged += new EventHandler(radioButton_CheckedChanged);
            rBBohrung.CheckedChanged += new EventHandler(radioButton_CheckedChanged);
            rBUmriss.CheckedChanged += new EventHandler(radioButton_CheckedChanged);
            rBStencil.CheckedChanged += new EventHandler(radioButton_CheckedChanged);
            cBNoCopper.CheckedChanged += new EventHandler(checkBox_CheckedChanged); ;
            writeGCODE.Enabled = false;
            writeSVG.Enabled = false;
            convertGCODE.Enabled = false;
            convertSVG.Enabled = false;
            clearButton.Enabled = false;
            bShowNC.Enabled = false;
            saveNeccessary = false;
            labFileName.Text = "";
            outlineArrNbr = 0;
            outlineStruct[] outlineArr = new outlineStruct[1];
            polygon = new Polygon();
            spindle = spindleStatus.dontknow;
            string iniStr = string.Concat(inipath, ininame);
            bool fexists = File.Exists(iniStr);
            // Set the Minimum, Maximum, and initial Value.
            passNumericUpDown.Maximum = 10;
            passNumericUpDown.Minimum = 1;
            deltanumericUpDown.DecimalPlaces = 2;
            deltanumericUpDown.Increment = 0.01M;
            deltanumericUpDown.Maximum = 10;
            deltanumericUpDown.Minimum = deltanumericUpDown.Increment;
            deltaCNTnumericUpDown.Maximum = 5;
            deltaCNTnumericUpDown.Minimum = 1;
            if (fexists)
            {
                ini.Load(iniStr);
                //  Returns a KeyValue in a certain section
                // speedtextBox
                speedtextBox.Text = ini.GetKeyValue("Parameter", "Speed");
                // hpglUnitsPerMMtextBox
                hpglUnitsPerMMtextBox.Text = ini.GetKeyValue("Parameter", "HPGLUnitsPerMM");
                // RadioButton
                if (ini.GetKeyValue("Parameter", "RadioButton") == "Kupfer")
                {
                    checkRadioButton(rBKupfer);
                }
                if (ini.GetKeyValue("Parameter", "RadioButton") == "Bohrung")
                {
                    checkRadioButton(rBBohrung);
                }
                if (ini.GetKeyValue("Parameter", "RadioButton") == "Umriss")
                {
                    checkRadioButton(rBUmriss);
                }
                if (ini.GetKeyValue("Parameter", "RadioButton") == "Stencil")
                {
                    checkRadioButton(rBStencil);
                }
                cBNoCopper.Checked = false;
                if (ini.GetKeyValue("Parameter", "CheckBox") == "KeinKupfer")
                    cBNoCopper.Checked = true;
                // depthtextBox
                strDepthKupfer = ini.GetKeyValue("Parameter", "DepthKupfer");
                //
                strDepthUmriss = ini.GetKeyValue("Parameter", "DepthUmriss");
                //
                strDepthBohrung = ini.GetKeyValue("Parameter", "DepthBohrung");
                //
                strDepthStencil = ini.GetKeyValue("Parameter", "DepthStencil");
                // Delta für KeinKupfer
                strDeltaNoCopper = ini.GetKeyValue("Parameter", "DeltaNoCopper");
                deltanumericUpDown.Text = strDeltaNoCopper.Replace(".", ",");
                // Anzahl Delta für KeinKupfer
                strCNTDeltaNoCopper = ini.GetKeyValue("Parameter", "CNTDeltaNoCopper");
                deltaCNTnumericUpDown.Text = strCNTDeltaNoCopper;
                // heighttextBox
                heighttextBox.Text = ini.GetKeyValue("Parameter", "Height");
                // HPGLcheckBox
                if (ini.GetKeyValue("Parameter", "HPGL") == "1")
                    HPGLcheckBox.Checked = true;
                else
                    HPGLcheckBox.Checked = false;
                // passes
                byte passes = 3;
                if (!byte.TryParse(ini.GetKeyValue("Parameter", "PassesKupfer"), out passes))
                    // Fehler
                    decPassesKupfer = 1;
                else
                    decPassesKupfer = passes;
                if (!byte.TryParse(ini.GetKeyValue("Parameter", "PassesUmriss"), out passes))
                    // Fehler
                    decPassesUmriss = 5;
                else
                    decPassesUmriss = passes;
                decPassesBohrung = decPassesKupfer;
                decPassesStencil = decPassesKupfer;
                if (selectedRadioButton == rBKupfer)
                {
                    passNumericUpDown.Value = decPassesKupfer;
                    depthtextBox.Text = strDepthKupfer;
                    deltanumericUpDown.Enabled = cBNoCopper.Checked;
                    deltaCNTnumericUpDown.Enabled = cBNoCopper.Checked;
                }
                if (selectedRadioButton == rBUmriss)
                {
                    passNumericUpDown.Value = decPassesUmriss;
                    depthtextBox.Text = strDepthUmriss;
                    deltanumericUpDown.Enabled = false;
                    deltaCNTnumericUpDown.Enabled = false;
                }
                if (selectedRadioButton == rBBohrung)
                {
                    passNumericUpDown.Value = decPassesBohrung;
                    depthtextBox.Text = strDepthBohrung;
                    deltanumericUpDown.Enabled = false;
                    deltaCNTnumericUpDown.Enabled = false;
                }
                if (selectedRadioButton == rBStencil)
                {
                    passNumericUpDown.Value = decPassesStencil;
                    depthtextBox.Text = strDepthStencil;
                    deltanumericUpDown.Enabled = false;
                    deltaCNTnumericUpDown.Enabled = false;
                }
                // precode
                /*
M03 S500
G4 P1
M03 S750
G4 P1
M03 S1000
G4 P2                 */
                byte cntLines = 0;
                if (!byte.TryParse(ini.GetKeyValue("PreCodes", "LineCount"), out cntLines))
                    return;
                for (byte cntLine = 0; cntLine < cntLines; cntLine++)
                {
                    string lineNbr = cntLine.ToString() + ":";
                    preCodetextBox.AppendText(ini.GetKeyValue("PreCodes", lineNbr));
                    preCodetextBox.AppendText("\r\n");
                }
            }
            else
            {
                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(inipath);
                ini.AddSection("PreCodes").AddKey("LineCount").Value = "0";
                // speedtextBox
                speedtextBox.Text = "80.00";
                ini.AddSection("Parameter").AddKey("Speed").Value = speedtextBox.Text;
                // hpglUnitsPerMMtextBox
                hpglUnitsPerMMtextBox.Text = "40.00";
                ini.AddSection("Parameter").AddKey("HPGLUnitsPerMM").Value = hpglUnitsPerMMtextBox.Text;
                // depthtextBox
                depthtextBox.Text = strDepthKupfer;
                ini.AddSection("Parameter").AddKey("DepthKupfer").Value = strDepthKupfer; // depthtextBox.Text;
                ini.AddSection("Parameter").AddKey("DepthUmriss").Value = strDepthUmriss; // depthtextBox.Text;
                ini.AddSection("Parameter").AddKey("DepthBohrung").Value = strDepthBohrung; // depthtextBox.Text;
                ini.AddSection("Parameter").AddKey("DepthStencil").Value = strDepthStencil; // depthtextBox.Text;
                ini.AddSection("Parameter").AddKey("DeltaNoCopper").Value = strDeltaNoCopper; // DeltaNoCopper
                ini.AddSection("Parameter").AddKey("CNTDeltaNoCopper").Value = strCNTDeltaNoCopper; // DeltaNoCopper
                // heighttextBox
                heighttextBox.Text = "1.0000";
                ini.AddSection("Parameter").AddKey("Height").Value = heighttextBox.Text;
                // HPGLcheckBox
                HPGLcheckBox.Checked = false;
                ini.AddSection("Parameter").AddKey("HPGL").Value = "0";
                // passes
                passNumericUpDown.Value = decPassesKupfer;
                ini.AddSection("Parameter").AddKey("PassesKupfer").Value = decPassesKupfer.ToString(); // passNumericUpDown.Value.ToString();
                ini.AddSection("Parameter").AddKey("PassUmriss").Value = decPassesUmriss.ToString(); // passNumericUpDown.Value.ToString();
                // RadioButton
                if (selectedRadioButton == rBKupfer)
                {
                    ini.AddSection("Parameter").AddKey("RadioButton").Value = "Kupfer";
                }
                if (selectedRadioButton == rBBohrung)
                {
                    ini.AddSection("Parameter").AddKey("RadioButton").Value = "Bohrung";
                }
                if (selectedRadioButton == rBUmriss)
                {
                    ini.AddSection("Parameter").AddKey("RadioButton").Value = "Umriss";
                }
                if (selectedRadioButton == rBStencil)
                {
                    ini.AddSection("Parameter").AddKey("RadioButton").Value = "Stencil";
                }
                if (cBNoCopper.Checked)
                {
                    ini.AddSection("Parameter").AddKey("CheckBox").Value = "KeinKupfer";
                }
            }
            this.Load += Form1_Load;
        }

        private void checkRadioButton(RadioButton btn)
        {
            selectedRadioButton = btn;
            rBKupfer.Checked = false;
            rBBohrung.Checked = false;
            rBUmriss.Checked = false;
            rBStencil.Checked = false;
            cBNoCopper.Checked = false;
            cBNoCopper.Enabled = false;

            if (btn == rBKupfer)
            {
                rBKupfer.Checked = true;
                cBNoCopper.Enabled = true;
            }
            else
            if (btn == rBBohrung)
            {
                rBBohrung.Checked = true;
            }
            else
            if (btn == rBUmriss)
            {
                rBUmriss.Checked = true;
            }
            else
            if (btn == rBStencil)
            {
                rBStencil.Checked = true;
            }
        }

        private void FillTxtBxOut()
        {
            // gcode
            gcodeBoxOut.AppendText("%Generated by HPGL2GCODE\r\n");
            gcodeBoxOut.AppendText("(Start)\r\n");
            foreach (string preCode in preCodetextBox.Lines)
            {
                if (preCode != "")
                {
                    gcodeBoxOut.AppendText(preCode);
                    gcodeBoxOut.AppendText("\r\n");
                }
            }
            // svg
            /*
<?xml version="1.0" standalone="no"?>
<!DOCTYPE svg PUBLIC "-//W3C//DTD SVG 1.1//EN"
  "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd">
             */
            svgBoxOut.AppendText("<?xml version = \"1.0\" standalone = \"no\"?>\r\n");
            svgBoxOut.AppendText("<!DOCTYPE svg PUBLIC \" -//W3C//DTD SVG 1.1//EN\"\r\n");
            svgBoxOut.AppendText("\"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">\r\n");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 
            FillTxtBxOut();
        }

        void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb == null)
            {
                MessageBox.Show("Sender is not a CheckBox");
                return;
            }
            deltanumericUpDown.Enabled = cb.Checked;
            deltaCNTnumericUpDown.Enabled = cb.Checked;
        }

        void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb == null)
            {
                MessageBox.Show("Sender is not a RadioButton");
                return;
            }

            // Ensure that the RadioButton.Checked property
            // changed to true.
            if (rb.Checked)
            {
                // Keep track of the selected RadioButton by saving a reference
                // to it.
                selectedRadioButton = rb;
                if (rb == rBKupfer)
                {
                    passNumericUpDown.Value = decPassesKupfer;
                    depthtextBox.Text = strDepthKupfer;
                    deltanumericUpDown.Enabled = false;
                    deltaCNTnumericUpDown.Enabled = false;
                    cBNoCopper.Enabled = true;
                    if (cBNoCopper.Checked)
                    {
                        passNumericUpDown.Value = decPassesKupfer;
                        depthtextBox.Text = strDepthKupfer;
                        deltanumericUpDown.Value = decimal.Parse(strDeltaNoCopper);
                        deltanumericUpDown.Enabled = true;
                        deltaCNTnumericUpDown.Value = decimal.Parse(strCNTDeltaNoCopper);
                        deltaCNTnumericUpDown.Enabled = true;
                    }
                }
                if (rb == rBBohrung)
                {
                    passNumericUpDown.Value = decPassesBohrung;
                    depthtextBox.Text = strDepthBohrung;
                    deltanumericUpDown.Enabled = false;
                    deltaCNTnumericUpDown.Enabled = false;
                    cBNoCopper.Enabled = false;
                }
                if (rb == rBUmriss)
                {
                    passNumericUpDown.Value = decPassesUmriss;
                    depthtextBox.Text = strDepthUmriss;
                    deltanumericUpDown.Enabled = false;
                    deltaCNTnumericUpDown.Enabled = false;
                    cBNoCopper.Enabled = false;
                }
                if (rb == rBStencil)
                {
                    passNumericUpDown.Value = decPassesStencil;
                    depthtextBox.Text = strDepthStencil;
                    deltanumericUpDown.Enabled = false;
                    deltaCNTnumericUpDown.Enabled = false;
                    cBNoCopper.Enabled = false;
                }
            }
            else
            {
                if (rb == rBKupfer)
                {
                    decPassesKupfer = passNumericUpDown.Value;
                    strDepthKupfer = depthtextBox.Text;
                    cBNoCopper.Enabled = false;
                    if (cBNoCopper.Checked)
                    {
                        decPassesKupfer = passNumericUpDown.Value;
                        strDepthKupfer = depthtextBox.Text;
                        strDeltaNoCopper = deltanumericUpDown.Value.ToString();
                        deltanumericUpDown.Enabled = false;
                        strCNTDeltaNoCopper = deltaCNTnumericUpDown.Value.ToString();
                        deltaCNTnumericUpDown.Enabled = false;
                    }
                }
                if (rb == rBBohrung)
                {
                    decPassesBohrung = passNumericUpDown.Value;
                    strDepthBohrung = depthtextBox.Text;
                }
                if (rb == rBUmriss)
                {
                    decPassesUmriss = passNumericUpDown.Value;
                    strDepthUmriss = depthtextBox.Text;
                }
                if (rb == rBStencil)
                {
                    decPassesStencil = passNumericUpDown.Value;
                    strDepthStencil = depthtextBox.Text;
                }
            }
        }

        private void errorFunct(string hpgl, int hpglL)
        {
            errorNbr++;
            // nur den ersten Fehler festhalten
            if (error)
                return;
            error = true;
            errorText = hpgl;
            errorLine = hpglL;
        }

        private float dist2points(PointF p0, PointF p1)
        {
            float X = p1.X - p0.X;
            float Y = p1.Y - p0.Y;
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        private void add1Point2AllPolygons(float px, float py)
        {
            int l = allPolygons.Length;
            if (px > maxX)
                maxX = px;
            if (py > maxY)
                maxY = px;
            allPolygons[l - 1].X = px;
            allPolygons[l - 1].Y = py;
            Array.Resize(ref allPolygons, l + 1);
        }

        private string hpgl2gcode(string hpgl, int hpglLine, double depth, outputType type)
        {
            string gcode = string.Empty;
            string svg = string.Empty;
            const string nbrFormat = "0.0000";
            const string errorStrng = "E R R O R";
            if (hpgl == "")
                return gcode;
            // Convert a C# string to a char array  
            float HpglUnitsPerMM = float.Parse(hpglUnitsPerMMtextBox.Text.Replace(".", ","));
            char[] chars = hpgl.ToCharArray();
            gcode = errorStrng;
            /*
                Kommando	Bedeutung
                PA	Position absolute (Stift zu absoluten Koordinaten bewegen)
                PR	Position relative (Stift um Anzahl von Einheiten bewegen)
                PD	Pen down (Stift senken)
                PU	Pen up (Stift heben)
                SP	Select pen (Stift auswählen)
                IN  Initialize HP-GL/2-mode
                PT
            SVG
            M = moveto
            L = lineto
            H = horizontal lineto
            V = vertical lineto
            C = curveto
            S = smooth curveto
            Q = quadratic Bézier curve
            T = smooth quadratic Bézier curveto
            A = elliptical Arc
            Z = closepath

            */
            switch (chars[0])
            {
                case 'P':
                    switch (chars[1])
                    {
                        case 'A':
                            string coord = hpgl.Substring(2, hpgl.Length - 2);
                            coord = coord.Remove(coord.Length - 1);
                            string[] coords = coord.Split(',');
                            float x = float.Parse(coords[0]) / HpglUnitsPerMM;
                            string X = x.ToString(nbrFormat);
                            X = X.Replace(',', '.');
                            float y = float.Parse(coords[1]) / HpglUnitsPerMM;
                            string Y = y.ToString(nbrFormat);
                            Y = Y.Replace(',', '.');
                            if (spindle == spindleStatus.dontknow)
                                spindle = spindleStatus.up;
                            if (spindle == spindleStatus.down)
                            {
                                gcode = "G01 X" + X + "Y" + Y;
                                svg = "L" + X + " " + Y + " ";
                                if (cBNoCopper.Checked)
                                // Behandlung nur bei KeinKupfer
                                {
                                    // das Array um einen Eintrag vergrößern
                                    Array.Resize(ref outlineArr, outlineArrNbr + 1);
                                    // die Koordinaten eines Umlaufes merken
                                    outlineArr[outlineArrNbr].oldValue.X = x;
                                    outlineArr[outlineArrNbr].oldValue.Y = y;
                                    PointF p0 = new PointF();
                                    p0.X = 0;
                                    p0.Y = 0;
                                    outlineArr[outlineArrNbr].distance = dist2points(p0, outlineArr[outlineArrNbr].oldValue);
                                    // den Zeiger auf die aktuelle Zeile im Array
                                    // um eins weiter schieben
                                    outlineArrNbr++;
                                }
                            }
                            if (spindle == spindleStatus.up)
                            {
                                gcode = "G00 X" + X + "Y" + Y;
                                svg = "M" + X + " " + Y + " ";
                            }
                            add1Point2AllPolygons(x, y);
                            break;
                        case 'R':
                            errorFunct(hpgl, hpglLine);
                            gcode = "(Position relative)";
                            break;
                        case 'D':
                            spindle = spindleStatus.down;
                            gcode = "G01 Z" + depth.ToString(nbrFormat).Replace(",", ".");
                            break;
                        case 'U':
                            spindle = spindleStatus.up;
                            gcode = "G00 Z" + heighttextBox.Text.Replace(",", ".");
                            add1Point2AllPolygons(-1f, -1f);
                            break;
                        case 'T':
                            // Eingabesystem metrisch [mm] G21
                            // G17 X-Y  Ebene (modal, Grundzustand)
                            // Absolutmaßeingabe G90
                            // G80 Cancel Canned Cycle
                            //       gcode = "(PT0)\r\nG21\r\n(G17)\r\nG90\r\n(G80)\r\n";
                            // Vorschub pro Minute
                            // Vorschub
                            gcode = "(PT0)\r\nG21\r\nG90\r\nG94\r\n" + "F" + speedtextBox.Text.Replace(",", ".");
                            break;
                        default:
                            errorFunct(hpgl, hpglLine);
                            gcode = "(ERROR)";
                            break;
                    }
                    break;
                case 'S':
                    if (chars[1] == 'P')
                    {
                        if (chars[2] == '0')
                        {
                            // M03: Spindel Ein: Im Uhrzeigersinn (Rechtslauf)
                            // G04: Verweilzeit (Pause)
                            // G00: Im Eilgang eine Position mit den Vorschub Achsen anfahren
                            gcode = "(SP0)\r\nM03\r\nG4 P1";
                        }
                        if (chars[2] == '2')
                        {
                            // M03: Spindel Ein: Im Uhrzeigersinn (Rechtslauf)
                            // G04: Verweilzeit (Pause)
                            // G00: Im Eilgang eine Position mit den Vorschub Achsen anfahren
                            gcode = "(SP2)\r\nM03\r\nG4 P1";
                        }
                        if (chars[2] == '3')
                        {
                            // M03: Spindel Ein: Im Uhrzeigersinn (Rechtslauf)
                            // G04: Verweilzeit (Pause)
                            // G00: Im Eilgang eine Position mit den Vorschub Achsen anfahren
                            gcode = "(SP3)\r\nM03\r\nG4 P1";
                        }
                        if (chars[2] == '4')
                        {
                            // M03: Spindel Ein: Im Uhrzeigersinn (Rechtslauf)
                            // G04: Verweilzeit (Pause)
                            // G00: Im Eilgang eine Position mit den Vorschub Achsen anfahren
                            gcode = "(SP4 - Bohrer einsetzen)\r\nM03\r\nG4 P1";
                        }
                        if (chars[2] == '5')
                        {
                            // ab hier wird der folgende Code gespeichert, um bei Passes > 0 wieder abgespielt zu werden.
                            if (firstLine2Repeat == 0)
                                firstLine2Repeat = hpglLine;
                            gcode = "(SP5)";
                        }
                    }
                    break;
                case 'I':
                    if (chars[1] == 'N')
                        gcode = "(Initialize HP-GL/2-mode)";
                    break;
                case ';':
                    gcode = "";
                    break;
                default:
                    errorFunct(hpgl, hpglLine);
                    gcode = "(ERROR)";
                    break;
            }
            if (gcode == errorStrng)
                errorNbr++;
            string result;
            switch (type)
            {
                case outputType.svg:
                    result = svg;
                    break;
                case outputType.gcode:
                    result = gcode;
                    break;
                case outputType.dontknow:
                    result = errorStrng;
                    break;
                default:
                    result = errorStrng;
                    break;
            }
            return result;
        }

        private string[] getRecentPoint(inlineStruct oneLine)
        {
            string[] recentPoint = new string[2];
            recentPoint[0] = "";
            recentPoint[1] = "";
            if (oneLine.used && oneLine.hpgl.Length > 4)
            {
                if (oneLine.hpgl.Substring(0, 2) == "PA")
                {
                    string coord = oneLine.hpgl.Substring(2, oneLine.hpgl.Length - 2);
                    coord = coord.Remove(coord.Length - 1);
                    recentPoint = coord.Split(',');
                }
            }
            return recentPoint;
        }

        private void readButton_Click(object sender, EventArgs e)
        {
            string hpgltxt;
            int cntLines = 0;
            bool isPenUp = true;
            // Step 1: initialize array for example.
            inlineStruct[] inlineArr = new inlineStruct[1];
            try
            {
                Cursor = Cursors.WaitCursor;
                //Code ausführen  
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "plt files (*.plt)|*.plt|All files (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        //Get the path of specified file
                        filePath = openFileDialog.FileName;
                        int strLng = filePath.Length;
                        const int strMaxLng = 40;
                        if (strLng > strMaxLng)
                            labFileName.Text = "... " + filePath.Substring(strLng - strMaxLng);
                        else
                            labFileName.Text = filePath;
                        string lineIn;
                        string x0 = "";
                        string y0 = "";
                        string x1 = "";
                        string y1 = "";
                        string x2 = "";
                        string y2 = "";
                        bool pointsInline;
                        bool lineInNotDouble = true; ;
                        int inlines = 0;
                        int outlines = 0;
                        // Datei einlesen und zwischenspeichern
                        hpgltxt = File.ReadAllText(filePath, System.Text.Encoding.Default);
                        string[] hpglLines = hpgltxt.Split(';');
                        cntLines = hpglLines.Length;
                        // Datei einlesen und zwischenspeichern
                        while (inlines < cntLines)
                        {
                            lineIn = hpglLines[inlines] + ";";
                            lineIn = lineIn.Replace("\r", "");
                            lineIn = lineIn.Replace("\n", "");
                            if (lineIn.StartsWith("PU"))
                            {
                                if (lineIn.Contains(","))
                                {
                                    inlineArr[outlines].used = false;
                                    // Dupletten erkennen
                                    inlineArr[outlines].hpgl = "PU;";
                                    inlineArr[outlines].used = true;
                                    outlines++;
                                    Array.Resize(ref inlineArr, outlines + 1);
                                    lineIn = lineIn.Replace("PU", "PA");
                                    isPenUp = true;
                                }
                            }
                            if (lineIn.StartsWith("PD"))
                            {
                                if (lineIn.Contains(","))
                                {
                                    if (isPenUp)
                                    {
                                        inlineArr[outlines].used = false;
                                        // Dupletten erkennen
                                        inlineArr[outlines].hpgl = "PD;";
                                        inlineArr[outlines].used = true;
                                        outlines++;
                                        Array.Resize(ref inlineArr, outlines + 1);
                                    }
                                    lineIn = lineIn.Replace("PD", "PA");
                                    isPenUp = false;
                                }
                            }
                            inlineArr[outlines].used = false;
                            // Dupletten erkennen
                            if (outlines > 0)
                                lineInNotDouble = inlineArr[outlines - 1].hpgl != lineIn;
                            if (lineInNotDouble)
                            {
                                inlineArr[outlines].hpgl = lineIn;
                                inlineArr[outlines].used = true;
                                outlines++;
                                Array.Resize(ref inlineArr, outlines + 1);
                            }
                            inlines++;
                        }
                        outlines = 0;
                        bool startLine = false;
                        // array durchsuchen nach Punkten auf einer Linie
                        foreach (inlineStruct oneLine in inlineArr)
                        {
                            pointsInline = false;
                            string[] coords = getRecentPoint(inlineArr[outlines]);
                            // Untersuchung startet erst mit dem ersten regulären "PA"
                            if (!startLine)
                            {
                                startLine = coords[0] != "" && coords[1] != "";
                            }
                            if (startLine)
                            {
                                // wenn 3mal hintereinander der gleiche Wert für
                                // x oder y auftaucht, kann der mittlere (der 2.) Eintrag
                                // gelöscht werden.
                                x0 = x1;
                                y0 = y1;
                                x1 = x2;
                                y1 = y2;
                                x2 = coords[0];
                                y2 = coords[1];
                                pointsInline = ((x0 == x1) && (x1 == x2) && (x2 == x0)) || ((y0 == y1) && (y1 == y2) && (y2 == y0));
                            }
                            // Duplikate entfernen
                            if (pointsInline)
                                inlineArr[outlines - 1].used = false;
                            outlines++;
                        }
                        foreach (inlineStruct oneLine in inlineArr)
                        {
                            if (oneLine.used)
                                textBoxIn.AppendText(oneLine.hpgl + "\r\n");
                        }
                        convertGCODE.Enabled = true;
                        convertSVG.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                //Fehlerbehandlung
                Trace.WriteLine(ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void findLeftDownCorner()
        {
            float dist = float.MaxValue;
            // den kleinsten Y-Wert finden
            for (int line = outlineDesc.firstLineOutlineArr; line <= outlineDesc.lastLineOutlineArr; line++)
            {
                // wenn der aktuelle Entfernung zum Nullpunkt kleiner als der bisher kleinste ist,
                if (outlineArr[line].distance <= dist)
                {
                    // dann ist der aktuelle der neue kleinste
                    dist = outlineArr[line].distance;
                    outlineDesc.MinValueLine = line;
                }
            }
        }

        private void reallocateArray()
        {
            int cnt = outlineDesc.MinValueLine - outlineDesc.firstLineOutlineArr;
            for (int x = 0; x < cnt; x++)
            {
                outlineTmp = outlineArr[outlineDesc.firstLineOutlineArr];
                for (int line = outlineDesc.firstLineOutlineArr; line < outlineDesc.lastLineOutlineArr; line++)
                    outlineArr[line] = outlineArr[line + 1];
                outlineArr[outlineDesc.lastLineOutlineArr] = outlineTmp;
            }
        }

        private string add1InnerLine(int line, double singleDepth)
        {
            const string nbrFormat = "0.0000";
            string cmd = "";
            int l = line;
            if (line == outlineDesc.lastLineOutlineArr + 1)
                // zurück zum Anfang
                line = outlineDesc.firstLineOutlineArr;
            string X = outlineArr[line].newValue.X.ToString(nbrFormat);
            X = X.Replace(',', '.');
            string Y = outlineArr[line].newValue.Y.ToString(nbrFormat);
            Y = Y.Replace(',', '.');
            switch (l)
            {
                case 0:
                    add1Point2AllPolygons(-1.0f, -1.0f);
                    cmd = "g00 Z" + heighttextBox.Text.Replace(",", ".") + "\r\n";
                    cmd += "g00 X" + X + "Y" + Y + "\r\n";
                    add1Point2AllPolygons(outlineArr[line].newValue.X, outlineArr[line].newValue.Y);
                    cmd += "g01 Z" + singleDepth.ToString(nbrFormat).Replace(",", ".");
                    break;
                default:
                    cmd = "g01 X" + X + "Y" + Y;
                    add1Point2AllPolygons(outlineArr[line].newValue.X, outlineArr[line].newValue.Y);
                    break;
            }
            return cmd;
        }

        private void findNextPoint(int delta)
        {
            float deltaNoCopper = float.Parse(deltanumericUpDown.Text.Replace(".", ","));
            int line = 0;
            foreach (outlineStruct outline in outlineArr)
            {
                polygon.CurrentPoint.X = outline.oldValue.X;
                polygon.CurrentPoint.Y = outline.oldValue.Y;
                polygon.Points.Add(polygon.CurrentPoint);
                outlineArr[line].newValue.X = -1;
                outlineArr[line].newValue.Y = -1;
                line++;
            }
            List<PointF> enlarged_points = polygon.GetEnlargedPolygon(polygon.Points, deltaNoCopper * (delta + 1));
            line = 0;
            foreach (PointF newPoint in enlarged_points)
            {
                outlineArr[line].newValue.X = newPoint.X;
                outlineArr[line].newValue.Y = newPoint.Y;
                line++;
            }
            polygon.Points.Clear();
        }

        private void addInnerLines(int delta, double singleDepth)
        {
            findNextPoint(delta);
            for (int line = outlineDesc.firstLineOutlineArr; line <= outlineDesc.lastLineOutlineArr; line++)
            {
                if (outlineArr[line].newValue.X != -1)
                    gcodeBoxOut.AppendText(add1InnerLine(line, singleDepth) + "\r\n");
            }
            gcodeBoxOut.AppendText(add1InnerLine(outlineDesc.lastLineOutlineArr + 1, singleDepth) + "\r\n");
        }

        private void convertGCODE_Click(object sender, EventArgs e)
        {
            Array.Resize(ref allPolygons, 1);
            outlineDesc.firstLineOutlineArr = int.MaxValue;
            int lineNbr = 1;
            int lineNbrEntire = 1;
            firstLine2Repeat = 0;
            double entireDepth = double.Parse(depthtextBox.Text.Replace(".", ","));
            int passes = (int)passNumericUpDown.Value;
            double singleDepth = entireDepth / passes;

            string lineIn;
            string lineOut;
            int pass = passes;
            int entireNbr = textBoxIn.Lines.Length;
            polygon.CurrentPoint = new PointF();
            polygon.Points = new List<PointF>();
            while (pass > 0)
            {
                for (int line = 0; line < entireNbr; line++)
                {
                    lineIn = textBoxIn.Lines[line];
                    if (cBNoCopper.Checked && selectedRadioButton == rBKupfer)
                    {
                        if (lineIn.Contains("PD"))
                        {
                            // nächste Zeile ist die erste Zeile dieses Konstruktes
                            outlineArrNbr = 0;
                            outlineDesc.firstLineOutlineArr = outlineArrNbr;
                        }
                        if (lineIn.Contains("PU"))
                        {
                            // das erste Vorkommen von PU ist nicht unbedingt das Ende eines Konstruktes
                            // das Ende eines Konstruktes liegt nur vor bei einer Zeilennummer
                            // des Konstruktbeginns kleiner als das MaxoldValue.x
                            if (outlineDesc.firstLineOutlineArr < int.MaxValue)
                            {
                                // vorige Zeile ist die letzte Zeile dieses Konstruktes
                                outlineDesc.lastLineOutlineArr = outlineArrNbr - 1;
                                findLeftDownCorner();
                                if (outlineDesc.MinValueLine != outlineDesc.firstLineOutlineArr)
                                    reallocateArray();
                                for (int i = 0; i < deltaCNTnumericUpDown.Value; i++)
                                {
                                    addInnerLines(i, singleDepth);
                                    lineNbrEntire += outlineDesc.lastLineOutlineArr - 1;
                                }
                                // points2Draw
                                Array.Clear(outlineArr, 0, outlineArr.Length);
                            }
                        }
                    }
                    lineOut = hpgl2gcode(lineIn, lineNbr, singleDepth * (passes - pass + 1), outputType.gcode);
                    if (lineNbr >= firstLine2Repeat)
                    {
                        if (lineOut != "")
                        {
                            if (HPGLcheckBox.Checked)
                                gcodeBoxOut.AppendText("(" + lineIn + ")\r\n");
                            gcodeBoxOut.AppendText(lineOut + "\r\n");
                        }
                    }
                    lineNbr++;
                    lineNbrEntire++;
                }
                pass--;
                lineNbr = 1;
            }
            gcodeBoxOut.AppendText("(Ende)\r\nG80\r\nG90\r\nG00 Z10 X0 Y0\r\nM05\r\nM09\r\nM30");
            writeGCODE.Enabled = true;
            clearButton.Enabled = true;
            bShowNC.Enabled = true;
            convertGCODE.Enabled = false;
            saveNeccessary = true;
            lineNbrEntire--;
            if (errorNbr == 0)
                error = false;
            string messageOK = lineNbrEntire.ToString() + " Zeilen übersetzt" + Environment.NewLine + errorNbr.ToString() + " Fehler";
            string messageNotOK = messageOK + ": " + errorText + " Zeile: " + errorLine.ToString();
            string caption = "Result";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            if (error)
                MessageBox.Show(this, messageNotOK, caption, buttons);
            else
                MessageBox.Show(this, messageOK, caption, buttons);
        }

        private void convertSVG_Click(object sender, EventArgs e)
        {
            int lineNbrEntire = 1;

            string lineIn;
            string lineOut;
            int entireNbr = textBoxIn.Lines.Length;
            /*
        <svg xmlns="http://www.w3.org/2000/svg" height="210" width="400">
        <title>svgOutput</title>
        <desc>made with HPLG2GCODE</desc>
        <path d="M150 0 L75 200 L225 200 Z" />
        </svg>
            */
            svgBoxOut.AppendText("<svg xmlns = \"http://www.w3.org/2000/svg\" height = \"210\" width = \"400\"> \r\n");
            svgBoxOut.AppendText("<title> svgOutput </title> \r\n");
            svgBoxOut.AppendText("<desc> made with HPLG2GCODE </desc> \r\n");
            svgBoxOut.AppendText("<path d=\"");
            for (int line = 0; line < entireNbr; line++)
            {
                lineIn = textBoxIn.Lines[line];
                lineOut = hpgl2gcode(lineIn, lineNbrEntire, 0.0, outputType.svg);
                if (lineOut != "")
                {
                    svgBoxOut.AppendText(lineOut);
                }
                lineNbrEntire++;
            }
            svgBoxOut.AppendText(" Z\" /> \r\n");
            svgBoxOut.AppendText("</svg> \r\n");
            writeSVG.Enabled = true;
            clearButton.Enabled = true;
            bShowNC.Enabled = true;
            saveNeccessary = true;
            lineNbrEntire--;
            string messageOK = lineNbrEntire.ToString() + " Zeilen übersetzt" + Environment.NewLine + errorNbr.ToString() + " Fehler";
            string messageNotOK = messageOK + ": " + errorText + " Zeile: " + errorLine.ToString();
            string caption = "Result";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            if (error)
                MessageBox.Show(this, messageNotOK, caption, buttons);
            else
                MessageBox.Show(this, messageOK, caption, buttons);
        }

        private void writeSVG_Click(object sender, EventArgs e)
        {
            DialogResult result;
            string message = "Überschreiben?";
            string caption = "Die Datei existiert";
            MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
            string writerFile = filePath.Replace(".plt", ".svg");
            if (File.Exists(writerFile))
            {
                result = MessageBox.Show(this, message, caption, buttons);

                if (result == DialogResult.No)
                {
                    return;
                }
            }
            System.IO.File.WriteAllText(writerFile, this.svgBoxOut.Text);
            saveNeccessary = false;
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            textBoxIn.Clear();
            gcodeBoxOut.Clear();
            svgBoxOut.Clear();
            FillTxtBxOut();
            errorLine = 0;
            errorNbr = 0;
            firstLine2Repeat = 0;
            spindle = spindleStatus.dontknow;
            labFileName.Text = "";
            writeGCODE.Enabled = false;
            bShowNC.Enabled = false;
            convertGCODE.Enabled = false;
            saveNeccessary = false;
        }

        private void writeGCODE_Click(object sender, EventArgs e)
        {
            DialogResult result;
            string message = "Überschreiben?";
            string caption = "Die Datei existiert";
            MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
            string writerFile = filePath.Replace(".plt", ".nc");
            if (File.Exists(writerFile))
            {
                result = MessageBox.Show(this, message, caption, buttons);

                if (result == DialogResult.No)
                {
                    return;
                }
            }
            System.IO.File.WriteAllText(writerFile, this.gcodeBoxOut.Text);
            saveNeccessary = false;
        }

        private void finish_Click(object sender, EventArgs e)
        {
            DialogResult result;
            string message = "Zurück zum Speichern?";
            string caption = "Datei nicht gespeichert";
            MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel;
            if (saveNeccessary)
            {
                result = MessageBox.Show(this, message, caption, buttons);
                if (result == DialogResult.Yes)
                    return;
            }
            ini.RemoveAllSections();
            // speedtextBox
            ini.AddSection("Parameter").AddKey("Speed").Value = speedtextBox.Text;
            // hpglUnitsPerMMtextBox
            ini.AddSection("Parameter").AddKey("HPGLUnitsPerMM").Value = hpglUnitsPerMMtextBox.Text;
            // depthtextBox
            ini.AddSection("Parameter").AddKey("DepthKupfer").Value = strDepthKupfer; // depthtextBox.Text;
            ini.AddSection("Parameter").AddKey("DepthUmriss").Value = strDepthUmriss; // depthtextBox.Text;
            ini.AddSection("Parameter").AddKey("DepthBohrung").Value = strDepthBohrung; // depthtextBox.Text;
            ini.AddSection("Parameter").AddKey("DepthStencil").Value = strDepthStencil; // depthtextBox.Text;
            ini.AddSection("Parameter").AddKey("DeltaNoCopper").Value = strDeltaNoCopper; // DeltaNoCopper
            ini.AddSection("Parameter").AddKey("CNTDeltaNoCopper").Value = strCNTDeltaNoCopper; // DeltaNoCopper
                                                                                                // heighttextBox
            ini.AddSection("Parameter").AddKey("Height").Value = heighttextBox.Text;
            if (HPGLcheckBox.Checked)
                ini.AddSection("Parameter").AddKey("HPGL").Value = "1";
            else
                ini.AddSection("Parameter").AddKey("HPGL").Value = "0";
            // passes
            ini.AddSection("Parameter").AddKey("PassesKupfer").Value = decPassesKupfer.ToString(); // passNumericUpDown.Value.ToString();
            ini.AddSection("Parameter").AddKey("PassUmriss").Value = decPassesUmriss.ToString(); // passNumericUpDown.Value.ToString();
                                                                                                 // RadioButton
            if (selectedRadioButton == rBKupfer)
            {
                ini.AddSection("Parameter").AddKey("RadioButton").Value = "Kupfer";
            }
            if (selectedRadioButton == rBBohrung)
            {
                ini.AddSection("Parameter").AddKey("RadioButton").Value = "Bohrung";
            }
            if (selectedRadioButton == rBUmriss)
            {
                ini.AddSection("Parameter").AddKey("RadioButton").Value = "Umriss";
            }
            if (selectedRadioButton == rBStencil)
            {
                ini.AddSection("Parameter").AddKey("RadioButton").Value = "Stencil";
            }
            if (cBNoCopper.Checked)
            {
                ini.AddSection("Parameter").AddKey("CheckBox").Value = "KeinKupfer";
            }
            // PreCodes
            byte cntLine = 0;
            string lineNbr;
            foreach (string preCode in preCodetextBox.Lines)
            {
                if (preCode != "")
                {
                    lineNbr = cntLine.ToString() + ":";
                    preCodetextBox.AppendText(ini.GetKeyValue("PreCode", lineNbr));
                    ini.AddSection("PreCodes").AddKey(lineNbr).Value = preCode;
                    cntLine++;
                }
            }
            ini.AddSection("PreCodes").AddKey("LineCount").Value = cntLine.ToString();
            //Save the INI
            ini.Save(string.Concat(inipath, ininame));
            this.Close();
        }

        private void bShowNC_Click(object sender, EventArgs e)
        {
            int Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            NCView GrapWindow = new NCView();
            GrapWindow.Height = Height - 100;
            GrapWindow.Width = (int)((float)GrapWindow.Height * (float)Width / (float)Height);
            float factorH = GrapWindow.Height / maxY;
            float factorW = GrapWindow.Width / maxX;

            if (factorH > factorW)
                factor = factorW;
            else
                factor = factorH;
            factor *= 0.9f;
            GrapWindow.Show();
        }
    }
}
