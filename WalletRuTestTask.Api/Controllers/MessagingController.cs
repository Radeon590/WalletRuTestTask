using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WalletRuTestTask.Api.Entities;
using WalletRuTestTask.Api.Services;
using WalletRuTestTask.Api.Services.DbService.NpSql;

namespace WalletRuTestTask.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagingController : ControllerBase
    {
        private readonly NpSqlDbService _npSqlDbService;
        private readonly ILogger _logger;
        private readonly WebSocketsHandler _webSocketsHandler;
        private const string MessagesTableName = "Messages";
        private const string MessageDateTimeColumnName = "DateTime";

        public MessagingController(NpSqlDbService npSqlDbService, ILogger<MessagingController> logger, WebSocketsHandler webSocketsHandler)
        {
            _npSqlDbService = npSqlDbService;
            _logger = logger;
            _webSocketsHandler = webSocketsHandler;
        }

        [HttpPost]
        public async Task<IResult> PostMessage(string content)
        {
            try
            {
                var newMessage = new Message(content, DateTime.Now);
                await _npSqlDbService.Add(MessagesTableName, newMessage);
                await _webSocketsHandler.BroadcastMessageAsync(newMessage);
                _logger.LogInformation("Received message {Content} in {DateTimeNow}", content, DateTime.Now);
                // не думаю, что здесь нужно делать лог, если бы это был сервис для обмена сообщениями между пользователями.
                // это не влияет на понимание состояния работы приложения, а только спамит в консоль каждый раз, когда пользователь напишет сообщение.
                // но с точки зрения идеологии блокчейн это нужно делать
                return Results.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while post message {Content} {DateTimeNow}", content, DateTime.Now);
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public List<Message> GetMessages([FromBody] DateTime borderDateTime)
        {
            try
            {
                var filters = new Dictionary<string, (string, object)>
                {
                    { MessageDateTimeColumnName, (">", borderDateTime) }
                };

                List<Message> messages = _npSqlDbService.Get<Message>(MessagesTableName, filters);

                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while get messages with borderDateTime {BorderDateTime}", borderDateTime);
                return null;
            }
        }
    }
}