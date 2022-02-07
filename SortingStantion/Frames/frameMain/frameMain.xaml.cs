using System.Windows.Controls;

namespace SortingStantion.frameMain
{
    /// <summary>
    /// Логика взаимодействия для frameMain.xaml
    /// </summary>
    public partial class frameMain : UserControl
    {
        public frameMain()
        {
            InitializeComponent();
            DataBridge.UserMessagePresenter = this.msgConteiner;
        }

        private void Button_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var mainscreen = DataBridge.MainScreen;

            if (mainscreen.WindowState == System.Windows.WindowState.Normal)
            {
                mainscreen.WindowState = System.Windows.WindowState.Maximized;
                return;
            }

            if (mainscreen.WindowState == System.Windows.WindowState.Maximized)
            {
                mainscreen.WindowState = System.Windows.WindowState.Normal;
                return;
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                DataBridge.MainScreen.DragMove();
            }
            catch
            {

            }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
