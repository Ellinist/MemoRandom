using MemoRandom.Client.Common.Implementations;
using MemoRandom.Client.Views.UserControls;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Модель представления для хранения UC с прогрессом
    /// </summary>
    public class ComparingProcessViewModel : BindableBase
    {
        public Window ProgressView { get; set; }

        public void GetStackPanel(StackPanel panel)
        {
            #region Блок прогресса по одному человеку
            var firstHuman = CommonDataController.HumansList[0];
            ComparedBlockControlViewModel humanVm = new();
            panel.Children.Add(new ComparedBlockControl(humanVm, firstHuman));
            #endregion

        }



        #region CTOR
        public ComparingProcessViewModel()
        {

        }
        #endregion
    }
}
