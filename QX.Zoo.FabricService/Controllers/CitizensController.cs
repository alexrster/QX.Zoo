using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QX.Zoo.Talk.MessageBus;
using QX.Zoo.Tests.Animals;

namespace QX.Zoo.FabricService.Controllers
{
    [Route("api/citizens")]
    public class CitizensController : Controller
    {
        private readonly IAsyncBusEntity _asyncBusEntity;

        public CitizensController(IAsyncBusEntity asyncBusEntity)
        {
            _asyncBusEntity = asyncBusEntity;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Json(new
            {
                AsyncBusEntity = _asyncBusEntity.EntityId
            });
        }

        [HttpGet("{id}/move")]
        public async Task<IActionResult> MoveCitizen(long id)
        {
            await _asyncBusEntity.PublishMessageAsync(
                new MoveCitizen
                {
                    BaseVersionNumber = id,
                    NewAddress = Guid.NewGuid().ToString("N")
                });

            return Ok();
        }
    }
}
