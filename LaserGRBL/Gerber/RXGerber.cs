//*** Macro definition ******************************************************
namespace CRXGerber
{
    using LaserGRBL;
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
    public enum Resolution {RatioNone = 0, Ratio23 = 3, Ratio24 = 4, Ratio25 = 5};

    public partial class CRXGerberForm : Form
    {
        public CheckBox m_ChkInvert;
        private Button m_BtnOk;

        public CRXGerberForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.m_BtnOk = new System.Windows.Forms.Button();
            this.m_ChkInvert = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // m_BtnOk
            // 
            this.m_BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_BtnOk.Location = new System.Drawing.Point(12, 34);
            this.m_BtnOk.Name = "m_BtnOk";
            this.m_BtnOk.Size = new System.Drawing.Size(383, 23);
            this.m_BtnOk.TabIndex = 0;
            this.m_BtnOk.Text = "OK";
            this.m_BtnOk.UseVisualStyleBackColor = true;
			this.m_BtnOk.ForeColor = ColorScheme.FormForeColor;
			this.m_BtnOk.BackColor = ColorScheme.FormButtonsColor;
            // 
            // m_ChkInvert
            // 
            this.m_ChkInvert.AutoSize = true;
            this.m_ChkInvert.Location = new System.Drawing.Point(13, 13);
            this.m_ChkInvert.Name = "m_ChkInvert";
            this.m_ChkInvert.Size = new System.Drawing.Size(69, 17);
            this.m_ChkInvert.TabIndex = 1;
            this.m_ChkInvert.Text = "Negative";
            this.m_ChkInvert.UseVisualStyleBackColor = true;
            // 
            // CRXGerberForm
            // 
            this.BackColor = ColorScheme.FormBackColor;
            this.ForeColor = ColorScheme.FormForeColor;
            this.AcceptButton = this.m_BtnOk;
            this.ClientSize = new System.Drawing.Size(407, 69);
            this.ControlBox = false;
            this.Controls.Add(this.m_ChkInvert);
            this.Controls.Add(this.m_BtnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CRXGerberForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gerber parameter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }

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
			m_MinX = 0;
			m_MinY = 0;
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
			int MinX = 999999;
			int MinY = 999999;
			int MaxX = 0;
			int MaxY = 0;
	
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
							for (int j = 3; j < (int)Cmd.Length; j++)
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

						if (Cmd.Contains("%AD"))
						{
							char AppLetter = ' ' ;
							int AppNum = -1;
							char Shape = ' ';
							float dimx = 0.0f;
							float dimy = 0.0f;

							//%AD<AppLetter><AppNum><Shape>
							Regex regx = new Regex(@"%AD(.)(-?\d+)(.)");
							Match match = regx.Match(Cmd);

							if (match.Success)
							{
								AppLetter = match.Groups[1].Value[0];
								AppNum = int.Parse(match.Groups[2].Value);
								Shape = match.Groups[3].Value[0];	
							}

							if (Shape == 'C')
							{
								//%AD<AppLetter><AppNum><Shape>,<dimx>
								regx = new Regex(@"%AD(.)(-?\d+)(.),(-?[0-9]+(?:\.[0-9]+)?)");
								match = regx.Match(Cmd);
						
								if (match.Success)
								{
									dimx = float.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
									dimy = dimx;
								}
							}

							if (Shape == 'R' || Shape == 'O')
							{
								//%AD<AppLetter><AppNum><Shape>,<dimx>X<dimy>
								regx = new Regex(@"%AD(.)(-?\d+)(.),(-?[0-9]+(?:\.[0-9]+)?)X(-?[0-9]+(?:\.[0-9]+)?)");
								match = regx.Match(Cmd);
								if (match.Success)
								{ 
									dimx = float.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
									dimy = float.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
								}
							}

							string Buffer = string.Format("{0}{1} -> {2} ({3}, {4})", AppLetter, AppNum, Shape, dimx, dimy);
							m_Apperture.Add(Buffer);
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
				
								if (x > MaxX)
									MaxX = x;
								if (y > MaxY)
									MaxY = y;
								if (x < MinX)
									MinX = x;
								if (y < MinY)
									MinY = y;
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
				catch (IOException) {}
				
				m_MinX = MinX;
				m_MinY = MinY;
				m_MaxX = MaxX;
				m_MaxY = MaxY;
				m_SizeX = MaxX - MinX;
				m_SizeY = MaxY - MinY;
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
			//printf_s("CRXGerberES::~CRXGerberES -> %ls\n", m_Label->Str());
		}

		public void ProcessLayer()
		{
			m_FinalImg = new Bitmap(m_SizeX, m_SizeY);
			m_FinalImg.SetResolution(1000.0f, 1000.0f);
            if (m_GuiImage != null)
				m_GuiImage.Image = m_FinalImg;

			CRXGerberForm GerberConfig = new CRXGerberForm();
            GerberConfig.ShowDialog();

            Graphics g = Graphics.FromImage(m_FinalImg);
			Color Primary = GerberConfig.m_ChkInvert.Checked ? Color.Black : Color.White;
			Color Secondary = GerberConfig.m_ChkInvert.Checked ? Color.White : Color.Black;
			g.Clear(Secondary);

			DrawLayer(m_PolygonLayer, Primary);
			DrawLayer(m_ClearLayer, Secondary);
			DrawLayer(m_DarkLayer, Primary, true);
		}
		private void DrawLayer(List<string> DCodeList, Color col, bool Flashpart = false)
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
					if (Polygon.Count !=0) 
						DrawFillPoly(Polygon, col);
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
						yy = int.Parse(match.Groups[4].Value) - m_MinY;
					}
 
					if (Func == 'M')
					{
						if (PolMode == 1 && First == 1)
						{
							Polygon.Add(new Point((int)xx, (int)yy));
						}

						x = xx;
						y = yy;
					}

					if (Func == 'F')
					{
						int Aperture  = 0;
						char Shape = ' ';
						float SizeX = 0.0f;
						float SizeY = 0.0f;

						for (int j = 0; j < m_Apperture.Count; j++)
						{
							string TmpStr = m_Apperture[j];

							//D<Apperture> -> <Shape> (<SizeX>, <SizeY>)
							regx = new Regex(@"D(-?\d+) -> (.) \((-?[0-9]+(?:\.[0-9]+)?),\s*(-?[0-9]+(?:\.[0-9]+)?)\)");
							match = regx.Match(TmpStr);
                    
							if (match.Success)
							{
								Aperture = int.Parse(match.Groups[1].Value);
								Shape = match.Groups[2].Value[0];
								SizeX = float.Parse(match.Groups[3].Value);
								SizeY = float.Parse(match.Groups[4].Value);
							}

							if (Aperture == DrawTools)
							{
								j = m_Apperture.Count;
							}
						}

						if (Flashpart)
							Flash(xx, yy, Shape, (int)(SizeX * 1000), (int)(SizeY * 1000), col);
					}

					if (Func == 'T')
					{
						First = 0;
						if (PolMode == 1)
							Polygon.Add(new Point(xx, yy));
						else
						{
							float Size = 1.0f;
							int Aperture = 0;
							char Shape = ' ';
							float SizeX = 0.0f;
							float SizeY = 0.0f;

							for (int j = 0; j < m_Apperture.Count; j++)
							{
								string TmpStr = m_Apperture[j];

								//D<Apperture> -> <Shape> (<SizeX>, <SizeY>)
								regx = new Regex(@"D(-?\d+) -> (.) \((-?[0-9]+(?:\.[0-9]+)?),\s*(-?[0-9]+(?:\.[0-9]+)?)\)");
								match = regx.Match(TmpStr);

								if (match.Success)
								{
									Aperture = int.Parse(match.Groups[1].Value);
									Shape = match.Groups[2].Value[0];
									SizeX = float.Parse(match.Groups[3].Value);
									SizeY = float.Parse(match.Groups[4].Value);
								}
										
								if (Aperture == DrawTools)
								{
									Size = (int)(SizeX * 1000);
									j = m_Apperture.Count;
								}
							}

                            if (Flashpart)
                                Trace(x, y, xx, yy, col, Size);
							else
                                Trace(x, y, xx, yy, col, 1.0f);

                            x = xx;
							y = yy;
						}
					}
				}
			}
		}

		private void DrawFillPoly(List<Point> Polygon, Color col)
		{
			Graphics g = Graphics.FromImage(m_FinalImg);
			Brush brush = new SolidBrush(col);
			Pen pen = new Pen(col, 1.0f);

			g.DrawPolygon(pen, Polygon.ToArray());
			g.FillPolygon(brush, Polygon.ToArray());
        }

		private void Trace(int x, int y, int xx, int yy, Color col, float Size)
		{
			Graphics g = Graphics.FromImage(m_FinalImg);
			Brush brush = new SolidBrush(col);
			Pen pen = new Pen(col, Size);
			Pen pen1 = new Pen(col, 1.0f);

			float Ray = Size/2;

			g.DrawEllipse(pen1, x-Ray, y-Ray, Size, Size);
			g.FillEllipse(brush, x-Ray, y-Ray, Size, Size);

			g.DrawLine(pen, x, y, xx, yy);
    
			g.DrawEllipse(pen1, xx - Ray, yy - Ray, Size, Size);
			g.FillEllipse(brush, xx - Ray, yy - Ray, Size, Size);
        }
		
		private void Flash(int X, int Y, char Shape, int SizeX, int SizeY, Color col)
		{
			Graphics g = Graphics.FromImage(m_FinalImg);
			Brush brush = new SolidBrush(col);
			Pen pen = new Pen(col, 1.0f);
    
			float RayX = SizeX/2;
			float RayY = SizeY/2;
			Rectangle rect = new Rectangle(X-(int)RayX, Y-(int)RayY, SizeX, SizeY);

			if (Shape == 'C')
			{	
				g.DrawEllipse(pen, X-RayX, Y-RayY, SizeX, SizeY);
				g.FillEllipse(brush, X-RayX, Y-RayY, SizeX, SizeY);
			}

			if (Shape == 'R')
			{
				g.DrawRectangle(pen, rect);
				g.FillRectangle(brush, rect);
			}
        
			if (Shape == 'O')
			{
				GraphicsPath path = new GraphicsPath();
				int d = SizeX < SizeY ? SizeX : SizeY;

				//if (d <= 0)
					//path.AddRectangle(rect);
				//else
				{
					path.AddArc(rect.X, rect.Y, d, d, 180, 90);
					path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
					path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
					path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
					path.CloseFigure();
				}
        
				g.DrawPath(pen, path);
				g.FillPath(brush, path);
			}
        }
	};
}
//*** End macro definition *************************************************


