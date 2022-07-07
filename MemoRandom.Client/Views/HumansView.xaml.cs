using System.Windows;
using DryIoc;
using MahApps.Metro.Controls;
using MemoRandom.Client.ViewModels;
using MemoRandom.Models.Common;
using Prism.Regions;

namespace MemoRandom.Client.Views
{
    /// <summary>
    /// Логика взаимодействия для HumansView.xaml
    /// </summary>
    public partial class HumansView : MetroWindow
    {
        void SetRegionManager(IRegionManager regionManager, DependencyObject regionTarget, string regionName)
        {
            RegionManager.SetRegionName(regionTarget, regionName);
            RegionManager.SetRegionManager(regionTarget, regionManager);
        }

        private void MetroWindow_ContentRendered(object sender, System.EventArgs e)
        {
            Activate();
        }

        #region CTOR
        public HumansView(IContainer container, HumansViewModel vm)
        {
            InitializeComponent();

            var regionManager = container.Resolve<IRegionManager>();
            if (regionManager != null)
            {
                // Установка регионов
                SetRegionManager(regionManager, WorkRegion, RegionNames.WorkRegion);
                SetRegionManager(regionManager, MenuRegion, RegionNames.MenuRegion);
            }

            DataContext = vm;
        }
        #endregion
    }
}
