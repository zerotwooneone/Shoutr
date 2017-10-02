using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Unity;
using WpfPractice.Listen;

namespace WpfPractice
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IUnityContainer _unityContainer;

        public App()
        {
            _unityContainer = new UnityContainer();
            SetupContainer(_unityContainer);

            var mainWindow = _unityContainer.Resolve<MainWindow>();
            mainWindow.Show();
        }

        public static void SetupContainer(IUnityContainer unityContainer)
        {
            var hierarchicalLifetimeManager = new HierarchicalLifetimeManager();
            unityContainer.RegisterType<IListenService>(hierarchicalLifetimeManager);
            unityContainer.RegisterType<Func<Guid, IBroadcastViewmodel>>(
                new InjectionFactory(c => new Func<Guid, IBroadcastViewmodel>(broadcastId =>
             {
                 var listenService = c.Resolve<IListenService>();
                 return new BroadcastViewmodel(listenService, broadcastId);
             })));

            unityContainer.RegisterTypes(
                AllClasses.FromLoadedAssemblies(),
                WithMappings.FromMatchingInterface,
                WithName.Default);
        }
    }

}
