using System.Windows;
using System.Windows.Input;

namespace WpfPractice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IMainWindowViewmodel mainWindowViewmodel)
        {
            DataContext = mainWindowViewmodel;
            InitializeComponent();
            
        }

        
    }
}
