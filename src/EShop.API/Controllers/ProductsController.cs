using Microsoft.AspNetCore.Mvc;

namespace EShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Hello World");
        }
    }
}
