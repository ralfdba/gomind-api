using gomind_backend_api.Models.Errors;
using gomind_backend_api.Models.ReferenceRange;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Reflection;
using System.Text.Json;

namespace gomind_backend_api.Controllers
{
    [Route("api/references-range")]
    [ApiController]
    [Authorize]
    public class ReferencesRangeController : ControllerBase
    {
        private readonly ILogger<ReferencesRangeController> _logger;
        private readonly BL.BL _bl;
        public ReferencesRangeController(ILogger<ReferencesRangeController> logger, BL.BL businessLogic)
        {
            _logger = logger;
            _bl = businessLogic;
        }

        #region Obtener todas las referencia de rango
        [HttpGet]
        [SwaggerOperation(
            Summary = "Obtener las referencias de rango",
            Description = "Permite obtener las referencias de rango.",
            Tags = new[] { "ReferencesRange" }
        )]
        public async Task<ActionResult<ReferenceRange>> GetAllReferenceRanges()
        {
            try
            {
                #region BL Logic

                var dataReferences = await _bl.GetReferencesRange();
                return Ok(dataReferences);

                #endregion
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
                return StatusCode(500, MessageResponse.Create(CommonErrors.UnexpectedError(ex.Message)));
            }
        }
        #endregion

        #region Obtener referencia de rango por ID
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Obtener las referencias de rango por su ID",
            Description = "Permite obtener las referencias de rango por su ID.",
            Tags = new[] { "ReferencesRange" }
        )]
        public async Task<ActionResult<ReferenceRange>> GetReferenceRangeById(int id)
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

                var response = await _bl.GetReferenceRangeById(id);

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

        #region Obtener referencia de rango por Parameter ID
        [HttpGet("parameter/{id}")]
        [SwaggerOperation(
            Summary = "Obtener las referencias de rango por el ID del parametro",
            Description = "Permite obtener las referencias de rango por el ID del parametro.",
            Tags = new[] { "ReferencesRange" }
        )]
        public async Task<ActionResult<IEnumerable<ReferenceRange>>> GetReferenceRangeByParameterId(int id)
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

                var response = await _bl.GetReferenceRangeByParameterId(id);

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

        #region Crear referencia de rango 
        [HttpPost]
        [SwaggerOperation(
            Summary = "Crear referencia de rango",
            Description = "Permite crear una referencia de rango a un parametro.",
            Tags = new[] { "ReferencesRange" }
        )]
        public async Task<ActionResult<MessageResponse>> CreateReferenceRange([FromBody] ReferenceRangeRequest request)
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
                if ((int)request.ConditionType == 1 && request.MinValue == null && request.MaxValue == null)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.ReferenceRangeNoValid1));
                }
                if ((int)request.ConditionType == 1 && request.MinValue > request.MaxValue)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.ReferenceRangeNoValid1));
                }
                if ((int)request.ConditionType > 1 && request.ConditionValue == null)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.ReferenceRangeNoValid3));
                }

                #endregion

                #region BL Logic               

                var response = await _bl.CreateReferenceRange(request);

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

        #region Modificar referencia de rango
        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Modificar referencia de rango",
            Description = "Permite modificar una referencia de rango.",
            Tags = new[] { "ReferencesRange" }
        )]
        public async Task<ActionResult<MessageResponse>> UpdateReferenceRange(int id, [FromBody] ReferenceRangeRequest request)
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

                var response = await _bl.UpdateReferenceRange(id, request);

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

        #region Eliminar referencia de rango
        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Eliminar referencia de rango",
            Description = "Permite eliminar una referencia de rango.",
            Tags = new[] { "ReferencesRange" }
        )]
        public async Task<ActionResult<MessageResponse>> DeleteReferenceRange(int id)
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

                var response = await _bl.DeleteReferenceRange(id);

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
