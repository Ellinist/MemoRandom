using MahApps.Metro.Controls;
using MemoRandom.Client.ViewModels;
using System.Windows;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для StartMemoRandom.xaml
    /// </summary>
    public partial class StartMemoRandomView/* : MetroWindow*/
    {
        private void MetroWindow_ContentRendered(object sender, System.EventArgs e)
        {
            Activate();
        }

        public StartMemoRandomView(StartMemoRandomViewModel vm)
        {
            //var _ = new Microsoft.Xaml.Behaviors.DefaultTriggerAttribute(typeof(Trigger), typeof(Microsoft.Xaml.Behaviors.TriggerBase), null);

            InitializeComponent();

            DataContext = vm;
        }
    }
}
