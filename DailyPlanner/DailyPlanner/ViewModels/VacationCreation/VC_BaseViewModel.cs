using DailyPlanner.Repository;
using DailyPlanner.Utility;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace DailyPlanner.ViewModels.VacationCreation
{
    public class VC_BaseViewModel : NavigableViewModel, INavigationAware
    {
        INavigationService navigationService;
        public VC_BaseViewModel(INavigationService _navigationService, IRepository _repository)
            :base(_repository)
        {
            navigationService = _navigationService;
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            if(parameters.ContainsKey(Constants.EXIT_KEY))
            {
                await navigationService.GoBackAsync(parameters: parameters);
                return;
            }
        }
    }
}
