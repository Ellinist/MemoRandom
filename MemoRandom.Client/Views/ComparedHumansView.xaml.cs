using MahApps.Metro.Controls;
using MemoRandom.Client.ViewModels;
using System.Windows.Controls;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для ComparedHumansView.xaml
    /// </summary>
    public partial class ComparedHumansView : MetroWindow
    {
        private void DgComparedHumans_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DgComparedHumans.Focus();
        }

        #region CTOR
        public ComparedHumansView(ComparedHumansViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;
            PersonImage.MouseLeftButtonDown += vm.PersonImage_MouseLeftButtonDown;
            PersonImage.MouseMove += vm.PersonImage_MouseMove;
            SourceCanvas.MouseWheel += vm.SourceCanvas_MouseWheel;
            this.Loaded += vm.ComparedHumansView_Loaded;
            DgComparedHumans.SelectionChanged += DgComparedHumans_SelectionChanged;
        }
        #endregion
    }
}
