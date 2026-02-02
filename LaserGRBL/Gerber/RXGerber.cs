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
    public enum Resolution { RatioNone = 0, Ratio23 = 3, Ratio24 = 4, Ratio25 = 5 };

    //*** Class definition ******************************************************
    public class CRXGerber
    {
        //private bool m_IsLeading;
        //private bool m_IsTrailing;
        //private bool m_IsDecimal;
        //private bool m_IsAbsolute;
        //private bool m_IsIncremental;
        private Resolution[] m_Format;
        //private bool m_IsMils;
        //private bool m_IsMM;
        //private bool m_IsLinear;
        //private bool m_IsClockWise;
        //private bool m_IsCounterClockWise;
        //private bool m_Is360ON;
        //private bool m_Is360OFF;
        //private bool m_IsEOF;
        private int m_MaxX;
        private int m_MaxY;
        private int m_MinX;
        private int m_MinY;
        private int m_SizeX;
        private int m_SizeY;
        private Color m_BackColor;
        private Color m_ForeColor;
        private float m_Dpi = 1000.0f;

        private List<string> m_ClearLayer;
        private List<string> m_PolygonLayer;
        private List<string> m_DarkLayer;
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
            //m_IsAbsolute = false;
            //m_IsIncremental = false;
            m_Format = new Resolution[3];
            m_Format[0] = Resolution.RatioNone;
            m_Format[1] = Resolution.RatioNone;
            m_Format[2] = Resolution.RatioNone;
            //m_IsMils = false;
            //m_IsMM = false;
            //m_IsLinear = false;
            //m_IsClockWise = false;
            //m_IsCounterClockWise = false;
            //m_Is360ON = false;
            //m_Is360OFF = false;
            //m_IsEOF = false;
            m_MaxX = 0;
            m_MaxY = 0;
            m_MinX = 999999;
            m_MinY = 999999;
            m_SizeX = 0;
            m_SizeY = 0;

            m_PolygonLayer = new List<string>();
            m_DarkLayer = new List<string>();
            m_ClearLayer = new List<string>();
            m_Apperture = new List<string>();

            List<string> CurrentList = m_PolygonLayer;

            int x = 0;
            int y = 0;
            int xx = 0;
            int yy = 0;
            int code = 0;
            int CurrentApperture = 0;
            List<string> m_TmpAppertureMacro = new List<string>();
            string MacroName = "";

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
									m_IsIncremental = true;
								*/
                                if (Cmd[j] == 'X' || Cmd[j] == 'Y' || Cmd[j] == 'Z')
                                {
                                    m_Format[Cmd[j] - 0x58] = (Resolution)(Cmd[j + 2] - 0x30);
                                    j += 2;
                                }
                            }
                        }

                        /*
						if (Cmd == "G70*" || Cmd == "%MOIN*%")
							m_IsMils = true;
						if (Cmd == "G71*" || Cmd == "%MOMM*%")
							m_IsMM = true;
						if (Cmd == "G01*")
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

                            if (match.Success)
                                MacroName = match.Groups[1].Value;

                            while (lines[++i] != "%")
                                m_TmpAppertureMacro.Add(lines[i].TrimEnd('*'));
                        }


                        if (Cmd.Contains("%AD"))
                        {
                            char AppLetter = ' ';
                            int AppNum = -1;
                            char Shape = ' ';
                            float Sx = 0.0f;
                            float Sy = 0.0f;
                            float HoleSx = 0.0f;
                            float HoleSy = 0.0f;
                            int NbSide = 0;
                            string CurrentMacroName = "";

                            //%AD<AppLetter><AppNum><Shape>,
                            Regex regx = new Regex(@"%AD(.)(-?\d+)(.),");
                            Match match = regx.Match(Cmd);

                            if (match.Success)
                            {
                                AppLetter = match.Groups[1].Value[0];
                                AppNum = int.Parse(match.Groups[2].Value);
                                Shape = match.Groups[3].Value[0];
                            }
                            else
                            {
                                regx = new Regex(@"%AD(.)(-?\d+)(.*)\*");
                                match = regx.Match(Cmd);

                                if (match.Success)
                                {
                                    AppLetter = match.Groups[1].Value[0];
                                    AppNum = int.Parse(match.Groups[2].Value);
                                    CurrentMacroName = match.Groups[3].Value;
                                }
                            }
                            if (Shape != ' ')
                            {
                                string ConvertToAM = "";
                                if (Shape == 'C')
                                {
                                    //%AD<AppLetter><AppNum><Shape>,<dimx>
                                    regx = new Regex(@"%AD(.)(-?\d+)(.),(-?[0-9]+(?:\.[0-9]+)?)");
                                    match = regx.Match(Cmd);

                                    if (match.Success)
                                        Sx = float.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);

                                    ConvertToAM = string.Format("1,1,{0},0,0", Sx);
                                    m_TmpAppertureMacro.Add(ConvertToAM);

                                    //%AD<AppLetter><AppNum><Shape>,<dimx>X<HoleX>
                                    regx = new Regex(@"%AD(.)(-?\d+)(.),(-?[0-9]+(?:\.[0-9]+)?)X(-?[0-9]+(?:\.[0-9]+)?)");
                                    match = regx.Match(Cmd);

                                    if (match.Success)
                                        HoleSx = float.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);

                                    //%AD<AppLetter><AppNum><Shape>,<dimx>X<HoleX>X<HoleY>
                                    regx = new Regex(@"%AD(.)(-?\d+)(.),(-?[0-9]+(?:\.[0-9]+)?)X(-?[0-9]+(?:\.[0-9]+)?)X(-?[0-9]+(?:\.[0-9]+)?)");
                                    match = regx.Match(Cmd);

                                    if (match.Success)
                                        HoleSy = float.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);

                                }

                                if (Shape == 'R' || Shape == 'O')
                                {
                                    //%AD<AppLetter><AppNum><Shape>,<dimx>X<dimy>
                                    regx = new Regex(@"%AD(.)(-?\d+)(.),(-?[0-9]+(?:\.[0-9]+)?)X(-?[0-9]+(?:\.[0-9]+)?)");
                                    match = regx.Match(Cmd);
                                    if (match.Success)
                                    {
                                        Sx = float.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                                        Sy = float.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
                                    }

                                    if (Shape == 'R')
                                        ConvertToAM = string.Format("21,1,{0},{1},0,0,0.0", Sx, Sy);
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

                                        ConvertToAM = string.Format("21,1,{0},{1},0,0,0.0", Sx, Sy);
                                        m_TmpAppertureMacro.Add(ConvertToAM);

                                        ConvertToAM = string.Format("1,1,{0},{1},{2}", d, -PosX, -PosY);
                                        m_TmpAppertureMacro.Add(ConvertToAM);

                                        ConvertToAM = string.Format("1,1,{0},{1},{2}", d, PosX, PosY);
                                    }

                                    m_TmpAppertureMacro.Add(ConvertToAM);

                                    //%AD<AppLetter><AppNum><Shape>,<dimx>X<dimy>X<HoleX>
                                    regx = new Regex(@"%AD(.)(-?\d+)(.),(-?[0-9]+(?:\.[0-9]+)?)X(-?[0-9]+(?:\.[0-9]+)?)X(-?[0-9]+(?:\.[0-9]+)?)");
                                    match = regx.Match(Cmd);
                                    if (match.Success)
                                        HoleSx = float.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);

                                    //%AD<AppLetter><AppNum><Shape>,<dimx>X<dimy>X<HoleX>X<HoleY>
                                    regx = new Regex(@"%AD(.)(-?\d+)(.),(-?[0-9]+(?:\.[0-9]+)?)X(-?[0-9]+(?:\.[0-9]+)?)X(-?[0-9]+(?:\.[0-9]+)?)X(-?[0-9]+(?:\.[0-9]+)?)");
                                    match = regx.Match(Cmd);
                                    if (match.Success)
                                        HoleSy = float.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture);
                                }

                                if (Shape == 'P')
                                {
                                    //%AD<AppLetter><AppNum><Shape>,<Diameter>X<NbSide>
                                    regx = new Regex(@"%AD(.)(-?\d+)(.),(-?[0-9]+(?:\.[0-9]+)?)X(-?\d+)");
                                    match = regx.Match(Cmd);
                                    if (match.Success)
                                    {
                                        Sx = float.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                                        NbSide = int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
                                    }
                                }

                                if (HoleSy != 0.0f)
                                {
                                    ConvertToAM = string.Format("21,0,{0},{1},0,0,0.0", HoleSx, HoleSy);
                                    m_TmpAppertureMacro.Add(ConvertToAM);
                                }
                                else if (HoleSx != 0.0f)
                                {
                                    ConvertToAM = string.Format("1,0,{0},0,0", HoleSx);
                                    m_TmpAppertureMacro.Add(ConvertToAM);
                                }
                            }

                            if (CurrentMacroName == MacroName || Shape != ' ')
                            {
                                for (int NbLine = 0; NbLine < m_TmpAppertureMacro.Count; NbLine++)
                                {
                                    string Buffer = string.Format("{0}{1} -> {2}", AppLetter, AppNum, m_TmpAppertureMacro[NbLine]);
                                    m_Apperture.Add(Buffer);
                                }

                                m_TmpAppertureMacro.Clear();
                            }
                        }

                        //if (Cmd == "M02*")
                        //m_IsEOF = true;

                        if (Cmd[0] == 'X')
                        {
                            string Buffer = "";

                            //X<xx>Y<yy>D<code>
                            Regex regx = new Regex(@"X(-?\d+)Y(-?\d+)D(-?\d+)");
                            Match match = regx.Match(Cmd);

                            if (match.Success)
                            {
                                xx = int.Parse(match.Groups[1].Value);
                                yy = int.Parse(match.Groups[2].Value);
                                code = int.Parse(match.Groups[3].Value);

                                if (CurrentApperture <= 3)
                                    CurrentApperture = code;

                                if (code == 1)
                                    Buffer = string.Format("   T-> D{0} ({1}, {2})", CurrentApperture, xx, yy);

                                if (code == 2)
                                    Buffer = string.Format("   M-> D2 ({0}, {1})", xx, yy);

                                CurrentList.Add(Buffer);
                                x = xx;
                                y = yy;

                                if (x > m_MaxX)
                                    m_MaxX = x;
                                if (y > m_MaxY)
                                    m_MaxY = y;
                                if (x < m_MinX)
                                    m_MinX = x;
                                if (y < m_MinY)
                                    m_MinY = y;
                            }

                        }

                        if (Cmd[0] == 'D' && Cmd != "D03*")
                        {
                            //D<Apperture>
                            Regex regx = new Regex(@"D(-?\d+)");
                            Match match = regx.Match(Cmd);

                            CurrentApperture = int.Parse(match.Groups[1].Value);
                        }

                        if (Cmd == "D03*")
                        {
                            string Buffer = string.Format("   F-> D{0} ({1}, {2})", CurrentApperture, x, y);
                            CurrentList.Add(Buffer);
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
            m_FinalImg = new Bitmap(m_SizeX, m_SizeY);
            m_FinalImg.SetResolution(m_Dpi, m_Dpi);
            if (m_GuiImage != null)
                m_GuiImage.Image = m_FinalImg;

            //CRXGerberForm GerberConfig = new CRXGerberForm();
            //GerberConfig.ShowDialog();

            Graphics g = Graphics.FromImage(m_FinalImg);
            m_BackColor = Color.Black;
            m_ForeColor = Color.White;

            g.Clear(m_BackColor);

            DrawLayer(m_PolygonLayer, false);
            DrawLayer(m_ClearLayer, true);
            DrawLayer(m_DarkLayer, false, true);
        }

        private void DrawLayer(List<string> DCodeList, bool Invert, bool Flashpart = false)
        {
            List<Point> Polygon = new List<Point>();

            int x = -1, y = -1, xx = -1, yy = -1, DrawTools = -1;
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
                        xx = int.Parse(match.Groups[3].Value) - m_MinX;
                        yy = m_SizeY - (int.Parse(match.Groups[4].Value) - m_MinY);
                    }

                    if (Func == 'M')
                    {
                        if (PolMode == 1 && First == 1)
                        {
                            Polygon.Add(new Point(xx, yy));
                        }

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

                            //D<Apperture> -> <Shape> (<SizeX>, <SizeY>) H(<HoleX>, <HoleY>)
                            regx = new Regex(@"D(-?\d+) -> (.*)");
                            match = regx.Match(TmpStr);

                            if (match.Success)
                            {
                                Aperture = int.Parse(match.Groups[1].Value);
                                Param = match.Groups[2].Value;
                            }

                            if (Aperture == DrawTools & Flashpart)
                                Flash(xx, yy, Param);
                        }
                    }

                    if (Func == 'T')
                    {
                        First = 0;
                        if (PolMode == 1)
                            Polygon.Add(new Point(xx, yy));
                        else
                        {
                            float Size = 1.0f;

                            if (Flashpart)
                            {
                                for (int j = 0; j < m_Apperture.Count; j++)
                                {
                                    string TmpStr = m_Apperture[j];

                                    //D<Apperture> -> <Shape> (<SizeX>, <SizeY>)
                                    regx = new Regex(@"D(-?\d+) -> (.*)");
                                    match = regx.Match(TmpStr);

                                    if (match.Success)
                                    {
                                        if (int.Parse(match.Groups[1].Value) == DrawTools)
                                        {
                                            string Param = match.Groups[2].Value;
                                            string[] Params = Param.Split(',');
                                            float[] fParams = Array.ConvertAll(Params, float.Parse);

                                            Size = fParams[2] * m_Dpi;
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

        private void DrawFillPoly(List<Point> Polygon, bool Invert)
        {
            Color Col = Invert ? m_BackColor : m_ForeColor;
            Graphics g = Graphics.FromImage(m_FinalImg);
            Brush brush = new SolidBrush(Col);

            g.FillPolygon(brush, Polygon.ToArray());
        }

        private void Trace(float x, float y, float xx, float yy, bool Invert, float Size)
        {
            Color Col = Invert ? m_BackColor : m_ForeColor;
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
            string[] Params = Param.Split(',');
            float[] fParams = Array.ConvertAll(Params, float.Parse);

            Color Col = fParams[1] == 0.0f ? m_BackColor : m_ForeColor;

            Graphics g = Graphics.FromImage(m_FinalImg);
            Brush brush = new SolidBrush(Col);

            if (fParams[0] == 1)
            {
                float Sx = fParams[2] * m_Dpi;
                float Ray = (float)Math.Ceiling(Sx / 2);
                float OffsetX = fParams[3] * m_Dpi;
                float OffsetY = fParams[4] * m_Dpi;
                g.FillEllipse(brush, X - Ray + OffsetX, Y - Ray + OffsetY, Sx, Sx);
            }
            else if (fParams[0] == 21)
            {
                float Sx = fParams[2] * m_Dpi;
                float RayX = Sx / 2;

                float Sy = fParams[3] * m_Dpi;
                float RayY = Sy / 2;

                GraphicsState State = g.Save();
                RectangleF rect = new RectangleF(-RayX, -RayY, Sx, Sy);
                g.TranslateTransform(X, Y);

                g.RotateTransform(fParams[6]);
                g.FillRectangle(brush, rect);

                g.Restore(State);
            }
            /*
            float RayX = SizeX/2;
			float RayY = SizeY/2;
            RectangleF rect = new RectangleF(X-RayX, Y-RayY, SizeX, SizeY);
					
			if (Shape == 'P')
			{
				int Sides = 6;
				float Radius = 0.04f;
				PointF Center = new PointF(20, 20);

                PointF[] polygonPoints = CreateRegularPolygonPoints(Center, Sides, Radius, 0.0f);

                g.DrawPolygon(pen, polygonPoints);
                g.FillPolygon(brush, polygonPoints);
            }
			*/
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
    };
}
//*** End macro definition *************************************************


