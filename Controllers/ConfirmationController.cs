using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RebatesAPI.DTO;
using RebatesAPI.Utilities;

namespace RebatesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfirmationController : ControllerBase
    {
        [HttpPost]
        [Route("ConfirmRebate")]
        public async Task<IActionResult> ConfirmRebate(ConfirmationRequest request)
        {
            AzureQueueHandler azQue = new AzureQueueHandler();
            if (request.Confirmation == "Y")
            {
                var response = await azQue.writeToAzureQueue(request);
                return Ok(response);
            }
            return Ok(false);
        }
    }
}
