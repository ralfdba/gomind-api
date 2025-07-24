using gomind_backend_api.JWT;
using gomind_backend_api.Models.Appointments;
using gomind_backend_api.Models.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

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

        [HttpPost] 
        public async Task<IActionResult> CreateAppointment([FromBody] Appointments.AppointmentsRequest request)
        {
            #region Inicio Log Information
            var serializedRequest = JsonSerializer.Serialize(request);
            _logger.LogInformation("Request: {RequestJson}", serializedRequest);
            #endregion

            try
            {    
                #region Validaciones iniciales

                if (request == null)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.BadRequest1));
                }

                if (request.UserId <= 0)
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

                var response = await _bl.CreateAppointmentByUser(request);
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
    }
}
