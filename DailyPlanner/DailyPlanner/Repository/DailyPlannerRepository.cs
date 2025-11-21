using Akavache;
using DailyPlanner.Models;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DailyPlanner.Settings;
using DailyPlanner.Models.NetworkModels;
using DailyPlanner.Remote;
using System.Linq;
using DailyPlanner.Models.Enum;

namespace DailyPlanner.Repository
{
    public class DailyPlannerRepository : IRepository
    {
        private const string LoggedUserKey = "LOGGED_USER";
        private const string DefaultTruckDriverMatrixKey = "TRUCK_DRIVER_MATRIX";
        private const string CausalsKey = "CAUSALS";
        private const string VacationListKey = "VACATIONS_LIST";
        private const string PlanningListKey = "PLANNINGS_LIST";
        private const string PlantListKey = "PLANT_LIST";

        private IBlobCache blobCache;
        private ISecureBlobCache secureBlobCache;
        private IRemoteAPI remoteAPI;
        private bool? IsLogged = null;

        public DailyPlannerRepository(ISecureBlobCache _secureBlobCache, IRemoteAPI _remoteAPI, IBlobCache _blobCache)
        {
            secureBlobCache = _secureBlobCache;
            remoteAPI = _remoteAPI;
            blobCache = _blobCache;
        }
        
        public async Task<bool> IsUserLoggedAsync()
        {
            if(IsLogged == null)
            {
                var loggedUser = await secureBlobCache.GetObject<UserModel>(LoggedUserKey)
                        .Catch((Exception ex) => Observable.Return<UserModel>(null));
                IsLogged = loggedUser != null;
                if (IsLogged.Value)
                {
                    Settings.Settings.Default.LoggedUserId = loggedUser.UserId;
                    Settings.Settings.Default.LoggedUserFullName = loggedUser.Username;
                    Settings.Settings.Default.LoggedUserRole = loggedUser.Role;
                    Settings.Settings.Default.PlantCode = loggedUser.PlantCode;
                    Settings.Settings.Default.PlantName =loggedUser.PlantDescription;
                    Settings.Settings.Default.AuthorizationToken = loggedUser.Token;
                }
            }
            return IsLogged.Value;
        }
        public async Task<UserModel> GetUser()
        {
            var loggedUser = await secureBlobCache.GetObject<UserModel>(LoggedUserKey)
                        .Catch((Exception ex) => Observable.Return<UserModel>(null));
            return loggedUser;
        }


        public async Task<Response<UserModel>> LoginAsync(string username, string password)
        {
            var response = await remoteAPI.LoginAsync(username, password, "");
            if (response.Success)
            {
                await secureBlobCache.InsertObject(LoggedUserKey, response.Item);
                Settings.Settings.Default.LoggedUserId = response.Item.UserId;
                Settings.Settings.Default.LoggedUserFullName = response.Item.Username;
                Settings.Settings.Default.LoggedUserRole = response.Item.Role;
                Settings.Settings.Default.PlantCode = response.Item.PlantCode;
                Settings.Settings.Default.PlantName = response.Item.PlantDescription;
                Settings.Settings.Default.AuthorizationToken = response.Item.Token;
                IsLogged = true;
            }
            return response;
        }
        public async Task<Response<bool>> LogoutAsync(string userId)
        {
            var response = await remoteAPI.LogoutAsync(userId);
            if (response.Success)
            {
                await secureBlobCache.InvalidateAll();
                Settings.Settings.Default.LoggedUserId = "";
                Settings.Settings.Default.LoggedUserFullName = "";
                Settings.Settings.Default.LoggedUserRole = RolesEnum.AUTISTA;
                Settings.Settings.Default.PlantName = "";
                Settings.Settings.Default.PlantName = "";
                Settings.Settings.Default.AuthorizationToken = null;
                IsLogged = false;

            }
            return response;
        }

        public async Task ForceUserLogout()
        {
            await secureBlobCache.InvalidateAll(); 
            Settings.Settings.Default.LoggedUserId = "";
            Settings.Settings.Default.LoggedUserFullName = "";
            Settings.Settings.Default.LoggedUserRole = RolesEnum.AUTISTA;
            Settings.Settings.Default.PlantName = "";
            Settings.Settings.Default.PlantName = "";
            Settings.Settings.Default.AuthorizationToken = null;
            IsLogged = false;
        }

        public async Task<Response<TruckDriverMatrix>> GetDefaultTruckDriverMatrix()
        {
            return await remoteAPI.GetTruckDriverDefaultMatrix();
        }
        public async Task<Response<CausalsList>> GetCausals(CausalTypeEnum filter)
        {
            var responseModel = new Response<CausalsList> { Item = null, Success = false, Message = "Errore durante il caricamento dei dati!" };
            switch (filter)
            {
                case CausalTypeEnum.VACATION:
                    responseModel = await remoteAPI.GetVacationCausals();
                    break;
                case CausalTypeEnum.REVOCATION:
                    responseModel = await remoteAPI.GetRevocationCausals();
                    break;
            }
            return responseModel;
        }

        public async Task<Response<bool>> CreateVacation(VacationModel model)
        {
            return await remoteAPI.CreateVacation(model);
        }
        public async Task<Response<bool>> UpdateVacation(VacationModel model)
        {
            return await remoteAPI.UpdateVacation(model);
        }
        public async Task<Response<bool>> RevokeVacation(VacationModel model)
        {
            return await remoteAPI.RevokeVacation(model);
        }
        public async Task<Response<VacationsList>> GetVacationsList(DateTime startingDate, DateTime endingDate)
        {
            return await remoteAPI.GetVacationsList(startingDate, endingDate);
        }

        public async Task<Response<PlanningsList>> GetPlanningsListForDay(DateTime day)
        {
            return await remoteAPI.GetPlanningsForDay(day);
        }
        public async Task<Response<List<DriverModel>>> GetAvailibleDriversForDay(DateTime day)
        {
            return await remoteAPI.GetAvailibleDriversForDay(day);
        }
        public async Task<Response<PlanningsList>> GetDriverNotifications()
        {
            return await remoteAPI.GetDriverNotifications();
        }

        public async Task<Response<TruckDriverMatrix>> GetTruckDriverMatrixForPlanning(DateTime day)
        {
            var a = await remoteAPI.GetTruckDriverMatrixForPlanning(day);
            return a;
        }

        public async Task<Response<PlantsList>> GetPlantList()
        {
            return await remoteAPI.GetPlantList();
        }
        public async Task<Response<bool>> CreatePlanning(Planning model)
        {
            return await remoteAPI.CreatePlanning(model);
        }
        public async Task<Response<bool>> UpdatePlanning(Planning model)
        {
            return await remoteAPI.UpdatePlanning(model);
        }
        public async Task<Response<bool>> RevokePlanning(PlanningRevoke model)
        {
            return await remoteAPI.RevokePlanning(model);
        }

        public async Task<Response<WorkingHoursList>> GetWorkingHoursList(DateTime day)
        {
            return await remoteAPI.GetWorkingHoursForDay(day);
        }
        public async Task<Response<AnomalyList>> GetAnomalies()
        {
            return await remoteAPI.GetAnomalies();
        }
        public async Task<Response<bool>> CompleteAnomaly(AnomalyConfirmation model)
        {
            return await remoteAPI.CompleteAnomaly(model); 
        }

        public async Task<Response<bool>> TransferAnomaly(AnomalyTransfer model)
        {
            return await remoteAPI.TransferAnomaly(model);
        }
       
        public async Task<Response<bool>> UnlinkTruckDriver(TruckDriverUnlinkModel model)
        {
            return await remoteAPI.UnlinkTruckDriver(model);
        }

        public async Task<Response<bool>> DriverPlanningConfirmation(DriverPlanningConfirmation model)
        {
            return await remoteAPI.DriverPlanningConfirmation(model);
        }

        private DateTimeOffset GetOffset(int minutes)
        {
            var date = DateTime.Today.AddMinutes(minutes);
            return new DateTimeOffset(date, TimeZoneInfo.Local.GetUtcOffset(date));
        }

    }
}
