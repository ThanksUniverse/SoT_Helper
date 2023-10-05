//using System.Diagnostics;
//using System.Runtime.InteropServices;
//using DXNET;
//using DXNET.Direct2D;
//using DXNET.Mathematics.Interop;
//using DXNET.Windows;

//namespace SoT_Helper.Services
//{
//    public class SharpDXHelper
//    {
//        [DllImport("user32.dll", SetLastError = true)]
//        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

//        [DllImport("user32.dll", SetLastError = true)]
//        [return: MarshalAs(UnmanagedType.Bool)]
//        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

//        [StructLayout(LayoutKind.Sequential)]
//        private struct RECT
//        {
//            public int Left;
//            public int Top;
//            public int Right;
//            public int Bottom;
//        }

//        public void DrawRectangleInWindow(Process process)
//        {
//            // Get the window handle and current size
//            IntPtr handle = process.MainWindowHandle;
//            Rect rect;
//            GetWindowRect(handle, out rect);
//            int width = rect.Right - rect.Left;
//            int height = rect.Bottom - rect.Top;

//            // Create a Direct2D render target for the window
//            var factory = new SharpDX.Direct2D1.Factory();
//            var renderProperties = new HwndRenderTargetProperties()
//            {
//                Hwnd = handle,
//                PixelSize = new SharpDX.Size2(width, height),
//                PresentOptions = PresentOptions.None
//            };
//            var renderTarget = new WindowRenderTarget(factory, new RenderTargetProperties(), renderProperties);

//            // Create a red brush for drawing
//            var brush = new SolidColorBrush(renderTarget, Color.Red);

//            // Draw a red rectangle in the center of the window
//            renderTarget.BeginDraw();
//            renderTarget.Clear(Color.Black);
//            renderTarget.FillRectangle(new RectangleF(width / 4, height / 4, width / 2, height / 2), brush);
//            renderTarget.EndDraw();

//            // Dispose of resources
//            brush.Dispose();
//            renderTarget.Dispose();
//            factory.Dispose();
//        }
//    }
//}
