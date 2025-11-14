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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        
        #region Consultar Análisis IA 
        [HttpGet("analysis-job/{job_id}")]
        public async Task<ActionResult<List<AnalysisResult>>> GetAnalysisIa(string job_id)
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
                
                if (job.job_status != JobStatus.Completed || !job.success)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.JobNotValid));
                }

                if (job.key_result == null)
                {
                    return BadRequest(MessageResponse.Create(CommonErrors.JobKeyResultNull));
                }
                #endregion
               
                #region Response                
                var response = new List<AnalysisResult>();

                var getDataProcessed = await _bl.GetProcessedAnalysisResultsAsync(job.key_result);
                
                if (getDataProcessed == null || getDataProcessed.Count <= 0) 
                {
                    var stream = await _s3Service.GetFileAsync(bucketName, "results-ok", job_id);
                    response = await _bl.ProcesarArchivoJsonAsync(stream, job.key_result, job.user_id);
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

        #region Guardar Análisis IA 
        [HttpPost("analysis")]
        public async Task<ActionResult<MessageResponse>> SaveAnalysis([FromBody] AnalysisRequest request)
        {
            #region Inicio Log Information
            var serializedRequest = JsonSerializer.Serialize(request);
            _logger.LogInformation("Request: {RequestJson}", serializedRequest);
            #endregion

            try
            {
                #region Valdaciones Iniciales

                if (!ModelState.IsValid) {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Ok(MessageResponse.Create(CommonErrors.UserTokenNoValid1));
                }
                #endregion

                #region Response

                var response = await _bl.CreateUserRecommendationAsync(request, int.Parse(userIdClaim.Value));

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
    }
}
