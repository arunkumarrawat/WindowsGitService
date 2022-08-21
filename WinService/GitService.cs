using log4net;
using Ninject;
using System.ServiceProcess;
using WindowsGitService.DAL;
using WinService.DI;

namespace WinService
{
    public partial class GitService : ServiceBase
    {
        private IKernel _kernel { get; set; }
        private FileTrackerTimer _timeManager { get; set; }

        public GitService()
        {
            InitializeComponent();

            _kernel = new DependensyInjection().GetKernel();
            _timeManager = _kernel.Get<FileTrackerTimer>();
        }

        protected override void OnStart(string[] args)
        {
            _kernel.Get<ILog>().Warn("Service Start");

            _timeManager.InitilizeTimer();

        }

        protected override void OnStop()
        {
            _timeManager.ClearTimers();
        }
    }
}
