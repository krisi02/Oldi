using DailyPlanner.Models;
using DailyPlanner.Repository;
using DailyPlanner.Utility;
using DailyPlanner.ViewModels.ListViewItems;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Text;

namespace DailyPlanner.ViewModels.PlanningCreation
{
    public class PN_BaseViewModel : NavigableViewModel, INavigationAware
    {
        INavigationService navigationService;
        public PN_BaseViewModel(INavigationService _navigationService, IRepository _repository)
            :base(_repository)
        {
            navigationService = _navigationService;
        }


        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(Constants.EXIT_KEY))
            {
                await navigationService.GoBackAsync(parameters: parameters);
                return;
            }
        }
        
        public Planning selectedPlanning { get; set; }
        public DateTime selectedPlanningDate { get; set; }
    }
}
