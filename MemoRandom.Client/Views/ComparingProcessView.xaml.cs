using MemoRandom.Client.ViewModels;
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
using System.Windows.Shapes;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для ComparingProcessView.xaml
    /// </summary>
    public partial class ComparingProcessView : Window
    {
        private ComparingProcessViewModel _vm;

        public ComparingProcessView(ComparingProcessViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;
            _vm = vm;
            this.Loaded += ComparingProcessView_Loaded;
        }

        private void ComparingProcessView_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.GetStackPanel(ProgressStackPanel);
        }
    }
}
