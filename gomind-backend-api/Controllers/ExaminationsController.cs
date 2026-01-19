using gomind_backend_api.AWS;
using gomind_backend_api.DB;
using gomind_backend_api.Models.Company;
using gomind_backend_api.Models.Dynamo;
using gomind_backend_api.Models.Errors;
using gomind_backend_api.Models.Examination;
using gomind_backend_api.Models.Login;
using gomind_backend_api.Models.Parameters;
using gomind_backend_api.Models.Products;
using gomind_backend_api.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Data;
using System.Security.Claims;
using System.Text.Json;
using static gomind_backend_api.Models.Examination.Examination;

namespace gomind_backend_api.Controllers
{
    [Route("api/examinations")]
    [ApiController]
    [Authorize]
    public class ExaminationsController : ControllerBase
    {

        private readonly ILogger<ExaminationsController> _logger;
        private readonly BL.BL _bl;
        private readonly IS3Service _s3Service;
        private readonly IDynamoDbService _dynamoDbService;

        private static readonly string baseUrlName = "https://s3.amazonaws.com/";
        private static readonly string bucketName = "gomind-desa-examinations";
        

        public ExaminationsController(ILogger<ExaminationsController> logger, BL.BL businessLogic, IS3Service s3Service, IDynamoDbService dynamoDbService)
        {
            _logger = logger;
            _bl = businessLogic;
            _s3Service = s3Service;
            _dynamoDbService = dynamoDbService;
        }
        #region Subir Examen Médico 
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Subir examen médico en formato PDF",
            Description = "Permite subir el examen médico en formato PDF. Esto creará un Job, el cual proporcionará el status de la extracción de los parametros del examen.",
            Tags = new[] { "Examinations" }
        )]
        public async Task<ActionResult<ExaminationResponse>> UploadExamination([FromForm] ExaminationRequest request)
        {
            #region Inicio Log Information
            //Se obtiene el UserId del token
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

                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.FileNoUploaded));
                }

                if (request.File.ContentType != "application/pdf")
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.FileTypeNoValidPDF));
                }
                #endregion

                #region Upload file S3

                string jobId = Guid.NewGuid().ToString();                
                var key = await _s3Service.UploadFileAsync(request.File, bucketName, $"raw/{request.FileType}", jobId);
                string fileUrl = $"{baseUrlName}{bucketName}/{key}";

                var response = new ExaminationResponse
                {
                    JobId = jobId,
                    FileUrl = fileUrl
                };
                #endregion

                #region Guardar en DynamoDB

                var jobRecord = new Job
                {
                    id = jobId,
                    file_type = request.FileType,
                    user_id = userId,                     
                    created_at = DateTime.Now,                     
                    updated_at = DateTime.Now,                       
                    job_status = JobStatus.Pending,
                    bucket_name = bucketName,
                    key_upload = key                  

                };
                await _dynamoDbService.SaveAsync(jobRecord);

                #endregion

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

        #region Consultar Status Job 
        [HttpGet("job/{job_id}")]
        [SwaggerOperation(
            Summary = "Consultar status del Job",
            Description = "Permite consultar el Status del Job, este indicará el estado de la extracción de los parametros del examen. Los Status son Pendiente y Completado.",
            Tags = new[] { "Examinations" }
        )]
        public async Task<ActionResult<JobResponse>> GetJobStatus(string job_id)
        {
            #region Inicio Log Information
            _logger.LogInformation("Request-Job ID: {job_id}", job_id);
            #endregion

            try
            {
                #region Validaciones Iniciales
                if (job_id == null)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.BadRequest1));
                }

                var job = await _dynamoDbService.GetAsync<Job>(job_id);

                if (job == null)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.JobNotFound));
                }
                #endregion

                #region Response
                JobDetailResponse? jobDetailResponse = null;

                if (job.job_status == JobStatus.Completed) {

                    jobDetailResponse = new JobDetailResponse
                    {
                        FileUrl = job.key_result,
                        Success = job.success,
                        FileType = job.file_type
                    };
                    if (!job.success)
                    {
                        jobDetailResponse.ErrorMessage = job.error_message;
                    }                                   
                }   
                var response = new JobResponse
                {
                    JobId = job.id,
                    Status = job.job_status,
                    Response = jobDetailResponse
                };
                #endregion

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
        
        #region Consultar parametros obtenidos de la IA       

        [HttpGet("analysis-job/{job_id}")]
        [SwaggerOperation(
            Summary = "Obtiene los parametros cuando el Job esta en Status Completado",
            Description = "Permite obtener los parametros extraidos del examen si el Status del Job es Completado. Tras obtener los parametros, compara las referencias de rangos parametrizadas a estos parametros y si estan fuera de rango, se devolveran en el response de este servicio.",
            Tags = new[] { "Examinations" }
        )]
        public async Task<ActionResult<ExaminationAnalysis>> GetAnalysisIa2(string job_id)
        {
            #region Inicio Log Information
            //Se obtiene el UserId del token
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation("Request-Job ID: {job_id}", job_id);
            #endregion

            try
            {
                #region Validaciones Iniciales
                if (job_id == null)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.BadRequest1));
                }

                var job = await _dynamoDbService.GetAsync<Job>(job_id);

                if (job == null)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.JobNotFound));
                }

                if (job.job_status != JobStatus.Completed || !job.success)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.JobNotValid));
                }

                if (job.key_result == null)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.JobKeyResultNull));
                }

                if (userId <= 0)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.UserIdNoValid));
                }
                #endregion

                #region Response                
                var response = new ExaminationAnalysis();
               
                var getDataProcessed = await _bl.GetProcessedAnalysisResultsAsync(job.key_result, userId);

                if (getDataProcessed == null)
                {
                    var stream = await _s3Service.GetFileAsync(bucketName, "results-ok", job_id);
                    response = await _bl.ProcesarArchivoJsonAsync(stream, job.key_result, job.user_id, job.file_type, job.id);                    
                }
                else
                {
                    response = getDataProcessed;
                }
                #endregion

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

        #region Consultar los examenes del usuario en session
        [HttpGet("analysis-results")]
        [SwaggerOperation(
            Summary = "Lista los exámenes ya analizados del usuario en sesión",
            Description = "Permite listar todos los exámenes que el usuario ha subido para anánlisis de parametros.",
            Tags = new[] { "Examinations" }
        )]
        public async Task<ActionResult<List<UserExaminationList>>> GetHistoryExaminationUser([FromQuery] int fileType = 0)
        {
            try
            {
                #region Inicio Log Information
                //Se obtiene el UserId del token
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("User: {userId}", userId);
                #endregion

                if (userId <= 0)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.UserIdNoValid));
                }
                
                var results = await _bl.GetUserExaminationsAsync(userId, fileType);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
                return StatusCode(500, MessageResponse.Create(CommonErrors.UnexpectedError(ex.Message)));
            }
        }
        #endregion

        #region Consultar los examenes por su id
        [HttpGet("analysis-results/result-id/{uid}")]
        [SwaggerOperation(
            Summary = "Muestra los resultados del exámen ya analizados del usuario en sesión",
            Description = "Permite obtener los resultados del exámen del usuario en sesión segun su uid.",
            Tags = new[] { "Examinations" }
        )]
        public async Task<ActionResult<UserExaminationDetail>> GetExaminationDetail(Guid uid)
        {
            try
            {
                #region Inicio Log Information
                //Se obtiene el UserId del token
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("User: {userId}", userId);
                #endregion
                if (userId <= 0)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.UserIdNoValid));
                }

                var result = await _bl.GetExaminationDetailAsync(uid, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
                return StatusCode(500, MessageResponse.Create(CommonErrors.UnexpectedError(ex.Message)));
            }
        }
        #endregion

        #region Consultar los examenes por su File Key
        [HttpGet("analysis-results/file-key/{file_key}")]
        [SwaggerOperation(
            Summary = "Muestra los resultados del exámen ya analizados del usuario en sesión.",
            Description = "Permite obtener los resultados del exámen del usuario en sesión segun su file key.",
            Tags = new[] { "Examinations" }
        )]
        public async Task<ActionResult<UserExaminationDetail>> GetByFileKey(string file_key)
        {
            try
            {
                #region Inicio Log Information
                //Se obtiene el UserId del token
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("User: {userId}", userId);
                #endregion
                if (userId <= 0)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.UserIdNoValid));
                }
                var result = await _bl.GetExaminationByFileKeyAsync(file_key, userId);
                return Ok(result);
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
