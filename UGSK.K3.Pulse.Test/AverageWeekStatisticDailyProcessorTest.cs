using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace UGSK.K3.Pulse.Test
{
    [TestFixture]
    public class AverageWeekStatisticDailyProcessorTest
    {
        AverageWeekStatisticDailyProcessor _target;
        private Mock<IDataStorage> _mockDataStorage;
        private Mock<IBroadcaster> _mockBroadcaster;

        [SetUp]
        public void Init()
        {
            _mockDataStorage = new Mock<IDataStorage>();

            _mockBroadcaster = new Mock<IBroadcaster>();

            _target = new AverageWeekStatisticDailyProcessor(_mockDataStorage.Object, _mockBroadcaster.Object);
        }

        [Test]
        public void Process_void_PerformedLastWeek()
        {
            var passedDate = new DateTime(2015, 03, 22);
            var startWeekDate = new DateTime(2015, 03, 16);
            var product = "product";
            int previousAverageWeekStat = 12;
            int passedDateStat = 250;

            _mockDataStorage.Setup(m => m.GetProducts())
                .Returns(Task.FromResult(new List<string> { product }.AsEnumerable()));
            _mockDataStorage.Setup(m => m.GetCounter(product, PeriodKind.Weekly, startWeekDate, CounterKind.Average))
                .Returns(Task.FromResult(new Counter
                {
                    Kind = CounterKind.Average,
                    Value = previousAverageWeekStat,
                    PeriodKind = PeriodKind.Weekly,
                    PeriodStart = startWeekDate,
                    Product = product
                }));
            _mockDataStorage.Setup(
                m => m.GetCounter(product, PeriodKind.Daily, passedDate, CounterKind.Total))
                .Returns(Task.FromResult(new Counter
                {
                    Kind = CounterKind.Total,
                    PeriodKind = PeriodKind.Daily,
                    PeriodStart = passedDate,
                    Product = product,
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

            //weekly average stat was updated and closed
            _mockDataStorage.Verify(m =>
                m.UpdateCounter(It.Is<Counter>(
                    c =>
                        c.Kind == CounterKind.Average &&
                        c.PeriodKind == PeriodKind.Weekly &&
                        c.PeriodStart.Date == startWeekDate &&
                        c.Value == expectedValue &&
                        c.IsClosed == true
                    ),
                    It.Is<int>(d => d == expectedValue - previousAverageWeekStat)), Times.Once());

            //day stat has become closed without value changes
            _mockDataStorage.Verify(m =>
                m.UpdateCounter(It.Is<Counter>(
                    c =>
                        c.Kind == CounterKind.Total &&
                        c.PeriodKind == PeriodKind.Daily &&
                        c.PeriodStart.Date == passedDate &&
                        c.IsClosed == true),
                    It.Is<int>(d => d == 0)));
        }

        [Test]
        public void Process_void_PerformedStartedWeek()
        {
            var passedDate = new DateTime(2015, 03, 23);
            var startWeekDate = new DateTime(2015, 03, 23);
            var product = "product";

            _mockDataStorage.Setup(m => m.GetProducts())
                .Returns(Task.FromResult(new List<string> { product }.AsEnumerable()));
            _mockDataStorage.Setup(
                m => m.GetCounter(product, PeriodKind.Daily, passedDate, CounterKind.Total))
                .Returns(Task.FromResult(new Counter
                {
                    Kind = CounterKind.Total,
                    PeriodKind = PeriodKind.Daily,
                    PeriodStart = passedDate,
                    Product = product,
                    Value = 25
                }));

            Counter actual = null;

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
                        c.IsClosed == false), It.Is<int>(d => d == 25)), Times.Once);

            //day stat has become closed without value changes
            _mockDataStorage.Verify(m =>
                m.UpdateCounter(It.Is<Counter>(
                    c =>
                        c.Kind == CounterKind.Total &&
                        c.PeriodKind == PeriodKind.Daily &&
                        c.PeriodStart.Date == passedDate &&
                        c.IsClosed == true),
                    It.Is<int>(d => d == 0)), Times.Once);
        }

        [Test]
        public void Process_void_BroadcastNewWeekAverageCounter()
        {
            var currentDate = new DateTime(2015, 03, 23);
            var startWeekDate = new DateTime(2015, 03, 23);
            var product = "product";

            _mockDataStorage.Setup(m => m.GetProducts())
                .Returns(Task.FromResult(new List<string> { product }.AsEnumerable()));
            _mockDataStorage.Setup(m => m.UpdateCounter(It.IsAny<Counter>(), It.IsAny<int>()))
                .Returns((Counter counter, int delta) =>
                {
                    counter.Value += delta;
                    return Task.FromResult(counter);
                });

            _target.ProcessAsync(currentDate).Wait();

            _mockBroadcaster.Verify(m => m.SendCounter(It.Is<CounterMessage>(cm => cm.PeriodKind == PeriodKind.Weekly && cm.PeriodStart.Date == startWeekDate && cm.Product == "product" && cm.Value == 0)));
        }

        [Test]
        public void Process_void_ThereIsNoSenseToBroadcastIfStatisticIsUpdated()
        {
            var passedDate = new DateTime(2015, 03, 23);
            var startWeekDate = passedDate;
            var product = "product";

            _mockDataStorage.Setup(m => m.GetProducts())
                .Returns(Task.FromResult(new List<string> { product }.AsEnumerable()));
            _mockDataStorage.Setup(m => m.UpdateCounter(It.IsAny<Counter>(), It.IsAny<int>()))
                .Returns((Counter counter, int delta) =>
                {
                    counter.Value += delta;
                    return Task.FromResult(counter);
                });
            _mockDataStorage.Setup(
                m => m.GetCounter(product, PeriodKind.Daily, passedDate, CounterKind.Total))
                .Returns(Task.FromResult(new Counter
                {
                    Kind = CounterKind.Total,
                    PeriodKind = PeriodKind.Daily,
                    PeriodStart = passedDate,
                    Product = product,
                    Value = 25,
                    IsClosed = true
                }));

            _target.ProcessAsync(passedDate).Wait();

            _mockBroadcaster.Verify(m => m.SendCounter(It.IsAny<CounterMessage>()), Times.Never());
        }

        [Test]
        public void Process_void_ThereIsNoSenseToBroadcastIfWeekISClosed()
        {
            var passedDate = new DateTime(2015, 03, 22);
            var startWeekDate = new DateTime(2015, 03, 16);
            var product = "product";

            _mockDataStorage.Setup(m => m.GetProducts())
                .Returns(Task.FromResult(new List<string> { product }.AsEnumerable()));
            _mockDataStorage.Setup(m => m.UpdateCounter(It.IsAny<Counter>(), It.IsAny<int>()))
                .Returns((Counter counter, int delta) =>
                {
                    counter.Value += delta;
                    return Task.FromResult(counter);
                });
            _mockDataStorage.Setup(
                m => m.GetCounter(product, PeriodKind.Weekly, startWeekDate, CounterKind.Average))
                .Returns(Task.FromResult(new Counter
                {
                    Kind = CounterKind.Average,
                    PeriodKind = PeriodKind.Weekly,
                    PeriodStart = passedDate,
                    Product = product,
                    Value = 25,
                    IsClosed = true
                }));

            _target.ProcessAsync(passedDate).Wait();

            _mockBroadcaster.Verify(m => m.SendCounter(It.IsAny<CounterMessage>()), Times.Never());
        }
    }
}
