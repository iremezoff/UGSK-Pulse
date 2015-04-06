using System.Collections.Generic;
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

        public async Task<IEnumerable<Index>> Get()
        {
            var result = await _dataStorage.GetIndexes();
            return result;
        }

        public async Task<Index> Get(int id)
        {
            return await _dataStorage.GetIndex(id);
        }

        public async Task<Index> Get(string product, PeriodKind periodKind = PeriodKind.Daily)
        {
            return await _dataStorage.GetIndex(product);
        }

        public async Task<IHttpActionResult> Post(Index index)
        {
            try
            {
                await _processor.Process(index);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                return Conflict();
            }

            return CreatedAtRoute("rest", new { Id = index.Id }, index);
        }

        public async Task<IHttpActionResult> Put(Index index)
        {
            await _dataStorage.UpdateIndex(index);
            return StatusCode(HttpStatusCode.OK);
        }

        public async Task<IHttpActionResult> Delete(int id)
        {

            var index = await _dataStorage.GetIndex(id);

            if (index == null)
            {
                return NotFound();
            }

            await _dataStorage.DeleteIndex(id);

            return Ok(index);
        }
    }
}