using System;
using System.Windows;
using Microsoft.Practices.Unity;
using WpfPractice.BroadcastSliver;
using WpfPractice.DataModel;
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
            unityContainer.RegisterType<IListenService, ListenService>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<IBroadcastSliverService, BroadcastSliverService>(
                new ContainerControlledLifetimeManager());
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
                    var sliverService = c.Resolve<IBroadcastSliverService>();
                    return new BroadcastSliverViewmodel(sliverService,
                        param,
                        changed);
                })));
            unityContainer.RegisterTypes(
                AllClasses.FromLoadedAssemblies(),
                WithMappings.FromMatchingInterface,
                WithName.Default);
        }
    }

}
