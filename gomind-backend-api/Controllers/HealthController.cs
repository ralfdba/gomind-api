using gomind_backend_api.BL;
using gomind_backend_api.Models.Errors;
using gomind_backend_api.Models.Health;
using gomind_backend_api.Models.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using static gomind_backend_api.Models.Health.Health;

namespace gomind_backend_api.Controllers
{
    [Route("api/health")]
    [ApiController]
    [Authorize]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly BL.BL _bl;

        public HealthController(ILogger<HealthController> logger, BL.BL businessLogic)
        {
            _logger = logger;
            _bl = businessLogic;
        }

        #region Guardar Perfil Integral de Salud 
        [HttpPost("profile")]
        public async Task<ActionResult<MessageResponse>> SubmitHealthProfile([FromBody] HealthProfileRequest request)
        {
            #region Inicio Log Information
            var serializedRequest = JsonSerializer.Serialize(request);
            _logger.LogInformation("Request: {RequestJson}", serializedRequest);
            #endregion

            try
            {
                #region Validaciones iniciales

                if (!ModelState.IsValid) {
                    return BadRequest(CommonErrors.BadRequest1);
                }                
                if (request.UserId <= 0)
                {
                    return Ok(MessageResponse.Create(CommonErrors.UserIdNoValid));
                }
                #endregion

                #region BL Logic

                var response = await _bl.CreateHealthProfile(request);

                _logger.LogInformation("Response: {RequestJson}", JsonSerializer.Serialize(response));
                return Ok(response);
                #endregion

            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error");
                return StatusCode(500, MessageResponse.Create(CommonErrors.UnexpectedError(ex.Message)));
            }           
        }
        #endregion

        #region Obtener Evaluación de Salud 
        [HttpGet("evaluation")]
        public async Task<ActionResult<HealthEvaluationResponse>> GetHealthEvaluation()
        {
            #region Inicio Log Information
            //Se obtiene el UserId del token
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation("Request-User ID: {user_id}", userId);
            #endregion

            try
            {
                #region Validaciones iniciales

                if (userId <= 0 )
                {
                    return Ok(MessageResponse.Create(CommonErrors.GenericNoValid1));
                }

                #endregion

                #region BL Logic

                var response = await _bl.GetUserHealthEvaluation(userId);

                _logger.LogInformation("Response: {RequestJson}", JsonSerializer.Serialize(response));
                return Ok(response);

                #endregion

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
                return StatusCode(500, MessageResponse.Create(CommonErrors.UnexpectedError(ex.Message)));
            }
           
        }
        #endregion
    }
}
