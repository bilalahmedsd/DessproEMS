using System.IO;
using System;
using System.Security.Principal;

namespace EMS.Core.Helpers
{
    public static class Constants
    {
        static Constants()
        {
            if (!Directory.Exists(TempAssetsDirectory))
            {
                Directory.CreateDirectory(TempAssetsDirectory);
            }
        }

        private static System.Reflection.Assembly AppAssembly => System.Reflection.Assembly.GetEntryAssembly() ?? System.Reflection.Assembly.GetExecutingAssembly();
        private static string AppPath => AppAssembly.Location;

        private static string ProgramDataFolderPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            AppName
        );

        public static string AppName => AppAssembly?.GetName().Name;

        public static bool IsAdministrator = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public static string AssetsDirectory => ProgramDataFolderPath;

        public static string LogFilePath => Path.Combine(AssetsDirectory, "AppLog.log");

        public static string TempAssetsDirectory => Path.Combine(AssetsDirectory, "tmp");
    }
}