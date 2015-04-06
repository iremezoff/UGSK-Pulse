using System;
using System.Threading.Tasks;
using UGSK.K3.Pulse.Infrastructure;

namespace UGSK.K3.Pulse.Processors.SyncAdapters
{
    class CommonProcessorAdapter<T> : IStatisticProcessor where T : IStatisticProcessor
    {
        private readonly T _innerProcessor;

        public CommonProcessorAdapter(T innerProcessor)
        {
            _innerProcessor = innerProcessor;
        }

        public void Process(DateTime dateForHandle)
        {
            _innerProcessor.ProcessAsync(dateForHandle).Wait();
        }

        public async Task ProcessAsync(DateTime dateForHandle)
        {
            await _innerProcessor.ProcessAsync(dateForHandle);
        }
    }
}