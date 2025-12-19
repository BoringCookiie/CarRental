using System.Windows;
using CarRental.BackOffice.ViewModels;

namespace CarRental.BackOffice.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
