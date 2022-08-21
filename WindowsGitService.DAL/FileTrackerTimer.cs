using log4net;
using System;
using System.Threading;
using System.Collections.Generic;
using WindowsGitService.CustomConfig;
using System.Linq;
using System.Configuration;

namespace WindowsGitService.DAL
{
    public class FileTrackerTimer
    {
        private ILog _log;

        private IMonitoringFolders _monitoringFolders;

        private IFileChangesFacade _fileChangesFacade;

        private List<Timer> _timers;

        public FileTrackerTimer(ILog log, IMonitoringFolders monitoringFolders,
                                IFileChangesFacade fileChangesFacade)
        {
            if (log == null)
            {
                throw new ArgumentException(log.ToString());
            }

            _log = log;

            if (monitoringFolders == null)
            {
                throw new ArgumentException(monitoringFolders.ToString());
            }
            if (fileChangesFacade == null)
            {
                throw new ArgumentException(fileChangesFacade.ToString());
            }

            _monitoringFolders = monitoringFolders;
            _fileChangesFacade = fileChangesFacade;
            _timers = new List<Timer>();
        }

        public void InitilizeTimer()
        {
            // initialization of information about previously copied files
            _fileChangesFacade.InitializePreviouslyCopiedFiles();

            // reading information about scanned folders from App.config
            List<FolderElement> folders = _monitoringFolders.GetFolderElements();

            // allocation of groups according to the monitoring interval
            var timePeriods = folders.GroupBy(m => m.МonitoringPeriod);

            // creating a callback pl when the timer expires
            TimerCallback timerCallback = new TimerCallback(_fileChangesFacade.MakeFilesCompare);

            foreach (var group in timePeriods)
            {
                List<string> trackingFolders = new List<string>();

                // collect all watched folders in a temporary group
                trackingFolders.AddRange(group.Select(g => g.Path).ToList());

                // creating a timer for each time group
                Timer timer = new Timer(timerCallback, trackingFolders, 0, Int32.Parse(group.Key));

                _timers.Add(timer);
            }

            // TimerCallback tC = new TimerCallback(ClearTimers);
            // _timers.Add(new Timer(tC, 1, 10000, 50000));

            //  Backup timer
            TimerCallback backupCallback = new TimerCallback(SaveFileState);
            _timers.Add(new Timer(backupCallback, 1, 5000, 100000));
        }

        public void ClearTimers(object a)
        {
            ClearTimers();
        }

        public void ClearTimers()
        {
            _timers = null;

            _fileChangesFacade.SaveCurrentFileVersionState();

            _log.Error("Active timers destroyed");
        }

        public void SaveFileState(object a = null)
        {
            _fileChangesFacade.SaveCurrentFileVersionState();
        }
    }
}
