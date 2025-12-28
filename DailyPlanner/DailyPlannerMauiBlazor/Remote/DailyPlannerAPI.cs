using DailyPlannerMauiBlazor.Models;
using DailyPlannerMauiBlazor.Models.NetworkModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DailyPlannerMauiBlazor.Remote
{
    public class DailyPlannerAPI : IRemoteAPI
	{
		private readonly string apiBaseURL;
		public string ApiMethodPrefix { get; private set; }

		public DailyPlannerAPI(string _apiBaseURL,string _apiMethodPrefix = null)
        {
			apiBaseURL = _apiBaseURL;
			ApiMethodPrefix = _apiMethodPrefix;
        }

		private HttpClient _httpClient = null;
		public HttpClient HttpClient
		{
			get
			{
				if (_httpClient == null)
				{
					_httpClient = new HttpClient();//new NativeMessageHandler());
					_httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
					_httpClient.Timeout = new TimeSpan(0, 0, 30);
					_httpClient.BaseAddress = new Uri(apiBaseURL);
				}
				if (Settings.Settings.Default.AuthorizationToken != null)
				{
					_httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(ExtendedHTTPHeaders.XA_TOKEN_HEADER, System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Settings.Settings.Default.AuthorizationToken)));
				}
                else
                {
					_httpClient.DefaultRequestHeaders.Authorization = null;
				}
				/*
				if (_httpClient.DefaultRequestHeaders.Authorization == null)
				{
					if(Settings.Settings.Default.AuthorizationToken != null)
                    {
						_httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(ExtendedHTTPHeaders.XA_TOKEN_HEADER, System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Settings.Settings.Default.AuthorizationToken)));
					}
				}
				*/
				return _httpClient;
			}
		}

		public async Task<Response<UserModel>> LoginAsync(string loginName, string password, string fcmToken)
		{
			HttpResponseMessage result = null;
			try
			{
				var content = JsonConvert.SerializeObject(new CredentialsModel { Username = loginName, Password = password, FCMToken = fcmToken });
				HttpClient.DefaultRequestHeaders.Authorization = null;
				result = await HttpClient.PostAsync(ApiMethodPrefix + $"LoginUser", new StringContent(content, Encoding.UTF8, "application/json"));
			}
			catch (Exception x){ 
			}
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<UserModel> { Item = null, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server per l'autenticazione" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<UserModel>>(resp);

				return ret;
			}
		}

		public async Task<Response<bool>> LogoutAsync(string userId)
        {
			HttpResponseMessage result = null;
			try
			{
				var content = JsonConvert.SerializeObject(userId);
				result = await HttpClient.PostAsync(ApiMethodPrefix + $"LogoutUser", new StringContent(content, Encoding.UTF8, "application/json"));
			}
			catch (Exception x)
			{
			}
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<bool> { Item = false, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server per l'autenticazione" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<bool>>(resp);

				return ret;
			}
		}
		public async Task<Response<CausalsList>> GetRevocationCausals()
		{
			HttpResponseMessage result = null;
			try
			{
				result = await HttpClient.GetAsync(ApiMethodPrefix + $"RevocationCausals");
			}
			catch { }
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<CausalsList> { Item = null, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server per recuperare i dati!\r\nControllare la connessione e riprovare!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<CausalsList>>(resp);

				return ret;
			}
		}
		public async Task<Response<CausalsList>> GetVacationCausals()
        {
			HttpResponseMessage result = null;
			try
			{
				result = await HttpClient.GetAsync(ApiMethodPrefix + $"VacationCausals");
			}
			catch { }
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<CausalsList> { Item = null, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server per recuperare i dati!\r\nControllare la connessione e riprovare!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<CausalsList>>(resp);

				return ret;
			}
		}
		public async Task<Response<TruckDriverMatrix>> GetTruckDriverDefaultMatrix()
        {
			HttpResponseMessage result = null;
			try
			{
				result = await HttpClient.GetAsync(ApiMethodPrefix + $"DefaultTruckDriverMatrix?Id="+Settings.Settings.Default.LoggedUserId);
			}
			catch(Exception x)
			{
			}
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<TruckDriverMatrix> { Item = null, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server per recuperare i dati!\r\nControllare la connessione e riprovare!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<TruckDriverMatrix>>(resp);

				return ret;
			}
		}
		public async Task<Response<bool>> CreateVacation(VacationModel model)
        {
			HttpResponseMessage result = null;
			try
			{
				var content = JsonConvert.SerializeObject(model);
				HttpClient.DefaultRequestHeaders.Authorization = null;
				result = await HttpClient.PostAsync(ApiMethodPrefix + $"CreateVacation", new StringContent(content, Encoding.UTF8, "application/json"));
			}
			catch { }
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<bool> { Item = false, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<bool>>(resp);

				return ret;
			}
		}

		public async Task<Response<bool>> UpdateVacation(VacationModel model)
		{
			HttpResponseMessage result = null;
			try
			{
				var content = JsonConvert.SerializeObject(model);
				HttpClient.DefaultRequestHeaders.Authorization = null;
				result = await HttpClient.PostAsync(ApiMethodPrefix + $"UpdateVacation", new StringContent(content, Encoding.UTF8, "application/json"));
			}
			catch { }
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<bool> { Item = false, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<bool>>(resp);

				return ret;
			}
		}
		public async Task<Response<bool>> RevokeVacation(VacationModel model)
		{
			HttpResponseMessage result = null;
			try
			{
				var content = JsonConvert.SerializeObject(model);
				HttpClient.DefaultRequestHeaders.Authorization = null;
				result = await HttpClient.PostAsync(ApiMethodPrefix + $"RevokeVacation", new StringContent(content, Encoding.UTF8, "application/json"));
			}
			catch { }
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<bool> { Item = false, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<bool>>(resp);

				return ret;
			}
		}

		public async Task<Response<VacationsList>> GetVacationsList(DateTime startingDate, DateTime endingDate)
        {
			HttpResponseMessage result = null;
			try
			{
				result = await HttpClient.GetAsync(ApiMethodPrefix + $"VacationsList?Id=" + Settings.Settings.Default.LoggedUserId + "&startingDateString=" + startingDate.ToString("dd-MM-yyyy") + "&endingDateString=" + endingDate.ToString("dd-MM-yyyy"));
			}
			catch (Exception x)
			{
			}
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<VacationsList> { Item = null, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server per recuperare i dati!\r\nControllare la connessione e riprovare!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<VacationsList>>(resp);

				return ret;
			}
		}

		//TODO CHANGE CALL
		public async Task<Response<PlantsList>> GetPlantList()
		{
			HttpResponseMessage result = null;
			try
			{
				result = await HttpClient.GetAsync(ApiMethodPrefix + $"PlantList?Id=" + Settings.Settings.Default.LoggedUserId);
			}
			catch (Exception x)
			{
			}
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<PlantsList> { Item = null, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server per recuperare i dati!\r\nControllare la connessione e riprovare!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<PlantsList>>(resp);
				return ret;
			}
		}


		public async Task<Response<PlanningsList>> GetPlanningsForDay(DateTime day)
        {
			HttpResponseMessage result = null;
			try
			{
				result = await HttpClient.GetAsync(ApiMethodPrefix + $"TruckDriverMatrixForDay/" + Settings.Settings.Default.LoggedUserId + "?dateString=" + day.ToString("dd-MM-yyyy"));
			}
			catch (Exception x)
			{
			}
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<PlanningsList> { Item = null, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server per recuperare i dati!\r\nControllare la connessione e riprovare!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<PlanningsList>>(resp);
				 return ret;
			}
		}

		public async Task<Response<TruckDriverMatrix>> GetTruckDriverMatrixForPlanning(DateTime day)
        {
			HttpResponseMessage result = null;
			try
			{
				result = await HttpClient.GetAsync(ApiMethodPrefix + $"TruckDriverMatrixForPlanning/" + Settings.Settings.Default.LoggedUserId + "?dateString=" + day.ToString("dd-MM-yyyy"));
			}
			catch (Exception x)
			{
			}
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<TruckDriverMatrix> { Item = null, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server per recuperare i dati!\r\nControllare la connessione e riprovare!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<TruckDriverMatrix>>(resp);
				return ret;
			}
		}
		public async Task<Response<List<DriverModel>>> GetAvailibleDriversForDay(DateTime day)
		{
			HttpResponseMessage result = null;
			try
			{
				result = await HttpClient.GetAsync(ApiMethodPrefix + $"DriversListForDay/" + Settings.Settings.Default.LoggedUserId + "?dateString=" + day.ToString("dd-MM-yyyy"));
			}
			catch (Exception x)
			{
			}
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<List<DriverModel>> { Item = null, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server per recuperare i dati!\r\nControllare la connessione e riprovare!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<List<DriverModel>>>(resp);
				return ret;
			}
		}
		public async Task<Response<PlanningsList>> GetDriverNotifications()
		{
			HttpResponseMessage result = null;
			try
			{//TruckMoveAPI
				result = await HttpClient.GetAsync(ApiMethodPrefix + $"DriverNotifications/" + Settings.Settings.Default.LoggedUserId);
			}
			catch (Exception x)
			{
			}
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<PlanningsList> { Item = null, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server per recuperare i dati!\r\nControllare la connessione e riprovare!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<PlanningsList>>(resp);

				return ret;
			}
		}


		public async Task<Response<WorkingHoursList>> GetWorkingHoursForDay(DateTime day)
		{
			HttpResponseMessage result = null;
			try
			{
				result = await HttpClient.GetAsync(ApiMethodPrefix + $"WorkingHoursListForDay/" + Settings.Settings.Default.LoggedUserId + "?dateString=" + day.ToString("dd-MM-yyyy"));
			}
			catch (Exception x)
			{
			}
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<WorkingHoursList> { Item = null, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server per recuperare i dati!\r\nControllare la connessione e riprovare!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<WorkingHoursList>>(resp);

				return ret;
			}
		}
		public async Task<Response<bool>> CreatePlanning(Planning model)
        {
			HttpResponseMessage result = null;
			try
			{
				var content = JsonConvert.SerializeObject(model);
				HttpClient.DefaultRequestHeaders.Authorization = null;
				result = await HttpClient.PostAsync(ApiMethodPrefix + $"CreatePlanning", new StringContent(content, Encoding.UTF8, "application/json"));
			}
			catch { }
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<bool> { Item = false, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server!\r\nControllare la connessione e riprovare!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<bool>>(resp);
				return ret;
			}
		}
		public async Task<Response<bool>> UpdatePlanning(Planning model)
		{
			HttpResponseMessage result = null;
			try
			{
				var content = JsonConvert.SerializeObject(model);
				HttpClient.DefaultRequestHeaders.Authorization = null;
				result = await HttpClient.PostAsync(ApiMethodPrefix + $"UpdatePlanning", new StringContent(content, Encoding.UTF8, "application/json"));
			}
			catch { }
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<bool> { Item = false, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server!\r\nControllare la connessione e riprovare!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<bool>>(resp);
				return ret;
			}
		}
		public async Task<Response<bool>> RevokePlanning(PlanningRevoke model)
		{
			HttpResponseMessage result = null;
			try
			{
				var content = JsonConvert.SerializeObject(model);
				HttpClient.DefaultRequestHeaders.Authorization = null;
				result = await HttpClient.PostAsync(ApiMethodPrefix + $"RevokePlanning", new StringContent(content, Encoding.UTF8, "application/json"));
			}
			catch { }
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<bool> { Item = false, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server!\r\nControllare la connessione e riprovare!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<bool>>(resp);
				return ret;
			}
		}


		public async Task<Response<AnomalyList>> GetAnomalies()
		{
			HttpResponseMessage result = null;
			try
			{
				result = await HttpClient.GetAsync(ApiMethodPrefix + $"Anomalies?Id=" + Settings.Settings.Default.LoggedUserId);
			}
			catch (Exception x) { }
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<AnomalyList> { Item = null, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server per recuperare i dati!\r\nControllare la connessione e riprovare!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<AnomalyList>>(resp);
				return ret;
			}
		}
		public async Task<Response<bool>> CompleteAnomaly(AnomalyConfirmation model)
		{
			HttpResponseMessage result = null;
			try
			{
				var content = JsonConvert.SerializeObject(model);
				HttpClient.DefaultRequestHeaders.Authorization = null;
				result = await HttpClient.PostAsync(ApiMethodPrefix + $"CloseAnomaly", new StringContent(content, Encoding.UTF8, "application/json"));
			}
			catch { }
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<bool> { Item = false, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<bool>>(resp);
				return ret;
			}
		}
		public async Task<Response<bool>> TransferAnomaly(AnomalyTransfer model)
		{
			HttpResponseMessage result = null;
			try
			{
				var content = JsonConvert.SerializeObject(model);
				HttpClient.DefaultRequestHeaders.Authorization = null;
				result = await HttpClient.PostAsync(ApiMethodPrefix + $"TransferAnomaly", new StringContent(content, Encoding.UTF8, "application/json"));
			}
			catch { }
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<bool> { Item = false, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<bool>>(resp);
				return ret;
			}
		}

		public async Task<Response<bool>> UnlinkTruckDriver(TruckDriverUnlinkModel model)
		{
			HttpResponseMessage result = null;
			try
			{
				var content = JsonConvert.SerializeObject(model);
				HttpClient.DefaultRequestHeaders.Authorization = null;
				result = await HttpClient.PostAsync(ApiMethodPrefix + $"UnlinkTruckDriver", new StringContent(content, Encoding.UTF8, "application/json"));
			}
			catch { }
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<bool> { Item = false, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<bool>>(resp);
				return ret;
			}
		}

		public async Task<Response<bool>> DriverPlanningConfirmation(DriverPlanningConfirmation model)
        {
			HttpResponseMessage result = null;
			try
			{
				var content = JsonConvert.SerializeObject(model);
				HttpClient.DefaultRequestHeaders.Authorization = null;
				result = await HttpClient.PostAsync(ApiMethodPrefix + $"DriverConfirmPlanning", new StringContent(content, Encoding.UTF8, "application/json"));
			}
			catch { }
			if (result?.IsSuccessStatusCode != true)
			{
				return new Response<bool> { Item = false, Success = false, Message = result?.ReasonPhrase ?? "Impossibile raggiungere il server!" };
			}
			else
			{
				var resp = await result.Content.ReadAsStringAsync();
				var ret = JsonConvert.DeserializeObject<Response<bool>>(resp);
				return ret;
			}
		}
	}
}
