using Xamarin.Forms;

namespace DailyPlanner.Views
{
    public partial class WorkingHoursPage : ContentPage
    {
        public WorkingHoursPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            //selectedDatePicker.IsVisible = true;
            selectedDatePicker.Focus();
        }
    }
}
