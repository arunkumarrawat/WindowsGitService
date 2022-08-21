using System;

namespace WindowsGitService.DAL
{
    public sealed class FileViewInfo
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string Format { get; set; }
        public string Path { get; set; }
        public string FullPath { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastChange { get; set; }
        public int Version { get; set; }
        public int Hash { get; set; }

        public string GetFormat()
        {
            return Format;
        }

        public override string ToString()
        {
            return $"Name: {Name}, Format: {Format}, Version: {Version}" +
                   $"Created: {Created}, Last Change: {LastChange}, " +
                   $"Path: {Path}, Hash: {string.Join("", Hash)}";
        }

        public bool Equals(FileViewInfo fileView)
        {
            if (fileView == null)
            {
                return false;
            }

            return fileView.FullPath == this.FullPath;
        }

        public override bool Equals(object obj)
        {
            var fileView = obj as FileViewInfo;

            if (fileView == null)
            {
                return false;
            }

            return fileView.FullPath == this.FullPath;
        }

        public override int GetHashCode()
        {
            return Hash + FullPath.GetHashCode();
        }

        #region interfaces
        public FileViewInfo UpdateTo(FileViewInfo fileView)
        {
            if (fileView == null)
            {
                throw new ArgumentException();
            }

            fileView.Name = this.Name;
            fileView.FileName = this.FileName;
            fileView.Format = this.Format;
            fileView.Path = this.Path;
            fileView.FullPath = this.FullPath;
            fileView.Created = this.Created;
            fileView.LastChange = this.LastChange;
            fileView.Hash = this.Hash;

            return fileView;
        }
        #endregion
    }
}
