using gomind_backend_api.AWS;
using gomind_backend_api.DB;
using gomind_backend_api.Models.Company;
using gomind_backend_api.Models.Dynamo;
using gomind_backend_api.Models.Errors;
using gomind_backend_api.Models.Examination;
using gomind_backend_api.Models.Login;
using gomind_backend_api.Models.Products;
using gomind_backend_api.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
        public async Task<IActionResult> UploadExamination([FromForm] Examination.ExaminationRequest request)
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

                var response = new Examination.ExaminationResponse
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
                    user_id = request.UserId,                     
                    created_at = DateTime.Now,                     
                    updated_at = DateTime.Now,                       
                    status = JobStatus.Pending,
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

                if (job.status == JobStatus.Completed) {

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
                    Status = job.status,
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
        public async Task<IActionResult> GetAnalysisIa(string job_id)
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
                #endregion

                #region Response
                var stream = await _s3Service.GetFileAsync(bucketName, "results-ok", job_id);
                var response = await _bl.ProcesarArchivoJsonAsync(stream);
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
        public IActionResult SaveAnalysis([FromBody] Examination.AnalysisRequest request)
        {
            #region Inicio Log Information
            var serializedRequest = JsonSerializer.Serialize(request);
            _logger.LogInformation("Request: {RequestJson}", serializedRequest);
            #endregion

            try
            {
                if (!ModelState.IsValid) {
                    return BadRequest(ModelState);
                }

                //_logger.LogInformation("Response: {RequestJson}", JsonSerializer.Serialize(response));
                return Ok();
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
