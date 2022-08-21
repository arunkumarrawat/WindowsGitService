using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using WindowsGitService.DAL.Interfaces;

namespace WindowsGitService.DAL.FileManagment
{
    public class FileWriter : IChangedFileSaver
    {
        private readonly ILog _log;

        private readonly IFileValidator _fileValidator;

        public FileWriter(ILog log, IFileValidator fileValidator)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _fileValidator = fileValidator ?? throw new ArgumentNullException(nameof(fileValidator));
        }

        /// <summary>
        /// Copies a file to the specified directory
        /// </summary>
        /// <param name="file">Information about the copied file</param>
        /// <param name="targetDirectoryPath">Target folder</param>
        public void SaveFileСhanges(IEnumerable<FileViewInfo> files, string targetDirectoryPath = @"C:\Navicon\Temp")
        {
            if (_fileValidator.IsValidPath(targetDirectoryPath) == false)
            {
                _log.Error($"Path {targetDirectoryPath} incorrect");
                throw new ArgumentException("Directory path is invalid");
            }
            if (files == null)
            {
                _log.Error($"Got null files in CopyFile method");
                return;
            }

            foreach (var file in files)
            {
                CopyFile(file, targetDirectoryPath);
            }

            //_log.Info($"File changes are written to {targetDirectoryPath}");
        }

        /// <summary>
        /// Copies a file to the specified directory
        /// </summary>
        /// <param name="file">Information about the copied file</param>
        /// <param name="targetDirectoryPath">Target folder</param>
        public void CopyFile(FileViewInfo file, string targetDirectoryPath = @"C:\Navicon\Temp")
        {
            if (_fileValidator.IsValidPath(targetDirectoryPath) == false)
            {
                _log.Error($"Path {targetDirectoryPath} incorrect");
                throw new ArgumentException("Directory path is invalid");
            }
            if (file == null)
            {
                _log.Error($"Got null file in CopyFile method");
                return;
            }

            targetDirectoryPath += $@"\{file.GetFormat()}\{file.Name}";

            DirectoryInfo targetDirectoryInfo = new DirectoryInfo(targetDirectoryPath);

            if (targetDirectoryInfo.Exists == false)
            {
                targetDirectoryInfo.Create();
            }

            string targetFilePathInfo = targetDirectoryInfo.FullName + "\\" +
                                        $"{file.Name}-{file.Version}.{file.Format}";

            File.Copy(file.FullPath, targetFilePathInfo, true);

            _log.Info($"File {file.Name}.{file.Format} versions: {file.Version} recorded in {targetDirectoryInfo.FullName}");
        }

        public void SaveLastUpdate(List<FileViewInfo> lastVersion, string path = @"C:\Navicon\LastUpdated.txt")
        {
            using (StreamWriter myStream = new StreamWriter(path))
            {

                string s = JsonConvert.SerializeObject(lastVersion, Formatting.Indented);

                myStream.WriteLine(s);
            }
        }
    }
}
