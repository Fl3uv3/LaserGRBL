//*** Macro definition ******************************************************
namespace CRXGerber
{
    using LaserGRBL;
    using System;
    //using Cyotek.Drawing;
    //using SharpGL.SceneGraph;
    //using SharpGL.SceneGraph.Primitives;
    //using SharpGL.SceneGraph.Raytracing;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Globalization;
    using System.IO;
    using System.Management;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Contexts;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    //using System.Windows;

    //*** Define ***************************************************************

    //*** Class definition ******************************************************
    public class CRXGerber
    {
        //private bool m_IsLeading;
        //private bool m_IsTrailing;
        //private bool m_IsDecimal;
        //private bool m_IsAbsolute;

        private float[] m_FormatRatio;
        private bool m_IsInch;

        //private bool m_IsLinear;
        //private bool m_IsClockWise;
        //private bool m_IsCounterClockWise;
        //private bool m_Is360ON;
        //private bool m_Is360OFF;
        //private bool m_IsEOF;
        private float m_MaxX;
        private float m_MaxY;
        private float m_MinX;
        private float m_MinY;
        private float m_SizeX;
        private float m_SizeY;
        private Color m_ClearColor;
        private Color m_FlashColor;
        private float m_Dpi;
        private float m_PixelUnitRatio;

        private List<string> m_PolygonLayer;
        private List<string> m_ClearLayer;
        private List<string> m_DarkLayer;
        private List<string> m_MacroApperture;
        private List<string> m_Apperture;

        public Bitmap m_FinalImg;
        public PictureBox m_GuiImage;

        //***************************************************************************
        // Output         : 
        // Class name     : CRXGerberES
        // Function name  : CRXGerberES
        // Description    : CONSTRUCTEUR
        // Input          : string Filename
        //***************************************************************************
        public CRXGerber(string GerberFileName, PictureBox ImageGui = null)
        {
            m_GuiImage = ImageGui;
            //m_IsLeading = false;
            //m_IsTrailing = false;
            //m_IsDecimal = false;
            //m_IsAbsolute = true;
            m_FormatRatio = new float[3];
            m_FormatRatio[0] = 0.0f;
            m_FormatRatio[1] = 0.0f;
            m_FormatRatio[2] = 0.0f;
            m_IsInch = false;
            //m_IsLinear = false;
            //m_IsClockWise = false;
            //m_IsCounterClockWise = false;
            //m_Is360ON = false;
            //m_Is360OFF = false;
            //m_IsEOF = false;
            m_MaxX = -999999;
            m_MaxY = -999999;
            m_MinX = 999999;
            m_MinY = 999999;
            m_SizeX = 0;
            m_SizeY = 0;
            m_Dpi = 1000.0f;
            m_PixelUnitRatio = 1000.0f;

            m_PolygonLayer = new List<string>();
            m_DarkLayer = new List<string>();
            m_ClearLayer = new List<string>();
            m_MacroApperture = new List<string>();
            m_Apperture = new List<string>();

            List<string> CurrentList = m_PolygonLayer;

            float x = 0;
            float y = 0;
            int code = 0;
            int CurrentApperture = 0;

            if (GerberFileName != "")
            {
                try
                {
                    string[] lines = File.ReadAllLines(GerberFileName);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string Cmd = lines[i];
                        if (Cmd.Contains("%FS"))
                        {
                            for (int j = 3; j < Cmd.Length; j++)
                            {
                                /*if (Cmd[j] == 'L')
									m_IsLeading = true;

								if (Cmd[j] == 'T')
									m_IsTrailing = true;

								if (Cmd[j] == 'D')
									m_IsDecimal = true;
								
								if (Cmd[j] == 'A')
									m_IsAbsolute = true;

								if (Cmd[j] == 'I')
                                    m_IsAbsolute = false;
								*/
                                if (Cmd[j] == 'X' || Cmd[j] == 'Y' || Cmd[j] == 'Z')
                                {
                                    if (Cmd[j + 1] == '2')
                                        m_FormatRatio[Cmd[j] - 0x58] = Cmd[j + 2] switch
                                        {
                                            '3' => 1.0f,
                                            '4' => 0.1f,
                                            '5' => 0.01f,
                                            '6' => 0.001f,
                                            _ => 0.0f
                                        };
                                    if (Cmd[j + 1] == '4')
                                        m_FormatRatio[Cmd[j] - 0x58] = Cmd[j + 2] switch
                                        {
                                            '2' => 0.01f,
                                            '3' => 0.001f,
                                            '4' => 0.0001f,
                                            '5' => 0.00001f,
                                            '6' => 0.000001f,
                                            _ => 0.0f
                                        };
                                    j += 2;
                                }
                            }
                        }

                        if (Cmd == "G70*" || Cmd == "%MOIN*%")
                        {
                            m_IsInch = true;
                            m_PixelUnitRatio = 1000.0f;
                        }
                        if (Cmd == "G71*" || Cmd == "%MOMM*%")
                        {
                            m_IsInch = false;
                            m_PixelUnitRatio = 39.37f;
                        }
                        /*if (Cmd == "G01*")
							m_IsLinear = true;
						if (Cmd == "G02*" || Cmd == "G20*" || Cmd == "G21*")
							m_IsClockWise = true;
						if (Cmd == "G03*" || Cmd == "G30*" || Cmd == "G31*")
							m_IsCounterClockWise = true;
						if (Cmd == "G75*")
							m_Is360ON = true;
						if (Cmd == "G74*")
							m_Is360OFF = true;
						if (Cmd == "G90*")
							m_IsAbsolute = true;
						if (Cmd == "G91*")
							m_IsIncremental = true;
						*/
                        if (Cmd == "%LPC*%")
                            CurrentList = m_ClearLayer;

                        if (Cmd == "%LPD*%")
                            CurrentList = m_DarkLayer;

                        if (Cmd == "G36*")
                            CurrentList.Add("G36");

                        if (Cmd == "G37*")
                            CurrentList.Add("G37");

                        if (Cmd.Contains("%AM"))
                        {
                            Regex regx = new Regex(@"%AM(.*)\*");
                            Match match = regx.Match(Cmd);
                            string MacroName = "";

                            if (match.Success)
                                MacroName = match.Groups[1].Value;

                            string TmpAppertureMacro = MacroName + "\n";

                            do
                            {
                                i++;
                                string TmpBuffer = lines[i];
                                TmpBuffer = TmpBuffer.TrimEnd('%');
                                TmpBuffer = TmpBuffer.TrimEnd('*');
                                TmpAppertureMacro += TmpBuffer + "\n";
                            }
                            while (!lines[i].Contains("%"));

                            TmpAppertureMacro = TmpAppertureMacro.TrimEnd('\n');
                            m_MacroApperture.Add(TmpAppertureMacro);
                        }

                        if (Cmd.Contains("%AD"))
                        {
                            char AppLetter = ' ';
                            int AppNum = -1;
                            string AppertureName = " ";
                            string Param = " ";
                            float[] Params = new float[0];
                            float Sx = 0.0f;
                            float Sy = 0.0f;
                            float HoleSx = 0.0f;
                            float HoleSy = 0.0f;
                            int NbSide = 0;
                            string TmpAppertureMacro = "";

                            //%AD<AppLetter><AppNum><Shape>,
                            Regex regx = new Regex(@"^%AD([A-Z])(\d+)([A-Za-z0-9]+)(?:,([^*]+))?\*%$");
                            Match match = regx.Match(Cmd);

                            if (match.Success)
                            {
                                AppLetter = match.Groups[1].Value[0];
                                AppNum = int.Parse(match.Groups[2].Value);
                                AppertureName = match.Groups[3].Value;
                                Param = match.Groups[4].Value;
                                if (Param.Length != 0)
                                    Params = Array.ConvertAll(Param.Split('X'), float.Parse);
                            }
                            if (AppertureName.Length == 1)
                            {
                                if (AppertureName[0] == 'C')
                                {
                                    if (Params.Length >= 1)
                                        Sx = Params[0];

                                    TmpAppertureMacro += string.Format("1,1,{0},0,0\n", Sx);

                                    if (Params.Length >= 2)
                                        HoleSx = Params[1];

                                    if (Params.Length >= 3)
                                        HoleSy = Params[2];
                                }

                                if (AppertureName[0] == 'R' || AppertureName[0] == 'O')
                                {
                                    if (Params.Length >= 2)
                                    {
                                        Sx = Params[0];
                                        Sy = Params[1];
                                    }

                                    if (AppertureName[0] == 'R')
                                        TmpAppertureMacro += string.Format("21,1,{0},{1},0,0,0.0\n", Sx, Sy);
                                    else
                                    {
                                        float d = 0.0f;
                                        float PosX = 0.0f;
                                        float PosY = 0.0f;
                                        if (Sx < Sy)
                                        {
                                            d = Sx;
                                            Sy -= d;
                                            PosY = Sy / 2;
                                        }
                                        else
                                        {
                                            d = Sy;
                                            Sx -= d;
                                            PosX = Sx / 2;
                                        }

                                        TmpAppertureMacro += string.Format("21,1,{0},{1},0,0,0.0\n", Sx, Sy);
                                        TmpAppertureMacro += string.Format("1,1,{0},{1},{2}\n", d, -PosX, -PosY);
                                        TmpAppertureMacro += string.Format("1,1,{0},{1},{2}\n", d, PosX, PosY);
                                    }

                                    if (Params.Length >= 3)
                                        HoleSx = Params[2];

                                    if (Params.Length >= 4)
                                        HoleSy = Params[3];

                                }

                                if (AppertureName[0] == 'P')
                                {
                                    if (Params.Length >= 2)
                                    {
                                        Sx = Params[0];
                                        NbSide = (int)Params[1];
                                        float Radius = 0.04f;
                                        PointF Center = new PointF(20, 20);

                                        PointF[] polygonPoints = CreateRegularPolygonPoints(Center, NbSide, Radius, 0.0f);
                                    }
                                }

                                if (HoleSy != 0.0f)
                                    TmpAppertureMacro += string.Format("21,0,{0},{1},0,0,0.0\n", HoleSx, HoleSy);
                                else if (HoleSx != 0.0f)
                                    TmpAppertureMacro += string.Format("1,0,{0},0,0\n", HoleSx);
                            }
                            else
                            {
                                for (int MacroId = 0; MacroId < m_MacroApperture.Count; MacroId++)
                                {
                                    if (m_MacroApperture[MacroId].Contains(AppertureName))
                                    {
                                        TmpAppertureMacro = Regex.Replace(m_MacroApperture[MacroId], @"^.*\n", "");
                                        MacroId = m_MacroApperture.Count;
                                    }
                                }

                                for (int MacroParam = 0; MacroParam < Params.Length; MacroParam++)
                                {
                                    string MacroName = string.Format("${0}", MacroParam + 1);
                                    TmpAppertureMacro = TmpAppertureMacro.Replace(MacroName, Params[MacroParam].ToString());
                                }

                            }

                            if (TmpAppertureMacro != "")
                            {
                                TmpAppertureMacro = TmpAppertureMacro.TrimEnd('\n');
                                string Buffer = string.Format("{0}{1} -> {2}", AppLetter, AppNum, TmpAppertureMacro);
                                m_Apperture.Add(Buffer);
                            }
                        }

                        //if (Cmd == "M02*")
                        //m_IsEOF = true;

                        if (Cmd[0] == 'X' || Cmd.Contains("D03"))
                        {
                            string Buffer = "";

                            //X<xx>Y<yy>D<code>
                            Regex regx = new Regex(@"X(-?\d+)Y(-?\d+)D(-?\d+)");
                            Match match = regx.Match(Cmd);

                            if (match.Success)
                            {
                                x = float.Parse(match.Groups[1].Value) * m_FormatRatio[0];
                                y = float.Parse(match.Groups[2].Value) * m_FormatRatio[1];
                                code = int.Parse(match.Groups[3].Value);

                                if (!m_IsInch)
                                {
                                    x *= m_PixelUnitRatio;
                                    y *= m_PixelUnitRatio;
                                }

                            }

                            if (code == 1)
                                Buffer = string.Format("   T-> D{0} ({1}, {2})", CurrentApperture, x, y);

                            if (code == 2)
                                Buffer = string.Format("   M-> D2 ({0}, {1})", x, y);

                            if (Cmd.Contains("D03"))
                                Buffer = string.Format("   F-> D{0} ({1}, {2})", CurrentApperture, x, y);

                            CurrentList.Add(Buffer);

                            if (x > m_MaxX)
                                m_MaxX = x;
                            if (y > m_MaxY)
                                m_MaxY = y;
                            if (x < m_MinX)
                                m_MinX = x;
                            if (y < m_MinY)
                                m_MinY = y;
                        }
                        else if (Cmd[0] == 'D')
                        {
                            //D<Apperture>
                            Regex regx = new Regex(@"D(-?\d+)");
                            Match match = regx.Match(Cmd);

                            CurrentApperture = int.Parse(match.Groups[1].Value);
                        }
                    }
                }
                catch (IOException) { }

                m_SizeX = m_MaxX - m_MinX;
                m_SizeY = m_MaxY - m_MinY;
            }

            ProcessLayer();
        }


        //***************************************************************************
        // Output         : void
        // Class name     : CRXGerberES
        // Function name  : Release
        // Description    : Release Device Dependent Resources
        //***************************************************************************
        ~CRXGerber()
        {
        }

        public void ProcessLayer()
        {
            m_FinalImg = new Bitmap((int)m_SizeX, (int)m_SizeY);
            m_FinalImg.SetResolution(m_Dpi, m_Dpi);
            if (m_GuiImage != null)
                m_GuiImage.Image = m_FinalImg;

            //CRXGerberForm GerberConfig = new CRXGerberForm();
            //GerberConfig.ShowDialog();

            Graphics g = Graphics.FromImage(m_FinalImg);
            m_ClearColor = Color.Black;
            m_FlashColor = Color.White;

            g.Clear(m_ClearColor);

            DrawLayer(m_PolygonLayer, false, true);
            DrawLayer(m_ClearLayer, true);
            DrawLayer(m_DarkLayer, false, true);

            //m_FinalImg.Save("E:\\Final.png");
        }
        private void DrawLayer(List<string> DCodeList, bool Invert, bool Flashpart = false)
        {
            List<PointF> Polygon = new List<PointF>();

            float x = 0, y = 0, xx = 0, yy = 0;
            int DrawTools = -1;
            char Func = '?';
            int PolMode = 0, First = 0;

            for (int i = 0; i < DCodeList.Count; i++)
            {
                string Cmd = DCodeList[i];

                if (Cmd == "G36")
                {
                    Polygon.Clear();
                    First = 1;
                    PolMode = 1;
                }

                if (Cmd == "G37")
                {
                    if (Polygon.Count != 0)
                        DrawFillPoly(Polygon, Invert);
                    First = 0;
                    PolMode = 0;
                }

                if (Cmd[0] == ' ')
                {
                    //<Func> -> D<ToolNum> (<X>, <Y>)
                    Regex regx = new Regex(@"^\s*(.)-> D(-?\d+) \((-?[0-9]+(?:\.[0-9]+)?),\s*(-?[0-9]+(?:\.[0-9]+)?)\)");
                    Match match = regx.Match(Cmd);

                    if (match.Success)
                    {
                        Func = match.Groups[1].Value[0];
                        DrawTools = int.Parse(match.Groups[2].Value);
                        xx = float.Parse(match.Groups[3].Value) - m_MinX;
                        yy = m_SizeY - (float.Parse(match.Groups[4].Value) - m_MinY);
                    }

                    if (Func == 'M')
                    {
                        if (PolMode == 1 && First == 1)
                            Polygon.Add(new PointF(xx, yy));

                        x = xx;
                        y = yy;
                    }

                    if (Func == 'F')
                    {
                        int Aperture = 0;
                        string Param = "";

                        for (int j = 0; j < m_Apperture.Count; j++)
                        {
                            string TmpStr = m_Apperture[j];

                            //D<Apperture> -> <Param>
                            regx = new Regex(@"D(-?\d+) -> ([\s\S]*)");
                            match = regx.Match(TmpStr);

                            if (match.Success)
                            {
                                Aperture = int.Parse(match.Groups[1].Value);
                                Param = match.Groups[2].Value;
                            }

                            if (Aperture == DrawTools & Flashpart)
                            {
                                string[] Multiline = Param.Split('\n');

                                for (int LineIndex = 0; LineIndex < Multiline.Length; LineIndex++)
                                {
                                    string FinalLine = "";

                                    if (Multiline[LineIndex][0] != '0')
                                    {
                                        string[] SplitEval = Multiline[LineIndex].Split(',');

                                        for (int SplitElem = 0; SplitElem < SplitEval.Length; SplitElem++)
                                            FinalLine += ApertureMacroEval(SplitEval[SplitElem]) + ",";

                                        FinalLine = FinalLine.Trim(',');
                                        Flash(xx, yy, FinalLine);
                                    }
                                }

                                j = m_Apperture.Count;
                            }
                        }
                    }

                    if (Func == 'T')
                    {
                        First = 0;
                        if (PolMode == 1)
                            Polygon.Add(new PointF(xx, yy));
                        else
                        {
                            float Size = 1.0f;

                            if (Flashpart)
                            {
                                for (int j = 0; j < m_Apperture.Count; j++)
                                {
                                    string TmpStr = m_Apperture[j];

                                    //D<Apperture> -> <Param>
                                    regx = new Regex(@"D(-?\d+) -> (.*)");
                                    match = regx.Match(TmpStr);

                                    if (match.Success)
                                    {
                                        if (int.Parse(match.Groups[1].Value) == DrawTools)
                                        {
                                            string Param = match.Groups[2].Value;
                                            string[] Params = Param.Split(',');
                                            float[] fParams = Array.ConvertAll(Params, float.Parse);

                                            Size = fParams[2] * m_PixelUnitRatio;
                                            j = m_Apperture.Count;
                                        }
                                    }
                                }
                            }

                            Trace(x, y, xx, yy, Invert, Size);

                            x = xx;
                            y = yy;
                        }
                    }
                }
            }
        }

        private void DrawFillPoly(List<PointF> Polygon, bool Invert)
        {
            Color Col = Invert ? m_ClearColor : m_FlashColor;
            Graphics g = Graphics.FromImage(m_FinalImg);
            Brush brush = new SolidBrush(Col);

            g.FillPolygon(brush, Polygon.ToArray());
        }

        private void Trace(float x, float y, float xx, float yy, bool Invert, float Size)
        {
            Color Col = Invert ? m_ClearColor : m_FlashColor;
            Graphics g = Graphics.FromImage(m_FinalImg);
            Brush brush = new SolidBrush(Col);
            Pen pen = new Pen(Col, Size);

            float Ray = Size / 2;

            g.FillEllipse(brush, x - Ray, y - Ray, Size, Size);
            g.DrawLine(pen, x, y, xx, yy);
            g.FillEllipse(brush, xx - Ray, yy - Ray, Size, Size);
        }

        private void Flash(float X, float Y, string Param)
        {
            //string[] Params = Param.Split(',');
            float[] fParams = Array.ConvertAll(Param.Split(','), float.Parse);

            Color Col = fParams[1] == 0.0f ? m_ClearColor : m_FlashColor;

            Graphics g = Graphics.FromImage(m_FinalImg);
            Brush brush = new SolidBrush(Col);

            if (fParams[0] == 1)
            {
                float Sx = fParams[2] * m_PixelUnitRatio;
                float Ray = (float)Math.Ceiling(Sx / 2);
                float OffsetX = fParams[3] * m_PixelUnitRatio;
                float OffsetY = fParams[4] * m_PixelUnitRatio;
                g.FillEllipse(brush, X - Ray + OffsetX, Y - Ray + OffsetY, Sx, Sx);
            }
            else if (fParams[0] == 2 || fParams[0] == 20)
            {
                float LineWidth = fParams[2];
                PointF Start = new PointF(fParams[3], fParams[4]);
                PointF End = new PointF(fParams[5], fParams[6]);

                Pen pen = new Pen(Col, LineWidth);
                GraphicsState State = g.Save();

                g.TranslateTransform(X, Y);
                g.ScaleTransform(m_PixelUnitRatio, m_PixelUnitRatio);
                g.RotateTransform(fParams[7]);

                g.DrawLine(pen, Start, End);
                g.Restore(State);
            }
            else if (fParams[0] == 21)
            {
                float Sx = fParams[2] * m_PixelUnitRatio;
                float RayX = Sx / 2;

                float Sy = fParams[3] * m_PixelUnitRatio;
                float RayY = Sy / 2;

                GraphicsState State = g.Save();
                RectangleF rect = new RectangleF(-RayX, -RayY, Sx, Sy);
                g.TranslateTransform(X, Y);

                g.RotateTransform(fParams[6]);
                g.FillRectangle(brush, rect);

                g.Restore(State);
            }
            else if (fParams[0] == 4)
            {
                List<PointF> Poly = new List<PointF>();

                for (int i = 0; i < (fParams[2] + 1) * 2; i += 2)
                    Poly.Add(new PointF(fParams[3 + i], fParams[4 + i]));

                GraphicsState State = g.Save();
                g.TranslateTransform(X, Y);
                g.ScaleTransform(m_PixelUnitRatio, m_PixelUnitRatio);
                g.RotateTransform(fParams[fParams.Length - 1]);

                g.FillPolygon(brush, Poly.ToArray());
                g.Restore(State);
            }
        }
        private PointF[] CreateRegularPolygonPoints(PointF Center, int sides, float radius, float angle = 0.0f)
        {
            PointF[] points = new PointF[sides];
            float angleStep = (float)(2 * Math.PI / sides);

            for (int i = 0; i < sides; i++)
            {
                points[i] = new PointF(Center.X + radius * (float)Math.Cos(angle), Center.Y + radius * (float)Math.Sin(angle));
                angle += angleStep;
            }

            return points;
        }

        string ApertureMacroEval(string expr)
        {
            Stack<float> valeurs = new Stack<float>();
            Stack<char> operateurs = new Stack<char>();

            for (int i = 0; i < expr.Length; i++)
            {
                char c = expr[i];

                if (c == '-' && i + 1 < expr.Length && expr[i + 1] == '(')
                {
                    valeurs.Push(0);
                    operateurs.Push('-');
                }
                else if (char.IsDigit(c) || c == '.' || (c == '-' && IsNegNumber(expr, i)))
                {
                    string nombre = "";

                    if (c == '-')
                    {
                        nombre += '-';
                        i++;
                    }

                    while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.'))
                        nombre += expr[i++];

                    i--;
                    valeurs.Push(float.Parse(nombre, CultureInfo.InvariantCulture));
                }
                else if (c == '(')
                    operateurs.Push(c);
                else if (c == ')')
                {
                    while (operateurs.Peek() != '(')
                        valeurs.Push(ApplyOperation(valeurs.Pop(), valeurs.Pop(), operateurs.Pop()));
                    operateurs.Pop();
                }
                else if ("+-*/".Contains(c.ToString()))
                {
                    while (operateurs.Count > 0 && Priority(operateurs.Peek()) >= Priority(c))
                        valeurs.Push(ApplyOperation(valeurs.Pop(), valeurs.Pop(), operateurs.Pop()));
                    operateurs.Push(c);
                }
            }

            while (operateurs.Count > 0)
                valeurs.Push(ApplyOperation(valeurs.Pop(), valeurs.Pop(), operateurs.Pop()));

            float resultat = valeurs.Pop();

            valeurs.Clear();
            operateurs.Clear();

            return resultat.ToString();
        }

        static float ApplyOperation(float a, float b, char op)
        {
            return op switch
            {
                '+' => b + a,
                '-' => b - a,
                '*' => b * a,
                '/' => a == 0 ? throw new DivideByZeroException("Division by zero") : b / a,
                _ => throw new ArgumentException("Invalid Operation")
            };
        }

        static int Priority(char op)
        {
            return op switch
            {
                '+' => 1,
                '-' => 1,
                '*' => 2,
                '/' => 2,
                _ => 0
            };
        }

        bool IsNegNumber(string expr, int index)
        {
            if (expr[index] != '-') return false;
            if (index == 0) return true;

            char prev = expr[index - 1];
            return prev == '(' || "+-*/".Contains(prev.ToString());
        }

    };
}
//*** End macro definition *************************************************


