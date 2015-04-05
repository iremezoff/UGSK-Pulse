using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using UGSK.K3.Pulse.Infrastructure;

namespace UGSK.K3.Pulse.Controllers
{
    public class IndexController : ApiController
    {
        private readonly IDataStorage _dataStorage;
        private readonly IIndexProcessor _processor;

        public IndexController(IDataStorage dataStorage, IIndexProcessor processor)
        {
            _dataStorage = dataStorage;
            _processor = processor;
        }

        public async Task<Index> Get(string product, PeriodKind periodKind = PeriodKind.Daily)
        {
            return await _dataStorage.GetIndex(product);
        }

        public async Task<IHttpActionResult> Post(Index index)
        {
            await _processor.Process(index);

            return StatusCode(HttpStatusCode.Accepted);
        }
    }
}