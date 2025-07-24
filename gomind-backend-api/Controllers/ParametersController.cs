using Microsoft.AspNetCore.Http;
using gomind_backend_api.Models.Parameters;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;
using gomind_backend_api.Models.Errors;
using Amazon.Runtime.Internal;
using System.DirectoryServices.Protocols;
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
        public async Task<IActionResult> GetAllParameters()
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
        public async Task<IActionResult> GetParameterById(int id)
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
        public async Task <IActionResult> CreateParameter([FromBody] ParameterRequest request)
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
        public async Task<IActionResult> UpdateParameter(int id, [FromBody] ParameterRequest request)
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
        public async Task<IActionResult> DeleteParameter(int id)
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
    }
}
