using gomind_backend_api.Models.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
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
        [SwaggerOperation(
            Summary = "Guardar el perfil de salud del usuario en sesión",
            Description = "Permite guardar el perfil de salud del usuario en sesión.",
            Tags = new[] { "Health" }
        )]
        public async Task<ActionResult<MessageResponse>> SubmitHealthProfile([FromBody] HealthProfileRequest request)
        {
            #region Inicio Log Information
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");            
            var serializedRequest = JsonSerializer.Serialize(request);
            _logger.LogInformation("Request: {RequestJson}, UserId: {UserId}", serializedRequest, userId);
            #endregion

            try
            {
                #region Validaciones iniciales

                if (!ModelState.IsValid) {
                    return BadRequest(CommonErrors.BadRequest1);
                }                
                if (userId <= 0)
                {
                    return Ok(MessageResponse.Create(CommonErrors.UserIdNoValid));
                }
                #endregion

                #region BL Logic

                var response = await _bl.CreateHealthProfile(request, userId);

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
        [SwaggerOperation(
            Summary = "Obtener el perfil de salud del usuario en sesión",
            Description = "Permite obtener el perfil de salud del usuario en sesión.",
            Tags = new[] { "Health" }
        )]
        public async Task<ActionResult<HealthEvaluationResponse>> GetHealthEvaluation()
        {
            #region Inicio Log Information            
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
