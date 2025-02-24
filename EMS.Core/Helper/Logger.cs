using System;
using System.IO;

namespace EMS.Core.Helpers
{
    public static class Logger
    {
        private static readonly string LogFilePath = $"{Constants.LogFilePath}_{DateTime.Today:dd-MM-yy}";

        private static void CreateFolderIfNotExists()
        {
            string folderPath = Constants.AssetsDirectory;

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        public static void Log(string message)
        {
            try
            {
                CreateFolderIfNotExists();
                File.AppendAllText(LogFilePath, $"{DateTime.Now}\nLog: {message}\n\n");
            }
            catch { /**/ }
        }

        public static void Log(Exception exception)
        {
            try
            {
                CreateFolderIfNotExists();
                File.AppendAllText(LogFilePath, $"{DateTime.Now}\nException: {exception?.ToJson()}\n\n");
            }
            catch { /**/ }
        }
    }
}