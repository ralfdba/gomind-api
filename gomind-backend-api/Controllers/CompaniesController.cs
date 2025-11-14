using Amazon.Runtime.Internal;
using gomind_backend_api.JWT;
using gomind_backend_api.Models.Company;
using gomind_backend_api.Models.Errors;
using gomind_backend_api.Models.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using System.Text.Json;

namespace gomind_backend_api.Controllers
{
    [Route("api/companies")]
    [ApiController]
    [Authorize]
    public class CompaniesController : ControllerBase
    {

        private readonly ILogger<CompaniesController> _logger;
        private readonly BL.BL _bl;
        public CompaniesController(ILogger<CompaniesController> logger, BL.BL businessLogic)
        {
            _logger = logger;
            _bl = businessLogic;
        }

        #region Se obtienen los productos por company id

        [HttpGet("{company_id}/products")]        
        public async Task <ActionResult<CompanyInfo>> GetProductsByCompanyId(int company_id)
        {
            #region Inicio Log Information
            _logger.LogInformation("Request-Company ID: {company_id}", company_id);
            #endregion

            try
            {
                #region Validaciones iniciales
                if (company_id <= 0)
                {
                    return Ok(MessageResponse.Create(CommonErrors.GenericNoValid1));
                }

                #endregion

                #region BL Logic
                var dataProducts = await _bl.GetProductsByCompany(company_id);
                
                var response = new CompanyInfo
                {
                    CompanyId = company_id,
                    Products = dataProducts.ToArray()
                };

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

        #region Se obtienen los health providers por company id

        [HttpGet("{company_id}/health-providers")]

        public async Task<ActionResult<CompanyHealthProviderInfo>> GetHealthProvidersByCompanyId(int company_id)
        {
            #region Inicio Log Information
            _logger.LogInformation("Request-Company ID: {company_id}", company_id);
            #endregion

            try
            {
                #region Validaciones iniciales
                if (company_id <= 0)
                {
                    return Ok(MessageResponse.Create(CommonErrors.GenericNoValid1));
                }

                #endregion

                #region BL Logic
                var data = await _bl.GetHealthProvidersByCompanyId(company_id);

                var response = new CompanyHealthProviderInfo
                {
                    CompanyId = company_id,
                    HealthProviders = data.ToArray()
                };

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
