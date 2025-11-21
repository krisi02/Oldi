using DailyPlanner.Repository;
using Prism.Mvvm;
using Shiny.Push;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DailyPlanner.ViewModels
{
    public abstract class NavigableViewModel : BindableBase
    {
        IRepository repository;
        public NavigableViewModel(IRepository _repository)
        {
            repository = _repository;
        }
        public async Task ForceLogout()
        {
            await repository.ForceUserLogout();
            var pushManager = Shiny.ShinyHost.Resolve<IPushManager>();
            await pushManager.TryClearTags();
        }
        public bool IsAuthorized(string responseMessage)
        {
            if(responseMessage == null) { return true; }
            if(responseMessage.ToUpper() == "UNAUTHORIZED") { return false; }
            else { return true; }
        }
        private bool _canNavigate = true;
        public bool CanNavigate
        {
            get { return _canNavigate; }
            set { SetProperty(ref _canNavigate, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }
    }
}
