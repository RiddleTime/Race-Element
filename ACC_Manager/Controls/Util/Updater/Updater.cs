using RaceElement.Util;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Controls.Util.Updater
{
    internal class AppUpdater
    {
        internal void Update(ReleaseAsset asset)
        {
            if (!Verify(asset))
            {
                LogWriter.WriteToLog("AutoUpdater: Unable to verify release asset.");
                return;
            }

            string assemblyStart = System.Reflection.Assembly.GetExecutingAssembly().Location;

            FileInfo currentAssemblyFile = new FileInfo(assemblyStart);

            MoveCurrentExecutableToDocumentsPath(currentAssemblyFile.FullName);

            if (!DownloadNewVersion(asset.BrowserDownloadUrl, currentAssemblyFile.FullName))
            {
                RevertVersion(assemblyStart);
                return;
            }

            if (!RunNewVersion(currentAssemblyFile.FullName))
            {
                LogWriter.WriteToLog("AutoUpdater: Something went wrong trying to run the newest version.");
                RevertVersion(assemblyStart);
                return;
            }
        }

        private bool MoveCurrentExecutableToDocumentsPath(string currentExecutableFullName)
        {
            Debug.WriteLine($"AutoUpdater: Moving current Executable to {FileUtil.AccManagerAppDataPath}");

            FileInfo toBeMoved = new FileInfo(currentExecutableFullName);

            string tempTargetFile = $"{FileUtil.AccManagerAppDataPath}AccManager.exe";

            FileInfo targetFile = new FileInfo(tempTargetFile);
            if (targetFile.Exists)
                targetFile.Delete();

            toBeMoved.MoveTo(tempTargetFile);

            Debug.WriteLine($"Current location of currentExecutable {toBeMoved.FullName} ");
            return true;
        }

        private bool RevertVersion(string currentExecutableFullName)
        {
            try
            {
                Debug.WriteLine($"AutoUpdater: Reverting Executable from {FileUtil.AccManagerAppDataPath} to {currentExecutableFullName}");

                FileInfo toBeMoved = new FileInfo($"{FileUtil.AccManagerAppDataPath}AccManager.exe");

                FileInfo targetFile = new FileInfo(currentExecutableFullName);
                if (targetFile.Exists)
                    targetFile.Delete();

                toBeMoved.MoveTo(currentExecutableFullName);

                Debug.WriteLine($"Current location of currentExecutable {toBeMoved.FullName} ");
            }
            catch (Exception e)
            {
                LogWriter.WriteToLog("AutoUpdater: Something went wrong trying to revert the previous version.");
                LogWriter.WriteToLog(e);
            }
            return true;
        }

        private bool DownloadNewVersion(string downloadUrl, string targetFile)
        {
            try
            {
                var client = new WebClient();
                client.DownloadFile(downloadUrl, targetFile);

                LogWriter.WriteToLog($"AutoUpdater: Downloaded latest version from {downloadUrl} to file:\n{targetFile}");

                FileInfo newVersion = new FileInfo(targetFile);
                return newVersion.Exists;
            }
            catch (Exception e)
            {
                LogWriter.WriteToLog("AutoUpdater: Something went wrong trying to download the newest version.");
                LogWriter.WriteToLog(e);
                return false;
            }
        }

        private bool RunNewVersion(string targetFile)
        {
            LogWriter.WriteToLog("Shutting down, starting newest version");

            FileInfo newVersion = new FileInfo(targetFile);

            if (!newVersion.Exists)
                return false;

            string fullName = newVersion.FullName.Replace('\\', '/');
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "cmd",
                Arguments = $"/c start \"AccManager.exe\" \"{fullName}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            LogWriter.WriteToLog(startInfo.Arguments);


            Process.Start(startInfo);

            Environment.Exit(0);

            return false;
        }

        private bool Verify(ReleaseAsset asset)
        {
            if (asset.ContentType != "application/x-msdownload")
                return false;
            if (asset.State != "uploaded")
                return false;
            if (asset.Uploader.Login != "RiddleTime")
                return false;

            return true;
        }
    }
}
