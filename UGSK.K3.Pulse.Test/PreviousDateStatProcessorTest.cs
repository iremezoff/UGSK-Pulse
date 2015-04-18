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
    class PreviousDateStatProcessorTest
    {
        private PreviousDateStatProcessor _target;
        private Mock<IDataStorage> _mockDataStorage;
        private Mock<IBroadcaster> _mockBroadcaster;
        private string _initProduct = "product";

        [SetUp]
        public void Init()
        {
            _mockDataStorage = new Mock<IDataStorage>();
            _mockDataStorage.Setup(m => m.GetProducts())
                .Returns(Task.FromResult(new List<string> { _initProduct }.AsEnumerable()));

            _mockBroadcaster = new Mock<IBroadcaster>();

            _target = new PreviousDateStatProcessor(_mockBroadcaster.Object, _mockDataStorage.Object);
        }

        [Test]
        public void Perform_void_PassedDayShouldBeClosedAndCurrentDayCounterShouldBeBroadcasted()
        {
            var passedDate = new DateTime(2015, 03, 23);
            var expectedValue = 13;

            _mockDataStorage.Setup(
                m => m.GetCounterOrDefault(_initProduct, PeriodKind.Daily, passedDate, CounterKind.Total))
                .Returns(
                    Task.FromResult(new Counter()
                    {
                        Kind = CounterKind.Total,
                        PeriodStart = passedDate,
                        PeriodActualDate = passedDate,
                        Value = expectedValue,
                        PeriodKind = PeriodKind.Daily,
                        Product = _initProduct
                    }));

            _mockDataStorage.Setup(
                m => m.GetCounterOrDefault(_initProduct, PeriodKind.Daily, DateTime.Now.Date, CounterKind.Total))
                .Returns(Task.FromResult(new Counter()
                {
                    Kind = CounterKind.Total,
                    PeriodStart = DateTime.Now.Date,
                    PeriodActualDate = DateTime.Now.Date,
                    Value = expectedValue,
                    PeriodKind = PeriodKind.Daily,
                    Product = _initProduct
                }));

            _target.ProcessAsync(passedDate).Wait();

            _mockDataStorage.Verify(
                m =>
                    m.UpdateCounter(
                        It.Is<Counter>(
                            c =>
                                c.IsClosed == true &&
                                c.Kind == CounterKind.Total &&
                                c.PeriodKind == PeriodKind.Daily &&
                                c.PeriodStart == passedDate &&
                                c.Product == _initProduct), 0),
                Times.Once);
            _mockDataStorage.Verify(m => m.UpdateCounter(It.IsAny<Counter>(), It.IsAny<int>()), Times.Once);

            _mockBroadcaster.Verify(
                m =>
                    m.SendCounter(
                        It.Is<CounterMessage>(
                            c =>
                                c.Kind == CounterKind.Total &&
                                c.PeriodKind == PeriodKind.Daily &&
                                c.PeriodStart == DateTime.Now.Date &&
                                c.Product == _initProduct &&
                                c.Value == expectedValue)), Times.Once);
        }

        [Test]
        public void Perform_void_BroadcastCurrentCounterIfCounterIsClosed()
        {
            var passedDate = new DateTime(2015, 03, 23);
            var expectedValue = 13;

            _mockDataStorage.Setup(
                m => m.GetCounterOrDefault(_initProduct, PeriodKind.Daily, passedDate, CounterKind.Total))
                .Returns(Task.FromResult(new Counter()
                    {
                        Kind = CounterKind.Total,
                        PeriodStart = passedDate,
                        PeriodActualDate = passedDate,
                        Value = expectedValue / 2,
                        PeriodKind = PeriodKind.Daily,
                        Product = _initProduct,
                        IsClosed = true
                    }));

            _target.ProcessAsync(passedDate).Wait();

            _mockDataStorage.Verify(m => m.UpdateCounter(It.IsAny<Counter>(), It.IsAny<int>()), Times.Never);

            _mockBroadcaster.Verify(
                m =>
                    m.SendCounter(
                        It.Is<CounterMessage>(
                            c =>
                                c.Kind == CounterKind.Total &&
                                c.PeriodKind == PeriodKind.Daily &&
                                c.Value == expectedValue &&
                                c.PeriodStart == DateTime.Now.Date &&
                                c.Product == _initProduct)),
                Times.Once);
        }
    }
}
