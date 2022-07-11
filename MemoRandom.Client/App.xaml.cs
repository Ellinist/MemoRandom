using Prism.DryIoc;
using DryIoc;
using Prism.Ioc;
using Prism.Events;
using NLog;
using MemoRandom.Client.Views;
using MemoRandom.Data.Interfaces;
using MemoRandom.Data.Implementations;

namespace MemoRandom.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private static ILogger? _logger;

        /// <summary>
        /// Создание оболочки приложения
        /// </summary>
        /// <returns></returns>
        protected override StartMemoRandomView CreateShell()
        {
            _logger?.Info("MemoRandom started");
            return Container.Resolve<StartMemoRandomView>();
        }

        /// <summary>
        /// Регистрация типов
        /// </summary>
        /// <param name="containerRegistry"></param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            _logger = LogManager.GetCurrentClassLogger();
            containerRegistry.RegisterInstance<ILogger>(_logger);
            containerRegistry.RegisterInstance<IEventAggregator>(new EventAggregator());


            #region Контроллеры работы с внешними хранилищами информации

            containerRegistry.Register<IMemoRandomDbController, MemoRandomDbController>();

            #endregion
            
            
            // Регистрация диалогового модуля
            //containerRegistry.Register<IDialogServices, DialogServices>();

            // Подключение контроллера работы с файлами XML
            //containerRegistry.Register<IXmlSettingsController, XmlSettingsController>();
        }
    }
}
