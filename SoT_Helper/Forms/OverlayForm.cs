using SoT_Helper.Models;
using SoT_Helper.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SoT_Helper.Extensions;
using Timer = System.Windows.Forms.Timer;
using System.Diagnostics;

namespace SoT_Helper.Forms
{
    public partial class OverlayForm : Form
    {
        private Timer timer;

        protected override CreateParams CreateParams
        {
            get
            {
                var parms = base.CreateParams;
                parms.ExStyle |= 0x20;  // WS_EX_TRANSPARENT
                return parms;
            }
        }

        public OverlayForm()
        {
            InitializeComponent();
            // Set form properties for transparency
            this.FormBorderStyle = FormBorderStyle.None;
            Color color = Color.FromArgb(2, 2, 2);
            this.BackColor = color;
            this.TransparencyKey = color;
            this.Opacity = 1.0; // You can adjust this value, 1.0 is fully visible, 0.0 is fully transparent
            this.TopMost = true; // Make the form always on top
            this.DoubleBuffered = true;
            ProcessUtils.TryGetProcessMainWindow("SoTGame", out IntPtr windowHandle, out var SOT_WINDOW_W, out var SOT_WINDOW_H, out int x, out int y);
            // Set form size and location
            this.Size = new Size(SOT_WINDOW_W, SOT_WINDOW_H); // Width = 800, Height = 600
            this.Location = new Point(x, y); // Position = 100 pixels from the top-left corner of the screen
            //TextSize = CharmService.TextSize + 2;

            //// Add a button to close the form
            //Button closeButton = new Button();
            //closeButton.Text = "Close";
            //closeButton.Click += (sender, e) => { this.Close(); };

            //this.Controls.Add(closeButton);
            timer = new Timer();
            timer.Interval = 16;  // 16 milliseconds ~= 60 FPS
            timer.Tick += Timer_Tick;
            timer.Start();
            ShowOverlay = true;

            TextSize = int.Parse(ConfigurationManager.AppSettings["TextSize"]);

            if (Updater == null)
                Updater = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (NextActorsReadTime < DateTime.Now)
                        {
                            await Task.Run(async () =>
                            {
                                try
                                {
                                    await SoT_Tool.ReadAllActors();
                                }
                                catch (Exception ex)
                                {
                                    var id = SoT_Tool.DebugActorId;
                                    var raw = SoT_Tool.DebugRawName;
                                    SoT_Tool.ReadingAllActors = false;

                                    SoT_DataManager.InfoLog += $"\nException throw: {ex.Message}";
                                    SoT_DataManager.InfoLog += " Object rawname:" + raw;
                                }
                            });

                            NextActorsReadTime = DateTime.Now.AddMilliseconds(msBetweenFullUpdates);
                            if (ServerChange)
                            {
                                ServerChange = false;
                                SoT_Tool.FullReset();
                            }
                        }
                    }
                });

            //if (Instance == null)
            //{
            //    if (!Process.GetProcessesByName("SoTGame").Any())
            //    {
            //        //MessageBox.Show("Sea of Thieves is not running!");
            //        SoT_DataManager.InfoLog += "\nSea of Thieves is not running!";
            //        return;
            //    }
            //    Instance = new Charm();
            //    if (bool.Parse(ConfigurationManager.AppSettings["OverlayFPS"]))
            //        Instance.CharmSetOptions(Charm.CharmSettings.CHARM_FONT_ARIAL | Charm.CharmSettings.CHARM_DRAW_FPS);
            //    else
            //        Instance.CharmSetOptions(Charm.CharmSettings.CHARM_FONT_ARIAL);

            //    Instance.CharmInit(RenderLoop, "SoTGame");
            //}
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.Invalidate();  // Will trigger the OnPaint event
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ProcessUtils.TryGetProcessMainWindow("SoTGame", out IntPtr windowHandle, out var SOT_WINDOW_W, out var SOT_WINDOW_H, out int x, out int y);
            // Set form size and location
            this.Size = new Size(SOT_WINDOW_W, SOT_WINDOW_H); // Width = 800, Height = 600
            this.Location = new Point(x, y); // Position = 100 pixels from the top-left corner of the screen

            //e.Graphics.Clear(Color.Transparent);
            base.OnPaint(e);
            //e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            //e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            RenderLoop(e);
        }

        public static OverlayForm Instance { get; set; }
        private static Task Updater { get; set; }
        private static PaintEventArgs _renderer { get; set; }

        private static DateTime NextActorsReadTime { get; set; }
        private static DateTime NextOverlayUpdateTime { get; set; }

        private static int msBetweenFullUpdates { get; set; } = 2200;
        private static int msBetweenUpdates { get; set; } = 200;
        private static bool running;
        public static int TextSize { get { return CharmService.TextSize; } set { CharmService.TextSize = value; } }

        public static bool ShowOverlay { get; set; } = false;
        public static bool ServerChange { get; set; } = false;
        private static List<DisplayObject> DisplayObjects { get; set; } = new List<DisplayObject>();
        private static List<Ship> Ships { get; set; } = new List<Ship>();

        //public static void DrawOutlinedString(GraphicsPath renderer, float x, float y, string text, Color color, int size)
        //{
        //    DrawOutlinedString(renderer, (int)x, (int)y, text, color, size);
        //}

        //public static void DrawOutlinedString(PaintEventArgs e, int x, int y, string text, Color color, int size)
        //{
        //    // Draw outlined text
        //    var fontFamily = new FontFamily("Arial");
        //    var font = new Font(fontFamily, 48, FontStyle.Bold, GraphicsUnit.Pixel);
        //    var gp = new GraphicsPath();
        //    gp.AddString(text, fontFamily, (int)FontStyle.Bold, TextSize+size, new Point(x, y), StringFormat.GenericDefault);

        //    // Outline
        //    e.Graphics.DrawPath(Pens.Black, gp);

        //    SolidBrush myBrush = new SolidBrush(color);
        //    // Fill
        //    e.Graphics.FillPath(myBrush, gp);
        //}

        //public static void DrawLine(PaintEventArgs renderer, Vector2 a, Vector2 b, Color color, int thickness = 1)
        //{
        //    // Draw a blue line
        //    renderer.Graphics.DrawLine(new Pen(color, thickness), new Point((int)a.X, (int)a.Y), new Point((int)b.X, (int)b.Y));
        //    //renderer.DrawLine(a.X, a.Y, b.X, b.Y, thickness, color);
        //}
        
        public static void RenderLoop(PaintEventArgs renderer)
        {
            if (!ShowOverlay)
                return;
            //if (!ProcessUtils.TryGetProcess("SoTGame"))
            //{
            //    ShowOverlay = false;
            //    running = false;
            //    return;
            //}
            if (running)
                return;
            running = true;

            SoT_Tool.UpdateMyCoords();
            if (bool.Parse(ConfigurationManager.AppSettings["ShowCompass"]))
                Compass(renderer);

            List<DisplayObject> displayObjectsToDelete = new List<DisplayObject>();

            //renderer.DrawString(10, 20, "Charm Overlay by Coltonon", Color.Green);
            DisplayObjects = SoT_DataManager.DisplayObjects.ToList();

            foreach (var dispObj in DisplayObjects)
            {
                try
                {
                    if (NextOverlayUpdateTime > DateTime.Now || ServerChange)
                    {
                        return;
                    }
                    if (dispObj is StorageContainer && !bool.Parse(ConfigurationManager.AppSettings["ShowContainers"]))
                        continue;
                    if (dispObj is Player && !bool.Parse(ConfigurationManager.AppSettings["ShowPlayers"]))
                        continue;
                    //if(dispObj is Ship && !SoT_DataManager.Ships.Contains((Ship)dispObj))
                    //    SoT_DataManager.Ships.Add((Ship)dispObj);
                    if (dispObj is Ship && !bool.Parse(ConfigurationManager.AppSettings["ShowShips"]) && !bool.Parse(ConfigurationManager.AppSettings["ShowShipStatus"]))
                        continue;

                    dispObj.Update(SoT_Tool.my_coords);
                    if (dispObj.ToDelete)
                    {
                        //displayObjectsToDelete.Add(dispObj);
                        continue;
                    }
                    if (dispObj is Crews && !bool.Parse(ConfigurationManager.AppSettings["ShowCrews"]))
                        continue;
                    dispObj.DrawGraphics(renderer);

                    //if (dispObj.ShowIcon || dispObj.ShowText || dispObj is Ship)
                    //{
                    //    dispObj.DrawGraphics(renderer);
                    //}
                }
                catch (Exception e)
                {
                    //dispObj.ToDelete = true;
                    dispObj.ShowIcon = false;
                    dispObj.ShowText = false;
                    //displayObjectsToDelete.Add(dispObj);
                    SoT_DataManager.InfoLog += $"Error drawing {dispObj.GetType().Name}: {e.Message}\n";
                    if (!ProcessUtils.TryGetProcess("SoTGame"))
                    {
                        ShowOverlay = false;
                        running = false;
                        return;
                    }
                }
            }
            if (bool.Parse(ConfigurationManager.AppSettings["ShowCrosshair"]))
            {
                var w = SoT_Tool.SOT_WINDOW_W / 2;
                var h = SoT_Tool.SOT_WINDOW_H / 2; //20;
                renderer.Graphics.DrawLine(w - 5, h, w + 5, h, 1, Color.Red);
                renderer.Graphics.DrawLine(w, h - 5, w, h + 5, 1, Color.Red);
            }

            if (NextOverlayUpdateTime > DateTime.Now || ServerChange)
            {
                return;
            }

            if (!bool.Parse(ConfigurationManager.AppSettings["ShowShips"]) && !bool.Parse(ConfigurationManager.AppSettings["ShowShipStatus"]))
                return;
            Ships = SoT_DataManager.Ships.ToList();

            // Every ship find when reading actors is already added to the Ship list.
            //if (DisplayObjects.Any(a => a is Ship))
            //    SoT_DataManager.Ships = DisplayObjects
            //                .Where(a => a is Ship)
            //                .Select(s => (Ship)s).ToList();

            if (Ship.PlayerShip == null
                || (SoT_Tool.LocalPlayerCrewId != Guid.Empty
                && Ship.PlayerShip.CrewId != SoT_Tool.LocalPlayerCrewId)
                || Ship.PlayerShip.ToDelete)
            {
                try
                {
                    foreach (var ship in SoT_DataManager.Ships)
                    {
                        ship.ToDelete = false;
                        ship.Update(SoT_Tool.my_coords);
                        if (ship.CrewId == SoT_Tool.LocalPlayerCrewId)
                        {
                            Ship.PlayerShip = ship;
                            Ship.PlayerShip.ToDelete = false;
                            break;
                        }
                    }

                    if (SoT_Tool.LocalPlayerCrewId != null && SoT_Tool.LocalPlayerCrewId != Guid.Empty)
                    {
                        if (Ships != null && Ships.Any(s => s.CrewId == SoT_Tool.LocalPlayerCrewId))
                        {
                            Ship.PlayerShip = Ships.First(s => s.CrewId == SoT_Tool.LocalPlayerCrewId);
                        }
                    }
                    else if (Ships != null && Ships.Any())
                    {
                        var myship = Ships.OrderBy(s => s.Distance).First();
                        Ship.PlayerShip = myship;
                    }
                }
                catch (Exception ex)
                {
                    if (!ProcessUtils.TryGetProcess("SoTGame"))
                    {
                        ShowOverlay = false;
                        running = false;
                        return;
                    }
                }
            }

            if (bool.Parse(ConfigurationManager.AppSettings["ShowShipStatus"]) && DisplayObjects.Any(d => d is Ship && d.Distance < 1750))
            {
                try
                {
                    if (DisplayObjects
                        .Any(a => a is Ship && a.Distance < 1750 && !a.ToDelete
                        && ((Ship)a).CrewId != SoT_Tool.LocalPlayerCrewId))
                    {
                        int shipNo = 0;
                        var ships = DisplayObjects
                            .Where(a => a is Ship && a.Distance < 1750 && !a.ToDelete
                            && ((Ship)a).CrewId != SoT_Tool.LocalPlayerCrewId)
                            .Select(s => (Ship)s).OrderBy(s => s.ActorAddress).ToList();

                        foreach (var ship in ships)
                        {
                            ship.DrawShipStatus(renderer, shipNo);
                            shipNo++;
                        }
                    }

                    if (Ship.PlayerShip != null)
                        Ship.PlayerShip.DrawShipStatus(renderer);
                    else if (DisplayObjects.Any(a => a is Ship && a.Distance < 1750 && !a.ToDelete))
                    {
                        Ship.PlayerShip = (Ship)DisplayObjects.Where(a => a is Ship && a.Distance < 1750 && !a.ToDelete).OrderBy(s => s.Distance).First();
                    }
                    else
                    {
                        SoT_DataManager.InfoLog += "No ships found. Assuming server change.\n";
                        NextOverlayUpdateTime = DateTime.Now.AddSeconds(15);
                        ServerChange = true;
                        Ship.PlayerShip = null;
                        //SoT_Tool.FullReset();
                    }
                }
                catch
                {
                    if (!ProcessUtils.TryGetProcess("SoTGame"))
                    {
                        ShowOverlay = false;
                        running = false;
                        return;
                    }
                }
            }

            running = false;
        }

        private static void Compass(PaintEventArgs e)
        {
            var w = SoT_Tool.SOT_WINDOW_W / 2;
            var h = SoT_Tool.SOT_WINDOW_H / 90; //20;
            var direction = GetDirection(SoT_Tool.my_coords.rot_y);
            e.DrawOutlinedString(w - direction.Length * 4, h, direction, Color.Yellow, 0);
            string directionNumber = ((int)SoT_Tool.my_coords.rot_y + 180).ToString();
            int currentRotation = (int)SoT_Tool.my_coords.rot_y + 180;
            e.DrawOutlinedString(w - directionNumber.Length * 4, h + 15
                , directionNumber, Color.Yellow, -2);
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public static string GetDirection(float y)
        {
            if (y >= -22.5f && y < 22.5f)
            {
                return "E";
            }
            else if (y >= 22.5f && y < 67.5f)
            {
                return "SE";
            }
            else if (y >= 67.5f && y < 112.5f)
            {
                return "S";
            }
            else if (y >= 112.5f && y < 157.5f)
            {
                return "SW";
            }
            else if ((y >= 157.5f && y <= 180.0f) || (y >= -180.0f && y < -157.5f))
            {
                return "W";
            }
            else if (y >= -157.5f && y < -112.5f)
            {
                return "NW";
            }
            else if (y >= -112.5f && y < -67.5f)
            {
                return "N";
            }
            else if (y >= -67.5f && y < -22.5f)
            {
                return "NE";
            }
            else
            {
                return "Unknown";
            }
        }

        public static string GetDirectionText(float rotation)
        {
            if (rotation >= 337.5 || rotation < 22.5)
                return "N";
            else if (rotation >= 22.5 && rotation < 67.5)
                return "NE";
            else if (rotation >= 67.5 && rotation < 112.5)
                return "E";
            else if (rotation >= 112.5 && rotation < 157.5)
                return "SE";
            else if (rotation >= 157.5 && rotation < 202.5)
                return "S";
            else if (rotation >= 202.5 && rotation < 247.5)
                return "SW";
            else if (rotation >= 247.5 && rotation < 292.5)
                return "W";
            else
                return "NW";
        }
    }
}
