using System;
using System.Reactive.Subjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ShoutrGui.BroadcastSliver;
using ShoutrGui.DataModel;
using ShoutrGui.Dispatcher;

namespace GuiTests3.BroadcastSliver
{
    [TestClass]
    public class BroadcastSliverViewmodelTests : IDisposable
    {
        private MockRepository mockRepository;
        private Mock<IDispatcher> _dispatcher;
        private Mock<IObservable<SliverChangedParams>> _sliverChangedObservable;
        private Mock<IDisposable> _sliverChangedSubscription;


        public BroadcastSliverViewmodelTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            _dispatcher = mockRepository.Create<IDispatcher>();
            _sliverChangedObservable = mockRepository.Create<IObservable<SliverChangedParams>>();
            _sliverChangedSubscription = mockRepository.Create<IDisposable>();
        }

        public void Dispose()
        {
            this.mockRepository.VerifyAll();
        }

        private BroadcastSliverViewmodel CreateBroadcastSliverViewmodel(Action<SliverViewmodelParams> sliverParamsCallsback = null)
        {
            var sliverViewmodelParams = new SliverViewmodelParams(_dispatcher.Object);
            sliverParamsCallsback?.Invoke(sliverViewmodelParams);
            return new BroadcastSliverViewmodel(sliverViewmodelParams, _sliverChangedObservable.Object);
        }

        [TestMethod]
        public void BroadcastSliverViewmodel_Initial_InitialColor()
        {
            // Arrange
            var sliverViewmodelParams = new SliverViewmodelParams(_dispatcher.Object);
            _sliverChangedObservable
                .Setup(sco => sco.Subscribe(It.IsAny<IObserver<SliverChangedParams>>()))
                .Returns(_sliverChangedSubscription.Object);
            
            // Act
            var unitUnderTest = new BroadcastSliverViewmodel(sliverViewmodelParams, _sliverChangedObservable.Object);

            // Assert
            Assert.AreEqual(BroadcastSliverViewmodel.Initial, unitUnderTest.Color);
        }

        [TestMethod]
        public void BroadcastSliverViewmodel_AfterSuccess_SuccessColor()
        {
            // Arrange
            var sliverViewmodelParams = new SliverViewmodelParams(_dispatcher.Object);
            _dispatcher
                .Setup(d=>d.Invoke(It.IsAny<Action>()))
                .Callback<Action>(a=>a())
                .Verifiable();
            
            // Act
            var unitUnderTest = new BroadcastSliverViewmodel(sliverViewmodelParams, new BehaviorSubject<SliverChangedParams>(new SliverChangedParams{Success = true}));

            System.Threading.Thread.Sleep(2);

            // Assert
            Assert.AreEqual(BroadcastSliverViewmodel.Complete, unitUnderTest.Color);
        }
    }
}
