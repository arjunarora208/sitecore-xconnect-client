using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Sitecore.Practise.App.Services
{
    public static class LogHelper
    {
        public static void Error(string message, Exception serviceException, string objGuid)
        {
            FileStream fileStream = null;
            StreamWriter streamWriter = null;
            try
            {
                StringBuilder messageBuilder = new StringBuilder();
                messageBuilder.Append("The Exception is:-");

                messageBuilder.Append("Exception :: " + serviceException.ToString());
                if (serviceException.InnerException != null)
                {
                    messageBuilder.Append("InnerException :: " + serviceException.InnerException.ToString());
                }

                string logFilePath = ConfigurationManager.AppSettings["LogPath"];

                if (string.IsNullOrWhiteSpace(logFilePath))
                    logFilePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "\\Logs\\";


                logFilePath = logFilePath + "ProgramLog" + "-" + DateTime.Now.ToString("yyyyMMddhh") + "-" + objGuid + "-Error." + "txt";

                #region Create the Log file directory if it does not exists 
                DirectoryInfo logDirInfo = null;
                FileInfo logFileInfo = new FileInfo(logFilePath);
                logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
                if (!logDirInfo.Exists) logDirInfo.Create();
                #endregion Create the Log file directory if it does not exists

                if (!logFileInfo.Exists)
                {
                    fileStream = logFileInfo.Create();
                }
                else
                {
                    fileStream = new FileStream(logFilePath, FileMode.Append);
                }
                streamWriter = new StreamWriter(fileStream);
                streamWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " Error: " + message + " Detail Error: " + messageBuilder.ToString());
            }
            finally
            {
                if (streamWriter != null) streamWriter.Close();
                if (fileStream != null) fileStream.Close();
            }

        }

        public static void Info(string message, string InfoType, string objGuid)
        {
            FileStream fileStream = null;
            StreamWriter streamWriter = null;
            try
            {

                string logFilePath = ConfigurationManager.AppSettings["LogPath"];

                if (string.IsNullOrWhiteSpace(logFilePath))
                    logFilePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "\\Logs\\";

                logFilePath = logFilePath + "ProgramLog" + "-" + DateTime.Now.ToString("yyyyMMddhh") + "-" + objGuid + "-Info." + "txt";

                #region Create the Log file directory if it does not exists 
                DirectoryInfo logDirInfo = null;
                FileInfo logFileInfo = new FileInfo(logFilePath);
                logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
                if (!logDirInfo.Exists) logDirInfo.Create();
                #endregion Create the Log file directory if it does not exists

                if (!logFileInfo.Exists)
                {
                    fileStream = logFileInfo.Create();
                }
                else
                {
                    fileStream = new FileStream(logFilePath, FileMode.Append);
                }
                streamWriter = new StreamWriter(fileStream);
                streamWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " Info: " + InfoType + " " + message);
            }
            finally
            {
                if (streamWriter != null) streamWriter.Close();
                if (fileStream != null) fileStream.Close();
            }

        }

        public static void SaveBookmark(byte[] bookmark)
        {
            if (bookmark == null || bookmark.Length == 0) return;

            FileStream fileStream = null;
            StreamWriter streamWriter = null;
            try
            {
                string logFilePath = ConfigurationManager.AppSettings["LogPath"];

                if (string.IsNullOrWhiteSpace(logFilePath))
                    logFilePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "\\Logs\\";

                logFilePath = logFilePath + "bookmark.dat";

                using (var fs = new FileStream(logFilePath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(bookmark, 0, bookmark.Length);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (streamWriter != null) streamWriter.Close();
                if (fileStream != null) fileStream.Close();
            }

        }

        public static byte[] GetBookmark()
        {
            try
            {
                string logFilePath = ConfigurationManager.AppSettings["LogPath"];

                if (string.IsNullOrWhiteSpace(logFilePath))
                    logFilePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "\\Logs\\";

                logFilePath = logFilePath + "bookmark.dat";
                return File.ReadAllBytes(logFilePath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}