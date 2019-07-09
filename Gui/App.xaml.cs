using System;
using System.Windows;
using ShoutrGui.BroadcastSliver;
using ShoutrGui.DataModel;
using ShoutrGui.Listen;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.RegistrationByConvention;

namespace ShoutrGui
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
            unityContainer.RegisterType<IListenService, ListenService>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<Func<BroadcastViewmodelParams, IObservable<SliverChangedParams>, IBroadcastViewmodel>>(
                new InjectionFactory(c => new Func<BroadcastViewmodelParams, IObservable<SliverChangedParams>, IBroadcastViewmodel>((broadcastParams, changed) =>
             {
                 var listenService = c.Resolve<IListenService>();
                 return new BroadcastViewmodel(listenService,
                     broadcastParams,
                     c.Resolve<Func<SliverViewmodelParams, IObservable<SliverChangedParams>, IBroadcastSliverViewmodel>>(),
                     changed);
             })));
            unityContainer.RegisterType<Func<SliverViewmodelParams, IObservable<SliverChangedParams>, IBroadcastSliverViewmodel>>(
                new InjectionFactory(c => new Func<SliverViewmodelParams, IObservable<SliverChangedParams>, IBroadcastSliverViewmodel>((param, changed) =>
                {
                    return new BroadcastSliverViewmodel(param,
                        changed);
                })));
            unityContainer.RegisterTypes(
                AllClasses.FromLoadedAssemblies(),
                WithMappings.FromMatchingInterface,
                WithName.Default);
        }
    }

}
