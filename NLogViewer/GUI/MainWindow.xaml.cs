using System.Windows;

namespace NLogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NlogViewerViewModel _viewModel;
        public MainWindow()
        {
            _viewModel = new NlogViewerViewModel();
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
