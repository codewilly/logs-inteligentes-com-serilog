using API.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;
using System.Runtime.InteropServices;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExampleController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            Log.Information("Hello World");

            return Ok();
        }

        [HttpGet("users/{id}")]
        public IActionResult Get(int id, [FromQuery]string name = "")
        {
            Log.Information("Olá usuário {id}", id);

            return Ok();
        }

        [HttpPost]
        public IActionResult Create(ExampleViewModel viewModel)
        {
            Log.Information("Criando usuário: {name} - Documento: {document}",
                viewModel.Name, viewModel.Document);

            return StatusCode(201);
        }
    }
}