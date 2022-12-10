using Prism.DryIoc;
using Prism.Ioc;
using Prism.Events;
using NLog;
using MemoRandom.Client.Views;
using MemoRandom.Data.Interfaces;
using MemoRandom.Data.Implementations;
using AutoMapper;
using MemoRandom.Client.Common.Mappers;
using MemoRandom.Client.Common.Interfaces;
using MemoRandom.Client.Common.Implementations;

namespace MemoRandom.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private static ILogger _logger;

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

            containerRegistry.RegisterInstance<IMapper>(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MemoRandomMappingProfile());
            }).CreateMapper());

            containerRegistry.RegisterSingleton<ICommonDataController, CommonDataController>();

            #region Контроллеры работы с внешними хранилищами информации
            // Регистрация интерфейса работы с базой данных MS SQL SERVER
            containerRegistry.RegisterSingleton<IMsSqlController, MsSqlController>();
            containerRegistry.RegisterSingleton<IXmlController, XmlController>();
            #endregion
        }
    }
}
