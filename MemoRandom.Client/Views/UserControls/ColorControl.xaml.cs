using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MemoRandom.Client.Views.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ColorControl.xaml
    /// </summary>
    public partial class ColorControl : UserControl
    {
        //public static DependencyProperty SelectedColorProperty =
        //    DependencyProperty.Register("SelectedColor", typeof(ColorControl), typeof(Color), new UIPropertyMetadata(null));

        //public Color SelectedColor
        //{
        //    get { return (Color)GetValue(SelectedColorProperty); }
        //    set { SetValue(SelectedColorProperty, value); }
        //}





        public ColorControl()
        {
            InitializeComponent();
            cmbColors.ItemsSource = typeof(Colors).GetProperties();
        }
    }
}
