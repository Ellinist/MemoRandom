using MahApps.Metro.Controls;
using MemoRandom.Client.ViewModels;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Windows.Controls;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для ReasonsView.xaml
    /// </summary>
    public partial class ReasonsView : MetroWindow
    {





        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="vm"></param>
        public ReasonsView(ReasonsViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
        #endregion
    }
}
