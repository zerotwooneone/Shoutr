using Library.Reactive;
using Microsoft.Reactive.Testing;
using Moq;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Library.Tests.Reactive
{
    public class ObservableExtensionsTests : IDisposable
    {
        private readonly MockRepository _mockRepository;
        private readonly TestScheduler _testScheduler;

        public ObservableExtensionsTests()
        {
            this._mockRepository = new MockRepository(MockBehavior.Strict);
            _testScheduler = new TestScheduler();
        }

        public void Dispose()
        {
            this._mockRepository.VerifyAll();
        }

        [Fact]
        public void TickingThrottle_EmitsValue_WithoutWaiting()
        {
            // Arrange
            const int expected = 5;
            var source = Observable.Return(expected).Concat(Observable.Never<int>());
            var timeBetweenEmits = TimeSpan.FromSeconds(1);
            
            int? latestValue = null;

            // Act
            source.TickingThrottle(
                timeBetweenEmits,
                _testScheduler)
                .Subscribe(v=>{ latestValue = v; });

            // Assert
            Assert.Equal(expected, latestValue);
        }

        [Fact]
        public void TickingThrottle_EmitsValue_AfterWaitingForFirstValue()
        {
            // Arrange
            const int expected = 5;
            var timeBetweenEmits = TimeSpan.FromSeconds(1);
            TimeSpan timeBeforeFirstValue = timeBetweenEmits * 0.9;
            var source = DelayValues<int>((expected, timeBeforeFirstValue));
                        
            int? latestValue = null;

            // Act
            source.TickingThrottle(
                timeBetweenEmits,
                _testScheduler)
                .Subscribe(v=>{ latestValue = v; });

            _testScheduler.AdvanceBy(timeBeforeFirstValue);

            // Assert
            Assert.Equal(expected, latestValue);
        }

        [Fact]
        public void TickingThrottle_DoesNotEmitsSecondValue_AfterWaitingSmallSecondValue()
        {
            // Arrange
            const int expected = 5;
            var timeBetweenEmits = TimeSpan.FromSeconds(1);
            TimeSpan timeBeforeSecondValue = timeBetweenEmits * 0.9;
            var source = DelayValues(
                (expected, TimeSpan.Zero),
                (99, timeBeforeSecondValue));
                        
            int? latestValue = null;

            // Act
            var observable = source.TickingThrottle(
                timeBetweenEmits,
                _testScheduler);

            observable.Subscribe(v=>{ 
                latestValue = v; 
            }); 
            
            _testScheduler.AdvanceBy(timeBeforeSecondValue);

            // Assert
            Assert.Equal(expected, latestValue);
        }

        [Fact]
        public void TickingThrottle_EmitsSecondValue_AfterWaitingLargeSecondValue()
        {
            // Arrange
            const int expected = 5;
            var timeBetweenEmits = TimeSpan.FromSeconds(1);
            TimeSpan timeBeforeSecondValue = timeBetweenEmits * 1.1;
            var source = DelayValues(
                (99, TimeSpan.Zero),
                (expected, timeBeforeSecondValue));
                        
            int? latestValue = null;

            // Act
            source.TickingThrottle(
                timeBetweenEmits,
                _testScheduler).Subscribe(v=>{ 
                latestValue = v; 
            });     
            
            _testScheduler.AdvanceBy(timeBeforeSecondValue);

            // Assert
            Assert.Equal(expected, latestValue);
        }

        [Fact]
        public void TickingThrottle_EmitsSecondValueOnce_AfterExcessiveWaiting()
        {
            // Arrange
            const int target = 5;
            var timeBetweenEmits = TimeSpan.FromSeconds(1);
            TimeSpan timeBeforeSecondValue = timeBetweenEmits * 1.1;
            var source = DelayValues(
                (99, TimeSpan.Zero),
                (target, timeBeforeSecondValue));
                        
            int actual = 0;
            const int expected = 1;

            // Act
            source.TickingThrottle(
                timeBetweenEmits,
                _testScheduler).Subscribe(v=>{
                    if (v == target)
                    {
                        actual++;
                    }
                });     
            
            _testScheduler.AdvanceBy(timeBeforeSecondValue*10);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TickingThrottle_EmitsSecondValueOnce_AfterExcessiveWaitingWithThrid()
        {
            // Arrange
            const int target = 5;
            var timeBetweenEmits = TimeSpan.FromSeconds(1);
            TimeSpan timeBeforeSecondValue = timeBetweenEmits * 1.1;
            var source = DelayValues(
                (99, TimeSpan.Zero),
                (target, timeBeforeSecondValue),
                (100, TimeSpan.Zero));
                        
            int actual = 0;
            const int expected = 1;

            // Act
            source.TickingThrottle(
                timeBetweenEmits,
                _testScheduler).Subscribe(v=>{
                    if (v == target)
                    {
                        actual++;
                    }
                });     
            
            _testScheduler.AdvanceBy(timeBeforeSecondValue*10);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TickingThrottle_EmitsSecondValue_AfterWaitingLargeSecondValueWithThird()
        {
            // Arrange
            const int expected = 5;
            var timeBetweenEmits = TimeSpan.FromSeconds(1);
            TimeSpan timeBeforeSecondValue = timeBetweenEmits * 1.1;
            var source = DelayValues(
                (99, TimeSpan.Zero),
                (expected, timeBeforeSecondValue),
                (199, TimeSpan.Zero));
                        
            int? latestValue = null;

            // Act
            source.TickingThrottle(
                timeBetweenEmits,
                _testScheduler).Subscribe(v=>{ 
                latestValue = v; 
            });     
            
            _testScheduler.AdvanceBy(timeBeforeSecondValue);

            // Assert
            Assert.Equal(expected, latestValue);
        }

        [Fact]
        public void TickingThrottle_DoesNotEmitSecondValue_WithoutWaiting()
        {
            // Arrange
            const int expected = 5;
            var source = new[]{ expected,  99 }.ToObservable().Concat(Observable.Never<int>());
            var timeBetweenEmits = TimeSpan.FromSeconds(1);
            
            int? latestValue = null;

            // Act
            source.TickingThrottle(
                timeBetweenEmits,
                _testScheduler)
                .Subscribe(v=>{ latestValue = v; });

            // Assert
            Assert.Equal(expected, latestValue.Value);
        }

        [Fact]
        public void TickingThrottle_EmitSecondValue_AfterTimeBetweenEmits()
        {
            // Arrange
            const int expected = 5;
            var source = (new[]{ 99, expected }).ToObservable().Concat(Observable.Never<int>());
            var timeBetweenEmits = TimeSpan.FromSeconds(1);
            
            int? latestValue = null;

            // Act
            var observable = source.TickingThrottle(
                timeBetweenEmits,
                _testScheduler)
                .Subscribe(v=>{ 
                    latestValue = v; 
                });

            _testScheduler.AdvanceBy(timeBetweenEmits);

            // Assert
            Assert.Equal(expected, latestValue.Value);
        }

        [Fact]
        public void TickingThrottle_DoesNotEmitSecondValue_JustBeforeTimeBetweenEmits()
        {
            // Arrange
            const int expected = 5;

            var source = new[]{ expected, 99 }.ToObservable().Concat(Observable.Never<int>());
            var timeBetweenEmits = TimeSpan.FromSeconds(1);
            
            int? latestValue = null;

            // Act
            source.TickingThrottle(
                timeBetweenEmits,
                _testScheduler)
                .Subscribe(v=>{ 
                    if(v== expected)
                    {
                        _testScheduler.AdvanceBy(timeBetweenEmits*0.9);
                    }                    
                    latestValue = v; 
                });

            // Assert
            Assert.Equal(expected, latestValue.Value);
        }

        internal IObservable<T> DelayValues<T>(params (T value,TimeSpan delay)[] values)
        {
            return Observable.Concat(values.Select(tuple =>
            {
                if(tuple.delay > TimeSpan.Zero)
                {
                    return Observable.Return(tuple.value).Delay(tuple.delay, _testScheduler);
                }
                return Observable.Return(tuple.value);
            }));
        }
    }
}
