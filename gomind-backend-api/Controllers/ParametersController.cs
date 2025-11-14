using Amazon.Runtime.Internal;
using gomind_backend_api.Models.Errors;
using gomind_backend_api.Models.Parameters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices.Protocols;
using System.Security.Claims;
using System.Text.Json;

namespace gomind_backend_api.Controllers
{
    [Route("api/parameters")]
    [ApiController]
    [Authorize]
    public class ParametersController : ControllerBase
    {
        private readonly ILogger<ParametersController> _logger;
        private readonly BL.BL _bl;
        public ParametersController(ILogger<ParametersController> logger, BL.BL businessLogic)
        {
            _logger = logger;
            _bl = businessLogic;
        }

        #region Obtener todos los parametros
        [HttpGet]
        public async Task<ActionResult<Parameters>> GetAllParameters()
        {  
            try
            {
                #region BL Logic

                var dataParameters = await _bl.GetParameters();
                return Ok(dataParameters);

                #endregion
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
                return StatusCode(500, MessageResponse.Create(CommonErrors.UnexpectedError(ex.Message)));
            }
        }
        #endregion

        #region Obtener parametros por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Parameters>> GetParameterById(int id)
        {
            #region Inicio Log Information
            _logger.LogInformation("Request-ID: {id}", id);
            #endregion

            try
            {
                #region Validaciones iniciales

                if (id <= 0)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.GenericNoValid1));
                }

                #endregion

                #region BL Logic

                var response = await _bl.GetParametersById(id);

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

        #region Crear un nuevo parametro
        [HttpPost]
        public async Task <ActionResult<MessageResponse>> CreateParameter([FromBody] ParameterRequest request)
        {
            #region Inicio Log Information
            var serializedRequest = JsonSerializer.Serialize(request);
            _logger.LogInformation("Request: {RequestJson}", serializedRequest);
            #endregion

            try
            {
                #region Validaciones iniciales

                if (!ModelState.IsValid)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.BadRequest1));
                }
                #endregion

                #region BL Logic

                var response = await _bl.CreateParameter(request);

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

        #region Modificar un parametro

        [HttpPut("{id}")]
        public async Task<ActionResult<MessageResponse>> UpdateParameter(int id, [FromBody] ParameterRequest request)
        {
            #region Inicio Log Information
            var serializedRequest = JsonSerializer.Serialize(request);
            _logger.LogInformation("ID = {Id}, Request = {RequestJson}", id, JsonSerializer.Serialize(request));
            #endregion

            try
            {
                #region Validaciones iniciales

                if (!ModelState.IsValid)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.BadRequest1));
                }
                if (id <= 0)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.GenericNoValid1));
                }
                #endregion

                #region BL Logic

                var response = await _bl.UpdateParameter(id, request);

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

        #region Eliminar un parametro

        [HttpDelete("{id}")]
        public async Task<ActionResult<MessageResponse>> DeleteParameter(int id)
        {

            #region Inicio Log Information
            _logger.LogInformation("Request-ID: {id}", id);
            #endregion

            try
            {
                #region Validaciones iniciales
                
                if (id <= 0)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.GenericNoValid1));
                }
                #endregion

                #region BL Logic

                var response = await _bl.DeleteParameter(id);

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

        #region Obtener Parameter Result por User ID
        [HttpGet("results-user")]
        public async Task<IActionResult> GetParametersResultByUser(int? parameter_id)
        {
            #region Inicio Log Information
            //Se obtiene el UserId del token
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation("Request-User ID: {user_id}", userId);
            #endregion

            try
            {
                #region Validaciones iniciales

                if (userId <= 0)
                {
                    return Ok(MessageResponse.Create(CommonErrors.GenericNoValid1));
                }

                #endregion

                #region BL Logic                
                var response = await _bl.GetParameterResults(userId, parameter_id);

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
