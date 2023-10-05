//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using System.Drawing;
//using System.Runtime.InteropServices;
//using System.Windows.Forms;
//using Vulkan;
//using SharpDX.Direct3D12;

//namespace SoT_Helper.Services
//{
//    public class OverlayTest
//    {
//        private Margins marg;

//        //this is used to specify the boundaries of the transparent area
//        internal struct Margins
//        {
//            public int Left, Right, Top, Bottom;
//        }

//        //[DllImport("user32.dll", SetLastError = true)]

//        //private static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

//        //[DllImport("user32.dll")]

//        //static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

//        //[DllImport("user32.dll")]

//        //static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

//        //public const int GWL_EXSTYLE = -20;

//        //public const int WS_EX_LAYERED = 0x80000;

//        //public const int WS_EX_TRANSPARENT = 0x20;

//        //public const int LWA_ALPHA = 0x2;

//        //public const int LWA_COLORKEY = 0x1;

//        [DllImport("dwmapi.dll")]
//        static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMargins);

//        private Device device = null;

//        [DllImport("user32.dll")]
//        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

//        [DllImport("user32.dll")]
//        private static extern IntPtr CreateWindowEx(
//            int dwExStyle, string lpClassName, string lpWindowName,
//            int dwStyle, int x, int y, int nWidth, int nHeight,
//            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

//        [DllImport("user32.dll")]
//        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

//        [DllImport("user32.dll")]
//        private static extern int SetLayeredWindowAttributes(IntPtr hWnd, int crKey, byte bAlpha, int dwFlags);

//        [DllImport("user32.dll", SetLastError = true)]
//        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

//        [DllImport("user32.dll")]
//        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

//        //Including WINApi functions. You can find those on PInvoke.net
//        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
//        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);


//        // Making basic RECT struct for GetWindowRect function.
//        public struct RECT
//        {
//            public int left, top, right, bottom;
//        }

//        // Making the graphics object.
//        System.Drawing.Graphics g;

//        [DllImport("user32.dll")]
//        static extern IntPtr GetForegroundWindow();

//        //System.Drawing.Rectangle
//        [DllImport("user32.dll", SetLastError = true)]
//        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

//        private const int GWL_EXSTYLE = -20;
//        private const int WS_EX_LAYERED = 0x80000;
//        private const int WS_EX_TRANSPARENT = 0x20;
//        private const int LWA_COLORKEY = 0x1;
//        private const int LWA_ALPHA = 0x2;

//        private const string WINDOW_TITLE = "Sea of Thieves";
//        private const int OVERLAY_WIDTH = 200;
//        private const int OVERLAY_HEIGHT = 100;
        
//        const int WS_VISIBLE = 0x10000000;
//        const int WS_CHILD = 0x40000000;

//        private IntPtr _externalWindowHandle;
//        private IntPtr _overlayWindowHandle;

//        IntPtr handle = FindWindowByCaption(IntPtr.Zero, WINDOW_TITLE);

//        //public void CreateOverlay()
//        //{
//        //    // Find the handle of the external window
//        //    _externalWindowHandle = FindWindow(null, WINDOW_TITLE);

//        //    if (_externalWindowHandle == IntPtr.Zero)
//        //    {
//        //        throw new Exception("Failed to find external window");
//        //    }

//        //    // Create the overlay window as a child of the external window
//        //    _overlayWindowHandle = CreateWindowEx(
//        //        0, "STATIC", null,
//        //        WS_CHILD | WS_VISIBLE | WS_EX_LAYERED | WS_EX_TRANSPARENT,
//        //        0, 0, OVERLAY_WIDTH, OVERLAY_HEIGHT,
//        //        _externalWindowHandle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

//        //    if (_overlayWindowHandle == IntPtr.Zero)
//        //    {
//        //        throw new Exception("Failed to create overlay window");
//        //    }

//        //    // Set the overlay window styles and properties
//        //    int exStyle = GetWindowLong(_overlayWindowHandle, GWL_EXSTYLE);
//        //    SetWindowLong(_overlayWindowHandle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);
//        //    SetLayeredWindowAttributes(_overlayWindowHandle, 0, 255, LWA_ALPHA);


//        //    //Make the window's border completely transparant
//        //    SetWindowLong(this.Handle, GWL_EXSTYLE,
//        //            (IntPtr)(GetWindowLong(this.Handle, GWL_EXSTYLE) ^ WS_EX_LAYERED ^ WS_EX_TRANSPARENT));

//        //    //Set the Alpha on the Whole Window to 255 (solid)
//        //    SetLayeredWindowAttributes(this.Handle, 0, 255, LWA_ALPHA);

//        //    //Init DirectX
//        //    //This initializes the DirectX device. It needs to be done once.
//        //    //The alpha channel in the backbuffer is critical.
//        //    PresentParameters presentParameters = new PresentParameters();
//        //    presentParameters.Windowed = true;
//        //    presentParameters.SwapEffect = SwapEffect.Discard;
//        //    presentParameters.BackBufferFormat = Format.A8R8G8B8;

//        //    this.device = new Device(0, DeviceType.Hardware, this.Handle,
//        //    CreateFlags.HardwareVertexProcessing, presentParameters);


//        //    Thread dx = new Thread(new ThreadStart(this.dxThread));
//        //    dx.IsBackground = true;
//        //    dx.Start();
//        //}

//        Form overlay;

//        public void Test()
//        {
//            // Find the window handle of the external application
//            //IntPtr externalWindowHandle = FindWindow(null, WINDOW_TITLE);

//            //// Create a transparent form and set its position to be over the external window
//            //Form overlayForm = new Form();
            
//            //overlay = overlayForm;

//            //overlayForm.FormBorderStyle = FormBorderStyle.None;
//            //overlayForm.BackColor = Color.Magenta; // Set a transparent color for testing purposes
//            //SetParent(overlayForm.Handle, externalWindowHandle);
//            //SetWindowLong(overlayForm.Handle, GWL_EXSTYLE, (int)(GetWindowLong(overlayForm.Handle, GWL_EXSTYLE) | WS_EX_LAYERED));
//            //SetLayeredWindowAttributes(overlayForm.Handle, 0, 255, LWA_ALPHA);

//            ///* Making the RECT object that will handle info from WINApi function. */
//            //RECT outrect;
//            ///* Saving data to that object */
//            //GetWindowRect(handle, out outrect);

//            ///* Change the size of the form to the size of the Game window. */
//            //overlayForm.Size = new Size(outrect.right - outrect.left, outrect.bottom - outrect.top);

//            ///* Change the position of the form to the position of the Game window. */
//            //overlayForm.Top = outrect.top;
//            //overlayForm.Left = outrect.left;

//            ///* Change form's style to make it transparent and to remove the border */
//            //overlayForm.FormBorderStyle = FormBorderStyle.None;
//            //overlayForm.BackColor = System.Drawing.Color.Black;

//            ////overlayForm.TransparencyKey = overlayForm.BackColor;

//            ///* Using WINApi function GetWindowLong and SetWindowLong to be able to click throught the form */
//            //int initialStyle = GetWindowLong(overlayForm.Handle, -20);
//            //SetWindowLong(overlayForm.Handle, -20, initialStyle | 0x80000 | 0x20);

//            ///* Make the form on top of other windows */
//            //overlayForm.TopMost = true;

//            //overlayForm.Show();
//        }

//        private void prepaint()
//        {
//            while (true)
//            {
//                overlay.Refresh();
//                /* Refresh rate is 50ms */
//                System.Threading.Thread.Sleep(50);
//            }
//        }

//        private void painttext(System.Drawing.Graphics g)
//        {
//            /* Make a new font object for drawing */
//            Font bigFont = new Font("Arial", 20);
//            /* Make a colored brush for drawing text */
//            Brush mybrush = new SolidBrush(Color.White);
//            /* Draw 'Hello, World' at position 50, 50 of the game window */
//            g.DrawString("Hello, World!", bigFont, mybrush, 50, 50);
//        }
//    }
//}
