using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WalletRuTestTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagingController : ControllerBase
    {
        [HttpPost]
        public async Task PostMessage(string text)
        {
            
        }
    }
}
