using MemoRandom.Client.Common.Implementations;
using MemoRandom.Client.Views.UserControls;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Модель представления для хранения UC с прогрессом
    /// </summary>
    public class ComparingProcessViewModel : BindableBase
    {
        public Dispatcher ProgressDispatcher { get; set; }

        public Window ProgressView { get; set; }

        public StackPanel ProgressStackPanel { get; set; }

        public void GetStackPanel(StackPanel panel)
        {
            ProgressStackPanel = panel;

            ProgressMethod();
            //Thread thread = new Thread(ProgressMethod);
            //thread.Start();
        }

        private void ProgressMethod()
        {
            #region Блок прогресса по одному человеку
            var firstHuman = CommonDataController.HumansList[0];

            ComparedBlockControlViewModel humanVm = new();

            ProgressStackPanel.Children.Add(new ComparedBlockControl(humanVm, firstHuman, ProgressDispatcher));
            //ProgressDispatcher.Invoke(() =>
            //{
            //    ProgressStackPanel.Children.Add(new ComparedBlockControl(humanVm, firstHuman, ProgressDispatcher));
            //});
            #endregion
        }

        #region CTOR
        public ComparingProcessViewModel()
        {
            ProgressDispatcher = Dispatcher.CurrentDispatcher;
        }
        #endregion
    }
}
