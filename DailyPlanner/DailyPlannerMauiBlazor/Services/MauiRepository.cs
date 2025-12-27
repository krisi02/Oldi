using System;
using System.Globalization;
using System.Threading.Tasks;
using DailyPlanner.Models;
using DailyPlanner.Models.Enum;
using DailyPlanner.Models.NetworkModels;
using DailyPlanner.Remote;
using DailyPlannerMauiBlazor.Services;

namespace DailyPlannerMauiBlazor.Services
{
    public class MauiRepository : DailyPlanner.Repository.IRepository
    {
        private const string LoggedUserKey = "LOGGED_USER";
        
        private readonly IStorageService _storageService;
        private readonly IRemoteAPI _remoteAPI;
        private bool? _isLogged = null;

        public MauiRepository(IStorageService storageService, IRemoteAPI remoteAPI)
        {
            _storageService = storageService;
            _remoteAPI = remoteAPI;
        }

        public async Task<bool> IsUserLoggedAsync()
        {
            if (_isLogged == null)
            {
                var loggedUser = await _storageService.GetAsync<UserModel>(LoggedUserKey);
                _isLogged = loggedUser != null;
                if (_isLogged.Value && loggedUser != null)
                {
                    DailyPlanner.Settings.Settings.Default.LoggedUserId = loggedUser.UserId;
                    DailyPlanner.Settings.Settings.Default.LoggedUserFullName = loggedUser.Username;
                    DailyPlanner.Settings.Settings.Default.LoggedUserRole = loggedUser.Role;
                    DailyPlanner.Settings.Settings.Default.PlantCode = loggedUser.PlantCode;
                    DailyPlanner.Settings.Settings.Default.PlantName = loggedUser.PlantDescription;
                    DailyPlanner.Settings.Settings.Default.AuthorizationToken = loggedUser.Token;
                }
            }
            return _isLogged.Value;
        }

        public async Task<UserModel> GetUser()
        {
            var loggedUser = await _storageService.GetAsync<UserModel>(LoggedUserKey);
            return loggedUser ?? new UserModel();
        }

        public async Task<Response<UserModel>> LoginAsync(string username, string password)
        {
            var response = await _remoteAPI.LoginAsync(username, password, "");
            if (response.Success)
            {
                await _storageService.SetAsync(LoggedUserKey, response.Item);
                DailyPlanner.Settings.Settings.Default.LoggedUserId = response.Item.UserId;
                DailyPlanner.Settings.Settings.Default.LoggedUserFullName = response.Item.Username;
                DailyPlanner.Settings.Settings.Default.LoggedUserRole = response.Item.Role;
                DailyPlanner.Settings.Settings.Default.PlantCode = response.Item.PlantCode;
                DailyPlanner.Settings.Settings.Default.PlantName = response.Item.PlantDescription;
                DailyPlanner.Settings.Settings.Default.AuthorizationToken = response.Item.Token;
                _isLogged = true;
            }
            return response;
        }

        public async Task<Response<bool>> LogoutAsync(string userId)
        {
            var response = await _remoteAPI.LogoutAsync(userId);
            if (response.Success)
            {
                await _storageService.ClearAllAsync();
                DailyPlanner.Settings.Settings.Default.LoggedUserId = "";
                DailyPlanner.Settings.Settings.Default.LoggedUserFullName = "";
                DailyPlanner.Settings.Settings.Default.LoggedUserRole = RolesEnum.AUTISTA;
                DailyPlanner.Settings.Settings.Default.PlantCode = "";
                DailyPlanner.Settings.Settings.Default.PlantName = "";
                DailyPlanner.Settings.Settings.Default.AuthorizationToken = null;
                _isLogged = false;
            }
            return response;
        }

        public async Task ForceUserLogout()
        {
            await _storageService.ClearAllAsync();
            DailyPlanner.Settings.Settings.Default.LoggedUserId = "";
            DailyPlanner.Settings.Settings.Default.LoggedUserFullName = "";
            DailyPlanner.Settings.Settings.Default.LoggedUserRole = RolesEnum.AUTISTA;
            DailyPlanner.Settings.Settings.Default.PlantCode = "";
            DailyPlanner.Settings.Settings.Default.PlantName = "";
            DailyPlanner.Settings.Settings.Default.AuthorizationToken = null;
            _isLogged = false;
        }

        // Placeholder implementations for other required methods
        public Task<Response<System.Collections.Generic.List<DriverModel>>> GetAvailibleDriversForDay(DateTime day)
        {
            return _remoteAPI.GetAvailibleDriversForDay(day);
        }

        public Task<Response<CausalsList>> GetCausals(CausalTypeEnum filter)
        {
            return filter == CausalTypeEnum.REVOCATION 
                ? _remoteAPI.GetRevocationCausals() 
                : _remoteAPI.GetVacationCausals();
        }

        public Task<Response<TruckDriverMatrix>> GetDefaultTruckDriverMatrix()
        {
            return _remoteAPI.GetTruckDriverDefaultMatrix();
        }

        public Task<Response<PlanningsList>> GetPlanningsListForDay(DateTime day)
        {
            return _remoteAPI.GetPlanningsForDay(day);
        }

        public Task<Response<VacationsList>> GetVacationsList(DateTime startingDate, DateTime endingDate)
        {
            return _remoteAPI.GetVacationsList(startingDate, endingDate);
        }

        public Task<Response<bool>> CreateVacation(VacationModel model)
        {
            return _remoteAPI.CreateVacation(model);
        }

        public Task<Response<bool>> UpdateVacation(VacationModel model)
        {
            return _remoteAPI.UpdateVacation(model);
        }

        public Task<Response<bool>> RevokeVacation(VacationModel model)
        {
            return _remoteAPI.RevokeVacation(model);
        }

        public Task<Response<PlanningsList>> GetDriverNotifications()
        {
            return _remoteAPI.GetDriverNotifications();
        }

        public Task<Response<WorkingHoursList>> GetWorkingHoursList(DateTime day)
        {
            return _remoteAPI.GetWorkingHoursForDay(day);
        }

        public Task<Response<TruckDriverMatrix>> GetTruckDriverMatrixForPlanning(DateTime day)
        {
            return _remoteAPI.GetTruckDriverMatrixForPlanning(day);
        }

        public Task<Response<PlantsList>> GetPlantList()
        {
            return _remoteAPI.GetPlantList();
        }

        public Task<Response<AnomalyList>> GetAnomalies()
        {
            return _remoteAPI.GetAnomalies();
        }

        public Task<Response<bool>> CreatePlanning(Planning model)
        {
            return _remoteAPI.CreatePlanning(model);
        }

        public Task<Response<bool>> UpdatePlanning(Planning model)
        {
            return _remoteAPI.UpdatePlanning(model);
        }

        public Task<Response<bool>> RevokePlanning(PlanningRevoke model)
        {
            return _remoteAPI.RevokePlanning(model);
        }

        public Task<Response<bool>> CompleteAnomaly(AnomalyConfirmation model)
        {
            return _remoteAPI.CompleteAnomaly(model);
        }

        public Task<Response<bool>> TransferAnomaly(AnomalyTransfer model)
        {
            return _remoteAPI.TransferAnomaly(model);
        }

        public Task<Response<bool>> UnlinkTruckDriver(TruckDriverUnlinkModel model)
        {
            return _remoteAPI.UnlinkTruckDriver(model);
        }

        public Task<Response<bool>> DriverPlanningConfirmation(DriverPlanningConfirmation model)
        {
            return _remoteAPI.DriverPlanningConfirmation(model);
        }
    }
}
