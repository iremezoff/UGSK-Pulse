using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using UGSK.K3.Pulse.Infrastructure;
using UGSK.K3.Pulse.Processors;

namespace UGSK.K3.Pulse.Test
{
    [TestFixture]
    public class PerWeekDailyAverageStatisticProcessorTest
    {
        PerWeekDailyAverageStatisticProcessor _target;
        private Mock<IDataStorage> _mockDataStorage;
        private Mock<IBroadcaster> _mockBroadcaster;
        private const string InitProduct = "product";

        [SetUp]
        public void Init()
        {
            _mockDataStorage = new Mock<IDataStorage>();
            _mockDataStorage.Setup(m => m.GetProducts())
                .Returns(Task.FromResult(new List<string> { InitProduct }.AsEnumerable()));

            _mockBroadcaster = new Mock<IBroadcaster>();

            _target = new PerWeekDailyAverageStatisticProcessor(_mockDataStorage.Object, _mockBroadcaster.Object);
        }

        [Test]
        public void Process_void_PerformedWholeLastWeek()
        {
            var passedDate = new DateTime(2015, 03, 22);
            var startWeekDate = new DateTime(2015, 03, 16);
            int previousAverageWeekStat = 12;
            int passedDateStat = 250;

            _mockDataStorage.Setup(m => m.GetCounterOrDefault(InitProduct, PeriodKind.Weekly, startWeekDate, CounterKind.Average))
                .Returns(Task.FromResult(new Counter
                {
                    Kind = CounterKind.Average,
                    Value = previousAverageWeekStat,
                    PeriodKind = PeriodKind.Weekly,
                    PeriodStart = startWeekDate,
                    Product = InitProduct
                }));
            _mockDataStorage.Setup(
                m => m.GetCounterOrDefault(InitProduct, PeriodKind.Daily, passedDate, CounterKind.Total))
                .Returns(Task.FromResult(new Counter
                {
                    Kind = CounterKind.Total,
                    PeriodKind = PeriodKind.Daily,
                    PeriodStart = passedDate,
                    Product = InitProduct,
                    Value = passedDateStat
                }));

            _mockDataStorage.Setup(m => m.UpdateCounter(It.IsAny<Counter>(), It.IsAny<int>()))
                .Returns((Counter counter, int delta) =>
                {
                    counter.Value += delta;
                    return Task.FromResult(counter);
                });

            _target.ProcessAsync(passedDate).Wait();

            var expectedValue = (previousAverageWeekStat * 6 + passedDateStat) / 7;

            //weekly average stat was updated and closed. The difference is used because UpdateCounter assumes delata, don't forget!
            _mockDataStorage.Verify(m =>
                m.UpdateCounter(It.Is<Counter>(
                    c =>
                        c.Kind == CounterKind.Average &&
                        c.PeriodKind == PeriodKind.Weekly &&
                        c.PeriodStart.Date == startWeekDate &&
                        c.PeriodActualDate == passedDate &&
                        c.Value == expectedValue &&
                        c.IsClosed == true
                    ),
                    It.Is<int>(d => d == expectedValue - previousAverageWeekStat)), Times.Once);

            // neither counter don't have to be updated
            _mockDataStorage.Verify(m => m.UpdateCounter(It.IsAny<Counter>(), It.IsAny<int>()), Times.Once);

            // broadcast new average counter value
            _mockBroadcaster.Verify(
                m =>
                    m.SendCounter(It.Is<CounterMessage>(
                            c =>
                                c.Kind == CounterKind.Average &&
                                c.PeriodKind == PeriodKind.Weekly &&
                                c.PeriodStart == startWeekDate &&
                                c.Product == InitProduct &&
                                c.Value == expectedValue)));
        }

        [Test]
        public void Process_void_PerformedStartedWeek()
        {
            var passedDate = new DateTime(2015, 03, 23);
            var startWeekDate = new DateTime(2015, 03, 23);
            var expectedValue = 25;

            _mockDataStorage.Setup(
                m => m.GetCounterOrDefault(InitProduct, PeriodKind.Weekly, passedDate, CounterKind.Average))
                .Returns(Task.FromResult(new Counter
                {
                    Kind = CounterKind.Average,
                    PeriodKind = PeriodKind.Weekly,
                    PeriodStart = startWeekDate,
                    Product = InitProduct,
                    Value = 0
                }));

            _mockDataStorage.Setup(
                m => m.GetCounterOrDefault(InitProduct, PeriodKind.Daily, passedDate, CounterKind.Total))
                .Returns(Task.FromResult(new Counter
                {
                    Kind = CounterKind.Total,
                    PeriodKind = PeriodKind.Daily,
                    PeriodStart = passedDate,
                    Product = InitProduct,
                    Value = expectedValue
                }));

            _mockDataStorage.Setup(m => m.UpdateCounter(It.IsAny<Counter>(), It.IsAny<int>()))
                .Returns((Counter counter, int delta) =>
                {
                    counter.Value += delta;
                    return Task.FromResult(counter);
                });

            _target.ProcessAsync(passedDate).Wait();

            //the same verification
            _mockDataStorage.Verify(m =>
                m.UpdateCounter(It.Is<Counter>(
                    c =>
                        c.Kind == CounterKind.Average &&
                        c.PeriodKind == PeriodKind.Weekly &&
                        c.PeriodStart.Date == startWeekDate &&
                        c.PeriodActualDate == passedDate &&
                        c.IsClosed == false),
                        It.Is<int>(d => d == expectedValue)), Times.Once);

            _mockDataStorage.Verify(m => m.UpdateCounter(It.IsAny<Counter>(), It.IsAny<int>()), Times.Once);

            _mockBroadcaster.Verify(
               m =>
                   m.SendCounter(It.Is<CounterMessage>(
                           c =>
                               c.Kind == CounterKind.Average &&
                               c.PeriodKind == PeriodKind.Weekly &&
                               c.PeriodStart == startWeekDate &&
                               c.Product == InitProduct &&
                               c.Value == expectedValue)));
        }

        /// <summary>
        /// To prevent repeated counter updating, for example when app starts
        /// </summary>
        [Test]
        public void Process_void_BroadcastRecentStatictisWhenDailyCounterWasAssumed()
        {
            var passedDate = new DateTime(2015, 03, 24);
            var startWeekDate = new DateTime(2015, 03, 23);
            var expectedValue = 25;

            _mockDataStorage.Setup(m => m.UpdateCounter(It.IsAny<Counter>(), It.IsAny<int>()))
                .Returns((Counter counter, int delta) =>
                {
                    counter.Value += delta;
                    return Task.FromResult(counter);
                });

            _mockDataStorage.Setup(
                m => m.GetCounterOrDefault(InitProduct, PeriodKind.Weekly, startWeekDate, CounterKind.Average))
                .Returns(Task.FromResult(new Counter
                {
                    Kind = CounterKind.Average,
                    PeriodKind = PeriodKind.Weekly,
                    PeriodActualDate = passedDate,
                    PeriodStart = startWeekDate,
                    Product = InitProduct,
                    Value = expectedValue
                }));

            _mockDataStorage.Setup(
                m => m.GetCounterOrDefault(InitProduct, PeriodKind.Daily, passedDate, CounterKind.Total))
                .Returns(Task.FromResult(new Counter
                {
                    Kind = CounterKind.Total,
                    PeriodKind = PeriodKind.Daily,
                    PeriodStart = passedDate,
                    Product = InitProduct,
                    IsClosed = true
                }));

            _target.ProcessAsync(passedDate).Wait();

            _mockBroadcaster.Verify(
                m =>
                    m.SendCounter(
                        It.Is<CounterMessage>(
                            c =>
                                c.Kind == CounterKind.Average &&
                                c.PeriodKind == PeriodKind.Weekly &&
                                c.PeriodStart == startWeekDate &&
                                c.Product == InitProduct &&
                                c.Value == expectedValue)),
                Times.Once);
            _mockDataStorage.Verify(m => m.UpdateCounter(It.IsAny<Counter>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void Process_void_BroadcastRecentCounterWhenWeeklyCounterWasAlreadyUpdated()
        {
            var passedDate = new DateTime(2015, 03, 22);
            var startWeekDate = new DateTime(2015, 03, 16);
            var expectedValue = 25;

            _mockDataStorage.Setup(
                m => m.GetCounterOrDefault(InitProduct, PeriodKind.Weekly, startWeekDate, CounterKind.Average))
                .Returns(Task.FromResult(new Counter
                {
                    Kind = CounterKind.Average,
                    PeriodKind = PeriodKind.Weekly,
                    PeriodStart = passedDate,
                    Product = InitProduct,
                    Value = expectedValue,
                    IsClosed = true
                }));

            _target.ProcessAsync(passedDate).Wait();

            _mockBroadcaster.Verify(
                m =>
                    m.SendCounter(
                        It.Is<CounterMessage>(
                            c =>
                                c.Kind == CounterKind.Average &&
                                c.PeriodKind == PeriodKind.Weekly &&
                                c.PeriodStart == startWeekDate &&
                                c.Product == InitProduct &&
                                c.Value == expectedValue)),
                Times.Once);
            _mockDataStorage.Verify(m => m.UpdateCounter(It.IsAny<Counter>(), It.IsAny<int>()), Times.Never);
        }
    }
}
