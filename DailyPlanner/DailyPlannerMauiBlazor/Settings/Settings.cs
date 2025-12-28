using DailyPlannerMauiBlazor.Models.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DailyPlannerMauiBlazor.Settings
{
	public class Settings : INotifyPropertyChanged
	{
		public readonly static Settings Default = new Settings();

		public string LoggedUserId { get; set; }
		public string LoggedUsername { get; set; }
		public string LoggedUserFullName { get; set; }
		public RolesEnum LoggedUserRole { get; set; }
		public string PlantCode { get; set; }
		public string PlantName { get; set; }
		public string _authorizationToken { get; set; } 
		

		public string AuthorizationToken
		{
			get { return _authorizationToken; }
			set
			{
				_authorizationToken = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AuthorizationToken)));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
