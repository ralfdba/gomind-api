using gomind_backend_api.JWT;
using gomind_backend_api.Models.Company;
using gomind_backend_api.Models.Errors;
using gomind_backend_api.Models.Login;
using gomind_backend_api.Models.User;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static gomind_backend_api.Models.Login.Login;


namespace gomind_backend_api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly BL.BL _bl;
        private readonly JwtServices _jwtService;

        public AuthController(ILogger<AuthController> logger, BL.BL businessLogic, JwtServices jwtService)
        {
            _logger = logger;
            _bl = businessLogic;
            _jwtService = jwtService;
        }

        #region Login
        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Autenticación del usuario",
            Description = "Permite autenticar al usuario ingresando su correo y password.",
            Tags = new[] { "Auth" }
        )]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {      
                #region Validaciones iniciales
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {                    
                    return Ok(MessageResponse.Create(CommonErrors.MissingCredentials));                   
                }
                #endregion

                #region BL Logic
                var dataUser = await _bl.GetUserByEmail(request.Email, request.Password);

                if (dataUser == null)
                {
                    return Ok(MessageResponse.Create(CommonErrors.InvalidCredentials));
                }

                // Obtener productos a través de la clase BL
                var dataProducts = await _bl.GetProductsByCompany(dataUser.CompanyId);

                // Generar el token JWT
                var token = _jwtService.GenerateToken(dataUser.UserId.ToString());

                var response = new LoginResponse
                {
                    Token = token,
                    User = dataUser,
                    Company = new CompanyInfo
                    {
                        CompanyId = dataUser.CompanyId,
                        Products = dataProducts.ToArray()
                    }
                };
                return Ok(response);

                #endregion
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error");
                return StatusCode(500, MessageResponse.Create(CommonErrors.UnexpectedError(ex.Message)));
            }
        }
        #endregion

        #region Login Aplicativo Whatsapp, comprobar usuario existente
        [HttpPost("login/user-exist")]
        [SwaggerOperation(
            Summary = "Comprobación del usuario en el sistema para Whatsapp.",
            Description = "Permite comprobar que el usuario exista en el sistema ingresando su correo. Posterior a esto, se envia un codigo de verificación al correo para confirmar el acceso.",
            Tags = new[] { "Auth" }
        )]
        public async Task<IActionResult> CheckUserByEmail([FromBody] AuthRequestByEmail request)
        {
            try
            {
                #region Validaciones iniciales
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return Ok(MessageResponse.Create(CommonErrors.MissingCredentials2));
                }
                #endregion

                #region BL Logic
                var userExist = await _bl.CheckUserByEmail(request.Email);   
                return Ok(userExist);

                #endregion
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
                return StatusCode(500, MessageResponse.Create(CommonErrors.UnexpectedError(ex.Message)));
            }
        }
        #endregion

        #region Login Aplicativo Whatsapp, Login con codigo de verificación
        [HttpPost("login/wsp")]
        [SwaggerOperation(
            Summary = "Autenticación del usuario con codigo de verificación",
            Description = "Permite autenticar al usuario ingresando su correo y código de verificacion enviada al correo.",
            Tags = new[] { "Auth" }
        )]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequestAuthCode request)
        {
            try
            {
                #region Validaciones iniciales
                if (string.IsNullOrWhiteSpace(request.Email) || request.AuthCode <= 0)
                {
                    return Ok(MessageResponse.Create(CommonErrors.MissingCredentials));
                }
                #endregion

                #region BL Logic
                var dataUser = await _bl.GetUserByEmailAuthCode(request.Email, request.AuthCode);

                if (dataUser == null)
                {
                    return Ok(MessageResponse.Create(CommonErrors.InvalidCredentials));
                }

                // Obtener productos a través de la clase BL
                var dataProducts = await _bl.GetProductsByCompany(dataUser.CompanyId);

                // Generar el token JWT
                var token = _jwtService.GenerateToken(dataUser.UserId.ToString());

                var response = new LoginResponse
                {
                    Token = token,
                    User = dataUser,
                    Company = new CompanyInfo
                    {
                        CompanyId = dataUser.CompanyId,
                        Products = dataProducts.ToArray()
                    }
                };
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
