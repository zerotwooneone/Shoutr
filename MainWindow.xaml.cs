using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfPractice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IResizeService _resizeService;

        public MainWindow(IMainWindowViewmodel mainWindowViewmodel,
            IResizeService resizeService)
        {
            _resizeService = resizeService;
            DataContext = mainWindowViewmodel;
            InitializeComponent();
        }

        private void SliverPanel_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                _resizeService.OnSliverPanelResized(e);
                Console.WriteLine($"mainwindow resize:{_resizeService.GetHashCode()}");
            }
        }
    }
}
