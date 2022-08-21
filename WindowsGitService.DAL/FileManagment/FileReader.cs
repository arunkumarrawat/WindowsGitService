using System;
using System.Collections.Generic;
using System.IO;
using WindowsGitService.Logging;
using System.Linq;
using log4net;
using WindowsGitService.CustomConfig;
using WindowsGitService.DAL.Interfaces;
using Newtonsoft.Json;

namespace WindowsGitService.DAL
{
    public class FileReader : IFileAccessor
    {
        private readonly ILog _log;

        private readonly IMonitoringFolders _monitoringFolders;

        private readonly IFileValidator _fileValidator;

        public FileReader(ILog log, IFileValidator fileValidator, IMonitoringFolders monitoringFolders)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _fileValidator = fileValidator ?? throw new ArgumentNullException(nameof(fileValidator));
            _monitoringFolders = monitoringFolders ?? throw new ArgumentNullException(nameof(monitoringFolders));
        }

        /// <summary>
        /// Get a list of files from a list of directories IMonitoringFolders
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FileInfo> GetFiles()
        {
            List<FileInfo> fileInfos = new List<FileInfo>();

            List<string> paths = _monitoringFolders.GetFoldersPath();

            foreach (var path in paths)
            {
                fileInfos.AddRange(GetFiles(path));
            }

            return fileInfos;
        }

        /// <summary>
        /// Get a list of files from a directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IEnumerable<FileInfo> GetFiles(string path = @"C:\Navicon\JsonSerializer")
        {
            if (_fileValidator.IsValidPath(path) == false)
            {
                _log.Error($"Invalid directory path {path}");
                throw new ArgumentException(path);
            }

            if (Directory.Exists(path) == false)
            {
                _log.Error($"Directory does not exist {path}");
                throw new ArgumentException(path);
            }

            return new DirectoryInfo(path).GetFiles("*.*").ToList();
        }

        /// <summary>
        /// Get a list of files from a list of directories
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public IEnumerable<FileInfo> GetFiles(IEnumerable<string> paths)
        {
            if (paths == null)
            {
                _log.Error($"{nameof(GetFiles)} Passed null value { paths}");
                throw new ArgumentException(nameof(paths));
            }

            List<FileInfo> fileInfos = new List<FileInfo>();

            foreach (var path in paths)
            {
                fileInfos.AddRange(GetFiles(path));
            }

            return fileInfos;
        }

        public IEnumerable<FileViewInfo> GetLastUpdate(string path = @"C:\Navicon\LastUpdated.txt")
        {
            List<FileViewInfo> deserializedProduct;

            if (File.Exists(path) == false)
            {
                return null;
            }

            using (StreamReader sr = new StreamReader(path))
            {
                deserializedProduct = JsonConvert.DeserializeObject<List<FileViewInfo>>(sr.ReadToEnd());
            }

            return deserializedProduct;
        }
    }
}