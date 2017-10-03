using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Unity;
using WpfPractice.BroadcastSliver;
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
            unityContainer.RegisterType<IListenService,ListenService>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<Func<Guid, IBroadcastViewmodel>>(
                new InjectionFactory(c => new Func<Guid, IBroadcastViewmodel>(broadcastId =>
             {
                 var listenService = c.Resolve<IListenService>();
                 return new BroadcastViewmodel(listenService, broadcastId);
             })));
            unityContainer.RegisterType<Func<Guid, uint, IBroadcastSliverViewmodel>>(
                new InjectionFactory(c => new Func<Guid, uint, IBroadcastSliverViewmodel>((broadcastId, sliverIndex) =>
                {
                    var sliverService = c.Resolve<IBroadcastSliverService>();
                    return new BroadcastSliverViewmodel(sliverService, broadcastId, sliverIndex);
                })));

            unityContainer.RegisterTypes(
                AllClasses.FromLoadedAssemblies(),
                WithMappings.FromMatchingInterface,
                WithName.Default);
        }
    }

}
