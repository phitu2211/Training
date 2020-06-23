using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Training.Helper;
using Training.Models;

namespace Training.Controllers
{
    public class LogController : Controller
    {
        private readonly IElasticClient _elasticClient;

        public LogController(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }
        // GET
        public async Task<IActionResult> Index(int? pageNumber)
        {
            var log = _elasticClient.Search<Log>(s => s
                .From(0)
                .Size(1000)
                .Index("training--logging"))
                .Documents;
            
            var data = await PaginatedList<Log>.CreateAsync(log, pageNumber ?? 1, 10);

            return View(data);
        }
    }
}