using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace WindowsGitService.DAL
{
    public class FileChangesFacade : IFileChangesFacade
    {
        private readonly IFileAccessor _fileAccessor;

        private readonly IChangedFileSaver _changedFileSaver;

        private readonly IFileViewConverter _fileModelBuilder;

        private readonly IFileChangesTracker _fileChangesTracker;

        private readonly ILog _log;

        public FileChangesFacade(IFileAccessor      fileAccessor, IChangedFileSaver   changedFileSaver,
                                 IFileViewConverter modelBuilder, IFileChangesTracker fileChangesTracker,
                                 ILog log)
        {
            if (log == null)
            {
                throw new ArgumentException(nameof(log));
            }

            _log = log;

            if (fileAccessor == null)
            {
                _log.Error($"FileChangesDirector get null {nameof(fileAccessor)} constructor param");
                throw new ArgumentException(nameof(fileAccessor));
            }
            if (changedFileSaver == null)
            {
                _log.Error($"FileChangesDirector get null {nameof(changedFileSaver)} constructor param");
                throw new ArgumentException(nameof(changedFileSaver));
            }
            if (modelBuilder == null)
            {
                _log.Error($"FileChangesDirector get null {nameof(modelBuilder)} constructor param");
                throw new ArgumentException(nameof(modelBuilder));
            }
            if (fileChangesTracker == null)
            {
                _log.Error($"FileChangesDirector get null {nameof(fileChangesTracker)} constructor param");
                throw new ArgumentException(nameof(fileChangesTracker));
            }

            _fileAccessor = fileAccessor;
            _changedFileSaver = changedFileSaver;
            _fileModelBuilder = modelBuilder;
            _fileChangesTracker = fileChangesTracker;
        }

        /// <summary>
        /// Populates lastVersion with a list of initial data
        /// </summary>
        public void InitializePreviouslyCopiedFiles()
        {
            IEnumerable<FileViewInfo> deserializedProduct = _fileAccessor.GetLastUpdate();
         
            if (deserializedProduct != null)
            {
                _fileChangesTracker.lastVersion = (List<FileViewInfo>)deserializedProduct;
                _log.Info($"The director initialized the initial list from {_fileChangesTracker.lastVersion.Count} previously copied files");
            }
            else
            {
                _log.Warn("Previously scanned files are missing");
            }
        }

        /// <summary>
        /// Stores lastVersion as a list of initial data
        /// </summary>
        public void SaveCurrentFileVersionState()
        {
            const string path = @"C:\Navicon\LastUpdated.txt";

            _changedFileSaver.SaveLastUpdate(_fileChangesTracker.lastVersion, path);

            _log.Warn($"Saved file data to {path}");
        }

        /// <summary>
        /// Comparing actual files with data saved in lastVersion
        /// and saves the modified files in the directory
        /// compares files from all folders in config
        /// </summary>
        public void MakeFilesCompare()
        {
            _log.Warn($"Director starts comparing all folders available in App.config");

            IReadOnlyCollection<FileInfo> actualFiles = (IReadOnlyCollection<FileInfo>)_fileAccessor.GetFiles();

            _log.Info($"The director received a list of {actualFiles.Count} actual files");

            IEnumerable<FileViewInfo> actualViewFiles;

            actualViewFiles = _fileModelBuilder.ConvertToFileInfo(actualFiles);

            // _log.Info($"Director got file conversion to FileViewInfo");

            List<FileViewInfo> changedFiles = (List<FileViewInfo>)_fileChangesTracker.GetChangedFiles(_fileChangesTracker.lastVersion, actualViewFiles);

            _log.Info($"The director received a list of {changedFiles.Count} changed files");

            // updating the "lastversion" list of actual files
            changedFiles = _fileChangesTracker.UpdateLastVersion(changedFiles);

            // saving changed files
            _changedFileSaver.SaveFileСhanges(changedFiles);

            _log.Warn($"The director completed the operation to save the changes");
        }

        /// <summary>
        /// Comparing actual files with data saved in lastVersion
        /// and saves the modified files in the directory
        /// by timer
        /// </summary>
        /// <param name="folders">IEnumerable<FileInfo> список сканируемых папок</param>
        public void MakeFilesCompare(object folders)
        {
            IEnumerable<string> scanningFolders = folders as IEnumerable<string>;

            if(scanningFolders == null)
            {
                _log.Error("MakeFilesCompare(object folders) null argument param");
                return;
            }

            MakeFilesCompare(scanningFolders);
        }

        /// <summary>
        /// Comparing actual files with data saved in lastVersion
        /// and saves the modified files in the directory
        /// </summary>
        /// <param name="folders">Список сканируемых папок</param>
        public void MakeFilesCompare(IEnumerable<string> folders)
        {
            _log.Warn($"The director starts comparing files from ${string.Join(" ", folders)}");

            var actualFiles = (IReadOnlyCollection<FileInfo>)_fileAccessor.GetFiles(folders);

            _log.Info($"The director received a list of {actualFiles.Count} actual files");

            IEnumerable<FileViewInfo> actualViewFiles = _fileModelBuilder.ConvertToFileInfo(actualFiles);

            // _log.Info($"Director got file conversion to FileViewInfo");

            var changedFiles = (List<FileViewInfo>)_fileChangesTracker.GetChangedFiles(_fileChangesTracker.lastVersion, actualViewFiles);

            _log.Info($"The director received a list of {changedFiles.Count} changed files");

            // updating the "lastversion" list of actual files
            changedFiles = _fileChangesTracker.UpdateLastVersion(changedFiles);

            // saving changed files
            _changedFileSaver.SaveFileСhanges(changedFiles);

            _log.Warn($"The director completed the operation to save the changes");
        }
    }
}