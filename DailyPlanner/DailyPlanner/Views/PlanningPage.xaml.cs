using Xamarin.Forms;

namespace DailyPlanner.Views
{
    public partial class PlanningPage : ContentPage
    {
        public PlanningPage()
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
