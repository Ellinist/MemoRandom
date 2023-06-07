using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Reflection;
using System.Windows;

namespace MemoRandom.Client.ViewModels;

/// <summary>
/// Модель представления для окна "О программе"
/// </summary>
public class AboutViewModel : BindableBase
{
    private Window _view; // Окно
    private string _programVersion;
    private string _copyRight = $"Ellinist Software Studio © 2001-{DateTime.Now.Year}";
        
    public string ProgramVersion
    {
        get => _programVersion;
        set
        {
            _programVersion = value;
            RaisePropertyChanged(nameof(ProgramVersion));
        }
    }

    public string CopyRight
    {
        get => _copyRight;
        set
        {
            _copyRight = value;
            RaisePropertyChanged(nameof(CopyRight));
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
        if (parameter is not Window view) return;

        _view = view;
        ProgramVersion = "Build " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        RaisePropertyChanged(nameof(ProgramVersion));
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