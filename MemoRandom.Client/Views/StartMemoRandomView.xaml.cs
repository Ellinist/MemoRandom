using MahApps.Metro.Controls;
using MemoRandom.Client.ViewModels;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для StartMemoRandom.xaml
    /// </summary>
    public partial class StartMemoRandomView : MetroWindow
    {
        private void MetroWindow_ContentRendered(object sender, System.EventArgs e)
        {
            Activate();
        }

        public StartMemoRandomView(StartMemoRandomViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
