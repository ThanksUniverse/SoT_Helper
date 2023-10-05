using SoT_Helper.Services;

namespace SoT_Helper
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            //OverlayHelper.Run();
            ApplicationConfiguration.Initialize();
            Application.Run(new SoTHelper());
            //Application.Run(new MapForm());
            //Application.Run(new TestMap());
        }
    }
}