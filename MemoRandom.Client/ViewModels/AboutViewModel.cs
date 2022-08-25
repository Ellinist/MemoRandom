using Prism.Commands;
using Prism.Mvvm;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Модель представления для окна "О программе"
    /// </summary>
    public class AboutViewModel : BindableBase
    {
        private Window _view; // Окно
        private string _programVersion;
        
        public string ProgramVersion
        {
            get => _programVersion;
            set
            {
                _programVersion = value;
                RaisePropertyChanged(nameof(ProgramVersion));
            }
        }

        #region COMMANDS
        public DelegateCommand<object> OpenAboutWindowCommand { get; private set; }
        public DelegateCommand CloseAboutViewCommand { get; private set; }
        #endregion

        #region COMMAND IMPLEMENTATION
        /// <summary>
        /// Загрузка окна "О программе"
        /// </summary>
        /// <param name="parameter"></param>
        private void OpenAboutWindow(object parameter)
        {
            var view = parameter as Window;
            if (view != null)
            {
                _view = parameter as Window;
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            ProgramVersion = "Build " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// Закрытие окна "О программе"
        /// </summary>
        private void CloseAboutView()
        {
            _view.Close();
        }
        #endregion


        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitializeCommands()
        {
            OpenAboutWindowCommand = new DelegateCommand<object>(OpenAboutWindow);
            CloseAboutViewCommand = new DelegateCommand(CloseAboutView);
        }

        #region CTOR
        /// <summary>
        /// Конструктор
        /// </summary>
        public AboutViewModel()
        {
            InitializeCommands();
        }
        #endregion
    }
}
