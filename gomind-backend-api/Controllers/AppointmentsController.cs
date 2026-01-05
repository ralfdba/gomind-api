using gomind_backend_api.Models.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using System.Text.Json;
using static gomind_backend_api.Models.Appointments.Appointments;

namespace gomind_backend_api.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly ILogger<AppointmentsController> _logger;
        private readonly BL.BL _bl;

        public AppointmentsController(ILogger<AppointmentsController> logger, BL.BL businessLogic)
        {
            _logger = logger;
            _bl = businessLogic;
        }
        #region Crear appointment por user
        [HttpPost]
        [SwaggerOperation(
            Summary = "Crear cita médica al usuario",
            Description = "Permite crear una cita médica al usuario en sesión.",
            Tags = new[] { "Appointments" }
        )]
        public async Task<ActionResult<AppointmentsResponse>> CreateAppointment([FromBody] AppointmentsRequest request)
        {
            #region Inicio Log Information
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var serializedRequest = JsonSerializer.Serialize(request);
            _logger.LogInformation("Request: {RequestJson}, UserId: {UserId}", serializedRequest, userId);
            #endregion

            try
            {    
                #region Validaciones iniciales

                if (request == null)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.BadRequest1));
                }

                if (userId <= 0)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.UserIdNoValid));
                }

                if (request.ProductId <= 0)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.ProductIdNoValid));
                }

                if (request.HealthProviderId <= 0)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.ProductIdNoValid));
                }
                #endregion

                #region BL Logic

                var response = await _bl.CreateAppointmentByUser(request, userId);
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

        #region Obtener appointments por userId
        [HttpGet("users/search")]
        [SwaggerOperation(
            Summary = "Obtener citas médicas del usuario",
            Description = "Permite obtener las citas médicas del usuario en sesión.",
            Tags = new[] { "Appointments" }
        )]
        public async Task<ActionResult<AppointmentsByUser>> GetAppointmentsByUserId()
        {
            #region Inicio Log Information
            //Se obtiene el UserId del token
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation("Request-ID: {id}", userId);
            #endregion

            try
            {
                #region Validaciones iniciales

                if (userId <= 0)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.GenericNoValid1));
                }

                #endregion

                #region BL Logic

                var response = await _bl.GetAppointmentsByUser(userId);

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
