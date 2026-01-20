using Amazon.Runtime.Internal;
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
        [HttpGet("")]
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

        #region Obtener detalle de appointment por id

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Obtener citas médicas por su ID",
            Description = "Permite obtener el detalle de las citas médicas por ID.",
            Tags = new[] { "Appointments" }
        )]
        public async Task<ActionResult<AppointmentDetail>> GetAppointmentsById(int id)
        {   
            try
            {
                #region Validaciones iniciales
                
                if (id <= 0)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.GenericNoValid1));
                }
                #endregion

                #region BL Logic

                var response = await _bl.GetAppointmentByIdAsync(id);

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

        #region Eliminar appointment por id

        [HttpDelete("{id}")]
        [SwaggerOperation(
           Summary = "Eliminar una cita medica",
           Description = "Permite eliminar una cita medica.",
           Tags = new[] { "Appointments" }
       )]
        public async Task<ActionResult<MessageResponse>> DeleteAppointment(int id)
        {    
            try
            {
                #region Validaciones iniciales

                if (id <= 0)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.GenericNoValid1));
                }
                #endregion

                #region BL Logic

                var response = await _bl.DeleteAppointmentAsync(id);

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

        #region Modificar estado de la cita
        [HttpPut("{id}/state")]
        [SwaggerOperation(
            Summary = "Actualiza el estado de una cita médica",
            Description = "Recibe el ID de la cita y el nuevo estado (1, 2 o 3).",
            Tags = new[] { "Appointments" }
        )]
        public async Task<ActionResult<MessageResponse>> UpdateState(int id, [FromQuery] int newState)
        {          
            try
            {
                if (id <= 0)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.GenericNoValid1));
                }                
                var response = await _bl.UpdateAppointmentStateAsync(id, newState);

                _logger.LogInformation("Response: {RequestJson}", JsonSerializer.Serialize(response));
                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
                return StatusCode(500, MessageResponse.Create(CommonErrors.UnexpectedError(ex.Message)));
            }

        }
        #endregion

        //[HttpGet("current-hour")]
        //[SwaggerOperation(Summary = "Lista citas de la hora actual", Description = "Filtra registros del minuto 00 al 59 de la hora en curso.", Tags = new[] { "Appointments" })]
        //public async Task<ActionResult<List<AppointmentsConfirmedProducer>>> GetCurrentHour()
        //{
        //    try
        //    {
        //        var results = await _bl.GetAppointmentsCurrentHourAsync();
        //        return Ok(results);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error recuperando citas de la hora actual");
        //        return StatusCode(500, MessageResponse.Create(CommonErrors.UnexpectedError(ex.Message)));
        //    }
        //}

        //[HttpGet("current-hour/details/{id}")]
        //[SwaggerOperation(Summary = "Lista citas de la hora actual", Description = "Filtra registros del minuto 00 al 59 de la hora en curso.", Tags = new[] { "Appointments" })]
        //public async Task<ActionResult<AppointmentsConfirmedNotifier>> GetAppoinmetConfirmedDetails(int id)
        //{
        //    try
        //    {
        //        var results = await _bl.GetUpcomingAppointmentByIdAsync(id);
        //        return Ok(results);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error recuperando citas de la hora actual");
        //        return StatusCode(500, MessageResponse.Create(CommonErrors.UnexpectedError(ex.Message)));
        //    }
        //}
    }
}
