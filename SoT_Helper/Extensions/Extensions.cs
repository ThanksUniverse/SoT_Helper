using SoT_Helper.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static SoT_Helper.Services.Charm;

namespace SoT_Helper.Extensions
{
    public static class Extensions
    {

        //DrawBox(ScreenCoords.Value.X + Size, ScreenCoords.Value.Y + CharmService.TextSize, 50, 5,
        //            1, Color.Black, false);

        //DrawLine

        public static void DrawLine(this Graphics g, float x, float y, float x2, float y2, int thickness, Color color)
        {
            g.DrawLine(new Pen(color, thickness), x, y, x2, y2);
        }

        public static void DrawLine(this Graphics g, Vector2 start, Vector2 end, int thickness, Color color)
        {
            g.DrawLine(new Pen(color, thickness), start.X, start.Y, end.X, end.Y);
        }

        public static void DrawTraceLine(this Graphics g, Coordinates target, Color color)
        {
            var point = MathHelper.ObjectTracePointToScreen(target);
            var centerX = SoT_Tool.SOT_WINDOW_H / 2;
            var centerY = SoT_Tool.SOT_WINDOW_W / 2;
            //var center = new Vector2(centerX, centerY);
            var center = new Vector2(centerY, centerX);
            if (point != null)
                g.DrawLine(new Pen(color, 1), center.X, center.Y, point.Value.X, point.Value.Y);
                //CharmService.DrawLine(rend, center, point.Value, color);
        }

        public static void DrawBox(this Graphics g, float x, float y, float width, float height, int thickness, Color color, bool fill)
        {
            if(fill)
                g.FillRectangle(new SolidBrush(color), x, y, width, height);
            else
                g.DrawRectangle(new Pen(color, thickness), x, y, width, height);
        }

        public static void DrawOutlinedString(this PaintEventArgs e, float x, float y, string text, Color color, int size)
        {
            DrawOutlinedString(e, (int)x, (int)y, text, color, size);
        }

        public static void DrawOutlinedString(this PaintEventArgs e, int x, int y, string text, Color color, int size)
        {
            // Draw outlined text
            var fontFamily = new FontFamily("Arial");
            var font = new Font(fontFamily, CharmService.TextSize + size, FontStyle.Regular, GraphicsUnit.Pixel);

            // Create brushes
            Brush outlineBrush = Brushes.Black;
            SolidBrush textBrush = new SolidBrush(color);

            // Draw the outline by drawing the string at multiple offsets
            //int outlineWidth = 2;  // Set the width of the outline
            //for (int i = -outlineWidth; i <= outlineWidth; i++)
            //{
            //    for (int j = -outlineWidth; j <= outlineWidth; j++)
            //    {
            //        e.Graphics.DrawString(text, font, outlineBrush, x + i, y + j);
            //    }
            //}
            e.Graphics.DrawString(text, font, outlineBrush, x + 1, y + 1);
            e.Graphics.DrawString(text, font, outlineBrush, x - 1, y - 1);
            // Draw the original text on top
            e.Graphics.DrawString(text, font, textBrush, x, y);

            //// Draw outlined text
            //var fontFamily = new FontFamily("Arial");
            ////var font = new Font(fontFamily, 48, FontStyle.Bold, GraphicsUnit.Pixel);
            //var gp = new GraphicsPath();
            //gp.AddString(text, fontFamily, (int)FontStyle.Regular, CharmService.TextSize + size, new Point(x, y), StringFormat.GenericDefault);

            //// Outline
            //e.Graphics.DrawPath(Pens.Black, gp);

            //SolidBrush myBrush = new SolidBrush(color);
            //// Fill
            //e.Graphics.FillPath(myBrush, gp);
        }

        public static void DrawCircle(this Graphics g, float x, float y, int size, int thickness, Color color, bool fill)
        {
            DrawCircle(g, (int)x, (int)y, size, thickness, color, fill);
        }

        public static void DrawCircle(this Graphics g, int x, int y, int size, int thickness, Color color, bool fill)
        {
            if(fill)
                g.FillEllipse(new SolidBrush(color), x - size, y - size, size * 2, size * 2);
            else
                g.DrawEllipse(new Pen(color, thickness), x - size, y - size, size * 2, size * 2);

            //DrawCircle(ScreenCoords.Value.X, ScreenCoords.Value.Y, Size, 1, Color, true);
        }

        public static bool IsMatch(this string search, string text)
        {
            if(text.Contains("%"))
            {
                var searchText = text;
                text = search;
                search = searchText;
            }

            if (string.IsNullOrWhiteSpace(search))
            {
                return true;
            }
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }
            if (search.StartsWith("%") && search.EndsWith("%"))
            {
                return text.ToLower().Contains(search.ToLower().Replace("%", ""));
            }
            if (search.StartsWith("%"))
            {
                return text.ToLower().EndsWith(search.ToLower().Replace("%", ""));
            }
            if (search.EndsWith("%"))
            {
                return text.ToLower().StartsWith(search.ToLower().Replace("%", ""));
            }
            return text.ToLower() == search.ToLower();
        }

        public static float Magnitude(this Vector3 v)
        {
            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        }


        public static Vector3 Rotator(this Quaternion q)
        {
            var RotMatrix = MathHelper.FMatrix(q, Vector3.Zero);
            Vector3 Rotator;

            double SP = RotMatrix[0][2];
            double CP = Math.Sqrt(1 - SP * SP);
            Rotator.X = (float)(Math.Atan2(SP, CP)* 180f / Math.PI);

            if (Math.Abs(CP) < SMALL_NUMBER)
            {
                Rotator.Y = (float)(Math.Atan2(-RotMatrix[2][0], RotMatrix[1][1]) * 180f / Math.PI);
                Rotator.Z = 0f;
            }
            else
            {
                Rotator.Y = (float)(Math.Atan2(RotMatrix[0][1], RotMatrix[0][0])* 180f / Math.PI);
                Rotator.Z = (float)(Math.Atan2(RotMatrix[1][2], RotMatrix[2][2])* 180f / Math.PI);
            }

            return Rotator;
        }

        public static Vector3 Euler(this Quaternion q)
        {
            Vector3 euler;
            // roll (x-axis rotation)
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            euler.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
                euler.Y = (float)(Math.CopySign(Math.PI / 2, sinp)); // use 90 degrees if out of range
            else
                euler.Y = (float)Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            euler.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return euler;
        }

        //public Quaternion EulerToQuaternion(double pitch, double yaw, double roll)
        public static Quaternion EulerToQuaternion(this Vector3 rotation)
        {
            // Convert the Euler angles to radians
            double pitchRad = Math.PI / 180.0 * rotation.X;
            double yawRad = Math.PI / 180.0 * rotation.Y;
            double rollRad = Math.PI / 180.0 * rotation.Z;

            double cy = Math.Cos(yawRad * 0.5);
            double sy = Math.Sin(yawRad * 0.5);
            double cp = Math.Cos(pitchRad * 0.5);
            double sp = Math.Sin(pitchRad * 0.5);
            double cr = Math.Cos(rollRad * 0.5);
            double sr = Math.Sin(rollRad * 0.5);

            Quaternion q = new Quaternion();

            q.W = (float)(cy * cp * cr + sy * sp * sr);
            q.X = (float)(cy * cp * sr - sy * sp * cr);
            q.Y = (float)(sy * cp * sr + cy * sp * cr);
            q.Z = (float)(sy * cp * cr - cy * sp * sr);

            return q;
        }

        public static Vector3 Project(this Vector3 a, Vector3 b)
        {
            float m = b.Magnitude();
            return (Dot(a, b) / (m * m)) * b;
        }

        public static Vector3 ProjectOnPlane(this Vector3 a, Vector3 b)
        {
            return a - a.Project(b);
        }

        public static Quaternion Conjugate(this Quaternion q)
        {
            return new Quaternion(-q.X, -q.Y, -q.Z, q.W);
        }

        public const float SMALL_NUMBER = 1e-6f;

        public static Vector3 Forward(this Vector3 rotation)
        {
            double pitch = (rotation.X * Math.PI / 180.0);
            double yaw = (rotation.Y * Math.PI / 180.0);
            double cosPitch = Math.Cos(pitch);
            double sinPitch = Math.Sin(pitch);
            double cosYaw = Math.Cos(yaw);
            double sinYaw = Math.Sin(yaw);
            return new Vector3((float)(cosPitch * cosYaw), (float)(cosPitch * sinYaw), (float)sinPitch);
        }

        public static float Dot(this Vector3 a, Vector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static void DrawTraceLine(this Renderer rend, Coordinates target, Color color)
        {
            var point = MathHelper.ObjectTracePointToScreen(target);
            var centerX = SoT_Tool.SOT_WINDOW_H / 2;
            var centerY = SoT_Tool.SOT_WINDOW_W / 2;
            //var center = new Vector2(centerX, centerY);
            var center = new Vector2(centerY, centerX);
            if (point != null)
                CharmService.DrawLine(rend, center, point.Value, color);
        }
    }
}
