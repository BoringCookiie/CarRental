using System.Windows;
using CarRental.BackOffice.ViewModels;

namespace CarRental.BackOffice.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
