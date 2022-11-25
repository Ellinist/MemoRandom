using MemoRandom.Client.Common.Implementations;
using MemoRandom.Client.Common.Models;
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

            // Цикл по всем людям для сравнения
            foreach(var human in CommonDataController.ComparedHumansCollection)
            {
                // Каждый человек для сравнения в своем потоке
                Thread thread = new Thread(ProgressMethod);
                thread.Start(human);
            }
        }

        private void ProgressMethod(object humanObject)
        {
            var currentHuman = humanObject as ComparedHuman;
            if (currentHuman == null) return;

            ProgressDispatcher.Invoke(() =>
            {
                // Для каждого человека для сравнения создаем свой UC
                ComparedBlockControlViewModel humanVm = new();
                ProgressStackPanel.Children.Add(new ComparedBlockControl(humanVm, currentHuman, ProgressDispatcher));
            });
        }

        #region CTOR
        public ComparingProcessViewModel()
        {
            ProgressDispatcher = Dispatcher.CurrentDispatcher;
        }
        #endregion
    }
}
