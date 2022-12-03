using System.Windows;
using System.Windows.Controls;

namespace MemoRandom.Client.Views.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ComparedBlockControl.xaml
    /// </summary>
    public partial class ComparedBlockControl : UserControl
    {
        public static DependencyProperty ComparedHumanFullNameProperty =
            DependencyProperty.Register("ComparedHuman", typeof(string), typeof(ComparedBlockControl), null);

        public string ComparedHumanFullName
        {
            get { return (string)GetValue(ComparedHumanFullNameProperty); }
            set { SetValue(ComparedHumanFullNameProperty, value); }
        }

        public ComparedBlockControl()
        {
            InitializeComponent();
        }
    }
}
