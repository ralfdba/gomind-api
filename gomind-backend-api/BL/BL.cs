using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Transform;
using BCrypt.Net;
using gomind_backend_api.Bcrypt;
using gomind_backend_api.Controllers;
using gomind_backend_api.DB;
using gomind_backend_api.Models.Appointments;
using gomind_backend_api.Models.Errors;
using gomind_backend_api.Models.Health;
using gomind_backend_api.Models.HealthProvider;
using gomind_backend_api.Models.Parameters;
using gomind_backend_api.Models.Products;
using gomind_backend_api.Models.ReferenceRange;
using gomind_backend_api.Models.User;
using gomind_backend_api.Models.Utils;
using gomind_backend_api.Resources;
using MySqlConnector;
using Services;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Transactions;
using System.Xml.Linq;
using static gomind_backend_api.Models.Appointments.Appointments;
using static gomind_backend_api.Models.Examination.Examination;
using static gomind_backend_api.Models.Health.Health;

namespace gomind_backend_api.BL
{
    #region Enum para tipos de Health Evaluation
    public enum HealthEvaluationType
    {
        Physical,
        Emotional,
        Finance,
        Goldberg
    }
    #endregion
    public class BL
    {
        private readonly ILogger<BL> _logger;
        private readonly IMariaDbConnection _dbConnection;
        private readonly IHealthResourcesService _healthResourcesService;
        private readonly INotificacion _notificacion;
        private readonly BcryptServices _bcrypt;
       
        public BL(ILogger<BL> logger, IMariaDbConnection dbConnection, IHealthResourcesService healthResourcesService, INotificacion notificacion)
        {
            _logger = logger;
            _dbConnection = dbConnection;
            _healthResourcesService = healthResourcesService;
            _notificacion = notificacion;
            _bcrypt = new BcryptServices();
        }
        
        #region Obtener Usuario por Email y Password
        public async Task<UserInfo?> GetUserByEmail(string email, string pass)
        {               
            var userExist = await _dbConnection.ExecuteQueryAsync("CALL api_get_user_by_email(@p_email);",
                reader => new
                {
                    UserId = reader.GetInt32("user_id"),
                    PasswordHash = reader.GetString("secret2")
                },
                new Dictionary<string, object>
                {               
                    { "p_email", email }
                });

            if (userExist.Count == 0) {
                return null;
            }
            var userAuth = userExist[0];
            
            if (!_bcrypt.VerifyPassword(pass, userAuth.PasswordHash)) {
                return null;
            } 
            
            var dataUser = await _dbConnection.ExecuteQueryAsync<UserInfo>(              
                "CALL api_get_user_demographic_by_id(@p_user_id);",
                (reader) => new UserInfo
                {
                    UserId = reader.GetInt32("user_id"),
                    Name = reader.GetString("name"),
                    CompanyId = reader.GetInt32("company_id") 
                },
                new Dictionary<string, object>
                {
                    { "p_user_id", userAuth.UserId }
                }
            );

            return dataUser.FirstOrDefault();
        }
        #endregion

        #region Comprobar si existe Usuario por Email y envio de codigo de verificacion
        public async Task<UserExist> CheckUserByEmail(string email)
        {
            UserExist userExist = new UserExist();
            Random random = new Random();
            try             
            {
                #region Obtener usuario por email
                var user = await _dbConnection.ExecuteQueryAsync("CALL api_get_user_by_email(@p_email);",
                   reader => new
                   {
                       UserId = reader.GetInt32("user_id"),
                       PasswordHash = reader.GetString("secret2")
                   },
                   new Dictionary<string, object>
                   {                    
                       { "p_email", email }
                   });

                if (user.Count == 0)
                {
                    userExist.Message = CommonErrors.UserNotFound;
                    return userExist;
                }
                _logger.LogInformation("USUARIO POR EMAIL EXITOSO");
                #endregion

                #region Se genera codigo de verificacion aleatorio y se guarda en BBDD

                int codigoVerificacion = random.Next(1000, 10000);

                await _dbConnection.ExecuteNonQueryAsync(                   
                    "CALL api_upsert_auth_user_code(@p_user_id, @p_code)",
                   
                    new Dictionary<string, object>
                    {
                        { "p_user_id", user[0].UserId },
                        { "p_code", codigoVerificacion.ToString() }                    
                    }
                );
                _logger.LogInformation("ENVIO CODIGO VERIFICACION EXITOSO");
                #endregion

                #region Se envia el codigo de verificacion al correo del usuario

                Destinatario destinatario = new Destinatario();
                destinatario.Correo = email;
                var statusEnvioCorreo = _notificacion.EnvioCodigoVerificacion(codigoVerificacion.ToString(), destinatario);
                _logger.LogInformation("STATUS ENVIO CORREO - {status}",JsonSerializer.Serialize(statusEnvioCorreo));

                if (!statusEnvioCorreo.IsOK)
                {                   
                    userExist.Message = statusEnvioCorreo.Mensaje ?? "Error al enviar el correo.";  
                    return userExist;
                }

                userExist.Exist = true;
                userExist.Message = "";
                #endregion

                return userExist;

            }
            catch (Exception ex)
            {
                userExist.Exist = false;
                userExist.Message = ex.Message;
                _logger.LogError("ERROR: {error}", ex.Message);
                return userExist;
            }           
        }
        #endregion

        #region Obtener Usuario por Email y codigo de verificacion
        public async Task<UserInfo?> GetUserByEmailAuthCode(string email, int authCode)
        {
            try
            {
                #region Obtener usuario por email
                var user = await _dbConnection.ExecuteQueryAsync("CALL api_get_user_by_email(@p_email);",
                   reader => new
                   {
                       UserId = reader.GetInt32("user_id"),
                       PasswordHash = reader.GetString("secret2")
                   },
                   new Dictionary<string, object>
                   {                  
                       { "p_email", email }
                   });

                if (user.Count == 0)
                {
                    return null;
                }
                #endregion

                #region Validar codigo de verificacion
                var userExist = await _dbConnection.ExecuteQueryAsync("CALL api_validate_user_code(@p_user_id, @p_code);",
                    reader => new
                    {
                        Uuid = reader.GetGuid("id"),
                        UserId = reader.GetInt32("user_id"),
                        Code = reader.GetString("code"),
                        CreatedDate = reader.GetDateTime("created_date")
                    },
                    new Dictionary<string, object>
                    {                   
                        { "p_user_id", user[0].UserId },
                        { "p_code", authCode.ToString() }
                    });

                if (userExist.Count == 0)
                {
                    return null;
                }  
                #endregion

                #region Se obtiene la data del usuario

                var dataUser = await _dbConnection.ExecuteQueryAsync<UserInfo>(
                    "CALL api_get_user_demographic_by_id(@p_user_id);",
                    (reader) => new UserInfo
                    {
                        UserId = reader.GetInt32("user_id"),
                        Name = reader.GetString("name"),
                        CompanyId = reader.GetInt32("company_id")
                    },
                    new Dictionary<string, object>
                    {                   
                        { "p_user_id", userExist[0].UserId }
                    }
                );
                #endregion

                return dataUser.FirstOrDefault();

            }
            catch (Exception ex)
            {
                return null;
            }    
        }
        #endregion

        #region Obtener Productos por Empresa
        public async Task<IEnumerable<ProductInfo>> GetProductsByCompany(int companyId)
        {
            return await _dbConnection.ExecuteQueryAsync<ProductInfo>(
                "CALL api_get_products_by_company(@p_company_id)",
                (reader) => new ProductInfo 
                {
                    ProductId = reader.GetInt32("id"),
                    Name = reader.GetString("name")
                },
                new Dictionary<string, object> 
                {
                    { "p_company_id", companyId }
                }
            );
        }
        #endregion

        #region Obtener HealthProvidersByCompanyId
        public async Task<IEnumerable<HealthProviderInfo>> GetHealthProvidersByCompanyId(int companyId)
        {
            return await _dbConnection.ExecuteQueryAsync<HealthProviderInfo>(
                @"select
                hp.id,
                hp.name as health_provider_name
                from
                health_provider as hp 
                left join health_provider_by_companies as hpc on hpc.health_provider_id = hp.id
                where hpc.company_id = @p_company_id",
                (reader) => new HealthProviderInfo
                {
                    HealthProviderId = reader.GetInt32("id"),
                    Name = reader.GetString("health_provider_name")
                },
                new Dictionary<string, object>
                {
                    { "p_company_id", companyId }
                }
            );
        }
        #endregion

        #region Agendar cita a usuario
        public async Task<AppointmentsResponse> CreateAppointmentByUser(AppointmentsRequest request, int userId)
        {
           
            var appointmentData = await _dbConnection.ExecuteQueryAsync<AppointmentsResponse>(               
                "CALL api_insert_users_appointment(@p_users_id, @p_schedule_day, @p_health_provider_id, @p_product_id)",
                reader => new AppointmentsResponse
                {                    
                    AppointmentId = reader.GetInt32("appointment_id")
                },
                new Dictionary<string, object>              
                {
                    { "p_users_id", userId },
                    { "p_schedule_day", request.DateTime },
                    { "p_health_provider_id", request.HealthProviderId },
                    { "p_product_id", request.ProductId }
                }
            );
            return appointmentData.FirstOrDefault();
        }
        #endregion

        #region Obtener citas del usuario
        public async Task<IEnumerable<AppointmentsByUser>> GetAppointmentsByUser(int userId)
        {
            return await _dbConnection.ExecuteQueryAsync<AppointmentsByUser>(
                "CALL api_get_appointments_by_user(@p_user_id)",
                (reader) => new AppointmentsByUser
                {
                     AppointmentId = reader.GetInt32("id"),
                     ScheduleDay = reader.GetDateTime("schedule_day"),
                     HealthProvider = reader.GetString("health_provider"),
                     Product = reader.GetString("product")
                },
                new Dictionary<string, object>
                {
                    { "p_user_id", userId }
                }
            );
        }
        #endregion

        #region Guardar Perfil Integral de Salud
        public async Task<MessageResponse> CreateHealthProfile(HealthProfileRequest request, int userId)
        {         
            try
            {
                // Inicia la transacción
                using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    if (request.Physical != null) {
                        // SP1: Guardar datos físicos
                        await _dbConnection.ExecuteNonQueryAsync(
                            "CALL api_insert_users_health_phisically(@p_user_id, @q1, @q2, @q3, @q4, @q5, @q6, @q7, @q8, @q9, @q10)",
                            new Dictionary<string, object>
                            {
                            { "p_user_id", userId },
                            { "q1", request.Physical.q1 },
                            { "q2", request.Physical.q2 },
                            { "q3", request.Physical.q3 },
                            { "q4", request.Physical.q4 },
                            { "q5", request.Physical.q5 },
                            { "q6", request.Physical.q6 },
                            { "q7", request.Physical.q7 },
                            { "q8", request.Physical.q8 },
                            { "q9", request.Physical.q9 },
                            { "q10", request.Physical.q10 }
                            }
                        );                   
                    }

                    if (request.Emotional != null) {

                        // SP2: Guardar datos emocionales
                        await _dbConnection.ExecuteNonQueryAsync(
                        "CALL api_insert_users_health_emotional(@p_user_id, @q1, @q2, @q3, @q4, @q5, @q6, @q7, @q8, @q9, @q10)",
                        new Dictionary<string, object>
                        {
                            { "p_user_id", userId },
                            { "q1", request.Emotional.q1 },
                            { "q2", request.Emotional.q2 },
                            { "q3", request.Emotional.q3 },
                            { "q4", request.Emotional.q4 },
                            { "q5", request.Emotional.q5 },
                            { "q6", request.Emotional.q6 },
                            { "q7", request.Emotional.q7 },
                            { "q8", request.Emotional.q8 },
                            { "q9", request.Emotional.q9 },
                            { "q10", request.Emotional.q10 }
                        }                   
                        );
                    }
                        
                    if (request.Finance != null) {
                        // SP3: Guardar datos financieros
                        await _dbConnection.ExecuteNonQueryAsync(
                        "CALL api_insert_users_health_finance(@p_user_id, @q1, @q2, @q3, @q4, @q5, @q6, @q7, @q8, @q9, @q10)",
                        new Dictionary<string, object>
                        {
                            { "p_user_id", userId },
                            { "q1", request.Finance.q1 },
                            { "q2", request.Finance.q2 },
                            { "q3", request.Finance.q3 },
                            { "q4", request.Finance.q4 },
                            { "q5", request.Finance.q5 },
                            { "q6", request.Finance.q6 },
                            { "q7", request.Finance.q7 },
                            { "q8", request.Finance.q8 },
                            { "q9", request.Finance.q9 },
                            { "q10", request.Finance.q10 }
                        }                    
                        );
                    }
                        

                    if (request.Goldberg != null) {
                        // SP4: Guardar datos de Goldberg
                        await _dbConnection.ExecuteNonQueryAsync(
                        "CALL api_insert_users_health_goldberg(@p_user_id, @q1, @q2, @q3, @q4, @q5, @q6, @q7, @q8, @q9, @q10, @q11, @q12, @q13)",
                        new Dictionary<string, object>
                        {
                            { "p_user_id", userId },
                            { "q1", request.Goldberg.q1 },
                            { "q2", request.Goldberg.q2 },
                            { "q3", request.Goldberg.q3 },
                            { "q4", request.Goldberg.q4 },
                            { "q5", request.Goldberg.q5 },
                            { "q6", request.Goldberg.q6 },
                            { "q7", request.Goldberg.q7 },
                            { "q8", request.Goldberg.q8 },
                            { "q9", request.Goldberg.q9 },
                            { "q10", request.Goldberg.q10 },
                            { "q11", request.Goldberg.q11 },
                            { "q12", request.Goldberg.q12 },
                            { "q13", request.Goldberg.q13 }
                        }                   
                        );
                    }         
                    scope.Complete();
                    return MessageResponse.Create(CommonSuccess.HealthProfileSuccess1, true);                    
                }
            }
            catch (Exception ex)
            {
                return MessageResponse.Create(CommonErrors.UnexpectedError(ex.Message));                
            }
        }

        #endregion

        #region Obtener Perfil Integral de Salud
        public async Task<HealthEvaluationResponse> GetUserHealthEvaluation(int userId)        
        {
            #region Se inicializan las clases, se carga el Json con los recursos health evaluation
            var response = new HealthEvaluationResponse
            {
                UserId = userId
            };

            AllHealthResources allHealthResources = await _healthResourcesService.GetAllResourcesAsync();
            #endregion

            #region Physical Evaluation
            var physicalData = (await _dbConnection.ExecuteQueryAsync<HealthPhysicalData>(

                "CALL api_get_user_physical_health(@p_user_id)",
                reader => new HealthPhysicalData
                {
                    InitialData = new PhysicalGenericData
                    {
                        Weight = reader.GetInt32("initial_weight"),
                        Height = reader.GetInt32("initial_height"),
                        Result = reader.GetInt32("initial_result")
                    },
                    CurrentData = new PhysicalGenericData
                    {
                        Weight = reader.GetInt32("current_weight"),
                        Height = reader.GetInt32("current_height"),
                        Result = reader.GetInt32("current_result")
                    }
                },
                new Dictionary<string, object>
                {
                        { "p_user_id", userId }
                }
                )).FirstOrDefault();

            #region Se calcula IMC y Detalle a data Inicial y Actual

            if (physicalData != null)
            {
                physicalData.InitialData.ItemDetails = GetItemDetailsByTypeAndValue(HealthEvaluationType.Physical, physicalData.InitialData.Result, allHealthResources);
                physicalData.InitialData.Imc = Utils.CalculateImc(physicalData.InitialData.Weight, physicalData.InitialData.Height);
               
                physicalData.CurrentData.ItemDetails = GetItemDetailsByTypeAndValue(HealthEvaluationType.Physical, physicalData.CurrentData.Result, allHealthResources);
                physicalData.CurrentData.Imc = Utils.CalculateImc(physicalData.CurrentData.Weight, physicalData.CurrentData.Height);
            }     

            response.Physical = physicalData;
            #endregion

            #endregion

            #region Emotional Evaluation
            var emotionalData = (await _dbConnection.ExecuteQueryAsync<HealthEmotionalData>(

                "CALL api_get_user_emotional_health(@p_user_id)",
                reader => new HealthEmotionalData
                {
                    InitialData = new EmotionalGenericData
                    {
                        Result = reader.GetInt32("initial_result")
                    },
                    CurrentData = new EmotionalGenericData
                    {
                        Result = reader.GetInt32("current_result")
                    }
                },
                new Dictionary<string, object>
                {
                        { "p_user_id", userId }
                }
                )).FirstOrDefault();

            #region Detalle data Inicial y Actual

            if (emotionalData != null) 
            {
                emotionalData.InitialData.ItemDetails = GetItemDetailsByTypeAndValue(HealthEvaluationType.Emotional, emotionalData.InitialData.Result, allHealthResources);
                emotionalData.CurrentData.ItemDetails = GetItemDetailsByTypeAndValue(HealthEvaluationType.Emotional, emotionalData.CurrentData.Result, allHealthResources);
            }

            response.Emotional = emotionalData;
            #endregion

            #endregion

            #region Finance Evaluation
            var financeData = (await _dbConnection.ExecuteQueryAsync<HealthFinanceData>(

                "CALL api_get_user_finance_health(@p_user_id)",
                reader => new HealthFinanceData
                {
                    InitialData = new FinanceGenericData
                    {
                        Result = reader.GetInt32("initial_result")
                    },
                    CurrentData = new FinanceGenericData
                    {
                        Result = reader.GetInt32("current_result")
                    }
                },
                new Dictionary<string, object>
                {
                        { "p_user_id", userId }
                }
                )).FirstOrDefault();

            #region Detalle data Inicial y Actual
            if (financeData != null) 
            {
                financeData.InitialData.ItemDetails = GetItemDetailsByTypeAndValue(HealthEvaluationType.Finance, financeData.InitialData.Result, allHealthResources);
                financeData.CurrentData.ItemDetails = GetItemDetailsByTypeAndValue(HealthEvaluationType.Finance, financeData.CurrentData.Result, allHealthResources);
            }

            response.Finance = financeData;
            #endregion

            #endregion

            #region Goldberg Evaluation
            var goldbergData = (await _dbConnection.ExecuteQueryAsync<HealthGoldbergData>(

                "CALL api_get_user_goldberg_health(@p_user_id)",
                reader => new HealthGoldbergData
                {
                    InitialData = new GoldbergGenericData
                    {
                        Result = reader.GetInt32("initial_result")
                    },
                    CurrentData = new GoldbergGenericData
                    {
                        Result = reader.GetInt32("current_result")
                    }
                },
                new Dictionary<string, object>
                {
                        { "p_user_id", userId }
                }
                )).FirstOrDefault();

            #region Detalle data Inicial y Actual
            if (goldbergData != null)
            {
                goldbergData.InitialData.ItemDetails = GetItemDetailsByTypeAndValue(HealthEvaluationType.Goldberg, goldbergData.InitialData.Result, allHealthResources);
                goldbergData.CurrentData.ItemDetails = GetItemDetailsByTypeAndValue(HealthEvaluationType.Goldberg, goldbergData.CurrentData.Result, allHealthResources);
            }

            response.Goldberg = goldbergData;
            #endregion

            #endregion

            return response;
        }
        #endregion

        #region Obtener todos los parametros
        public async Task<IEnumerable<Parameters>> GetParameters()
        {
            return await _dbConnection.ExecuteQueryAsync<Parameters>(
            "CALL api_get_parameters()",
            (reader) =>
            {
                List<KeyResultDetail> keysResultsList;
                try
                {
                    var jsonString = reader.IsDBNull(reader.GetOrdinal("keys_results"))
                                ? "[]"
                                : reader.GetString("keys_results");

                    keysResultsList = JsonSerializer.Deserialize<List<KeyResultDetail>>(jsonString)
                                      ?? new List<KeyResultDetail>();
                }
                catch 
                {
                    keysResultsList = new List<KeyResultDetail>();
                }
                return new Parameters
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Description = reader.GetString("description"),
                    UnitOfMeasure = reader.IsDBNull("unit_of_measure") ? null : reader.GetString("unit_of_measure").ToString(),
                    KeysResults = keysResultsList                
                };           
            }       
            );        
        }
        #endregion

        #region Obtener parametro por su id
        public async Task<Parameters?> GetParametersById(int parameterId)
        {
            var dataParameter = await _dbConnection.ExecuteQueryAsync<Parameters>(
                "CALL api_get_parameter_by_id(@p_id)",
                (reader) =>
                {
                    List<KeyResultDetail> keysResultsList;
                    try
                    {
                        var jsonString = reader.IsDBNull(reader.GetOrdinal("keys_results"))
                                 ? "[]"
                                 : reader.GetString("keys_results");
                        
                        keysResultsList = JsonSerializer.Deserialize<List<KeyResultDetail>>(jsonString)
                                          ?? new List<KeyResultDetail>();
                    }
                    catch
                    {
                        keysResultsList = new List<KeyResultDetail>();
                    }
                    return new Parameters
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("name"),
                        Description = reader.GetString("description"),
                        UnitOfMeasure = reader.IsDBNull("unit_of_measure") ? null : reader.GetString("unit_of_measure").ToString(),
                        KeysResults = keysResultsList
                    };
                },
                new Dictionary<string, object>               
                {
                    { "p_id", parameterId }              
               
                }               
                );
            return dataParameter.FirstOrDefault();
        }
        #endregion

        #region Crear parametros
        public async Task<MessageResponse> CreateParameter(ParameterRequest request)
        {
            try 
            {
                var keysResultsJson = JsonSerializer.Serialize(request.KeysResults);
                var result = await _dbConnection.ExecuteNonQueryAsync(
                "CALL api_insert_parameter(@p_name, @p_description, @p_unit_of_measure, @p_keys_results)",
                new Dictionary<string, object>
                {
                    { "p_name", request.Name },
                    { "p_description", request.Description },
                    { "p_unit_of_measure", request.UnitOfMeasure },
                    { "p_keys_results", keysResultsJson }
                }
                );

                //Se verifica si se efectuo el cambio en BBDD
                if (result > 0)
                {
                    return MessageResponse.Create(CommonSuccess.GenericCreateSuccess1, true);
                }               
                else 
                {
                    return MessageResponse.Create(CommonErrors.GenericNoValid3);
                }
            }
            catch (Exception ex) 
            {
                return MessageResponse.Create(CommonErrors.UnexpectedError(ex.Message));
            }            
        }

        #endregion

        #region Modificar parametro
        public async Task<MessageResponse> UpdateParameter(int parameterId, ParameterRequest request)
        {
            try
            {
                var keysResultsJson = JsonSerializer.Serialize(request.KeysResults);
                var result = await _dbConnection.ExecuteNonQueryAsync(
                "CALL api_update_parameter(@p_id, @p_name, @p_description, @p_unit_of_measure, @p_keys_results)",
                new Dictionary<string, object>
                {
                    { "p_id", parameterId },
                    { "p_name", request.Name },
                    { "p_description", request.Description },
                    { "p_unit_of_measure", request.UnitOfMeasure },
                    { "p_keys_results", keysResultsJson }
                }            
                );

                //Se verifica si se efectuo el cambio en BBDD
                if (result > 0)
                {
                    return MessageResponse.Create(CommonSuccess.GenericUpdateSuccess1, true);
                }
                else
                {
                    return MessageResponse.Create(CommonErrors.GenericNoValid3);
                }
            }
            catch (Exception ex)
            {
                return MessageResponse.Create(CommonErrors.UnexpectedError(ex.Message));
            }
        }

        #endregion

        #region Eliminar parametro
        public async Task<MessageResponse> DeleteParameter(int parameterId)
        {
            try
            {
                var result = await _dbConnection.ExecuteNonQueryAsync(
                "CALL api_delete_parameter(@p_id)",
                new Dictionary<string, object>
                {
                    { "p_id", parameterId }
                }
                );

                //Se verifica si se efectuo el cambio en BBDD
                if (result > 0)
                {
                    return MessageResponse.Create(CommonSuccess.GenericDeleteSuccess1, true);
                }
                else
                {
                    return MessageResponse.Create(CommonErrors.GenericNoValid3);
                }
            }
            catch (Exception ex)
            {
                return MessageResponse.Create(CommonErrors.UnexpectedError(ex.Message));
            }
        }
        #endregion

        #region Obtener todos las referencia de rango
        public async Task<IEnumerable<ReferenceRange>> GetReferencesRange()
        {
            return await _dbConnection.ExecuteQueryAsync<ReferenceRange>(
                "CALL api_get_reference_range()",              

                (reader) => new ReferenceRange              
                {          
                    Id = reader.GetInt32("id"),
                    ParameterId = reader.GetInt32("parameter_id"),
                    KeyResult = reader.GetString("key_result"),
                    MinValue = reader.IsDBNull("min_value") ? (decimal?)null : reader.GetDecimal("min_value"),
                    MaxValue = reader.IsDBNull("max_value") ? (decimal?)null : reader.GetDecimal("max_value"),                   
                    ConditionType = (ConditionType)reader.GetInt32("condition_type"),
                    ConditionValue = reader.IsDBNull("condition_value") ? (decimal?)null : reader.GetDecimal("condition_value"),
                    Gender = reader.GetString("gender"),
                    MinAge = reader.GetInt32("min_age"),
                    MaxAge = reader.GetInt32("max_age"),
                    Active = reader.GetBoolean("active"),
                    Notes = reader. GetString("notes"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    UpdatedAt = reader.GetDateTime("updated_at")               
                }
            );
        }

        #endregion

        #region Obtener referencia de rango por su id
        public async Task<ReferenceRange?> GetReferenceRangeById(int referenceRangeId)
        {
            var dataParameter = await _dbConnection.ExecuteQueryAsync<ReferenceRange>(               
                "CALL api_get_reference_range_by_id(@p_id)",       
                
                (reader) => new ReferenceRange              
                {                  
                    Id = reader.GetInt32("id"),                   
                    ParameterId = reader.GetInt32("parameter_id"),
                    KeyResult = reader.GetString("key_result"),
                    MinValue = reader.IsDBNull("min_value") ? (decimal?)null : reader.GetDecimal("min_value"),
                    MaxValue = reader.IsDBNull("max_value") ? (decimal?)null : reader.GetDecimal("max_value"),
                    ConditionType = (ConditionType)reader.GetInt32("condition_type"),
                    ConditionValue = reader.IsDBNull("condition_value") ? (decimal?)null : reader.GetDecimal("condition_value"),
                    Gender = reader.GetString("gender"),                   
                    MinAge = reader.GetInt32("min_age"),                   
                    MaxAge = reader.GetInt32("max_age"),                   
                    Active = reader.GetBoolean("active"),                   
                    Notes = reader.GetString("notes"),                   
                    CreatedAt = reader.GetDateTime("created_at"),                   
                    UpdatedAt = reader.GetDateTime("updated_at")
              
                },               
                new Dictionary<string, object>
                {
                    { "p_id", referenceRangeId }
                }
           );
            return dataParameter.FirstOrDefault() ?? new ReferenceRange();
        }
        #endregion

        #region Obtener referencia de rango por Parameter id
        public async Task<IEnumerable<ReferenceRange>> GetReferenceRangeByParameterId(int parameterId)
        {
            var dataParameter = await _dbConnection.ExecuteQueryAsync<ReferenceRange>(
                "CALL api_get_reference_ranges_by_parameter_id(@p_parameter_id)",

                (reader) => new ReferenceRange
                {
                    Id = reader.GetInt32("id"),
                    ParameterId = reader.GetInt32("parameter_id"),
                    KeyResult = reader.GetString("key_result"),
                    MinValue = reader.IsDBNull("min_value") ? (decimal?)null : reader.GetDecimal("min_value"),
                    MaxValue = reader.IsDBNull("max_value") ? (decimal?)null : reader.GetDecimal("max_value"),
                    ConditionType = (ConditionType)reader.GetInt32("condition_type"),
                    ConditionValue = reader.IsDBNull("condition_value") ? (decimal?)null : reader.GetDecimal("condition_value"),
                    Gender = reader.GetString("gender"),
                    MinAge = reader.GetInt32("min_age"),
                    MaxAge = reader.GetInt32("max_age"),
                    Active = reader.GetBoolean("active"),
                    Notes = reader.GetString("notes"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    UpdatedAt = reader.GetDateTime("updated_at")

                },
                new Dictionary<string, object>
                {
                    { "p_parameter_id", parameterId }
                }
           );
            return dataParameter;
        }
        #endregion

        #region Crear referencia de rango
        public async Task<MessageResponse> CreateReferenceRange(ReferenceRangeRequest request)
        {
            try
            {
                var result = await _dbConnection.ExecuteNonQueryAsync(
                    "CALL api_insert_reference_range(@p_parameter_id, @p_min_value, @p_max_value, @p_condition_type, @p_condition_value, @p_gender, @p_min_age, @p_max_age, @p_active, @p_notes, @p_key_result)",
                    
                    new Dictionary<string, object>
                    {
                        { "p_parameter_id", request.ParameterId },
                        { "p_min_value", request.MinValue },
                        { "p_max_value", request.MaxValue },
                        { "p_condition_type", request.ConditionType },
                        { "p_condition_value", request.ConditionValue },
                        { "p_gender", request.Gender },
                        { "p_min_age", request.MinAge },
                        { "p_max_age", request.MaxAge },
                        { "p_active", request.Active },
                        { "p_notes", request.Notes },
                        { "p_key_result", request.KeyResult }
                    }
                    );

                return MessageResponse.Create(CommonSuccess.GenericCreateSuccess1, true);
            }
            catch (Exception ex)
            {
                return MessageResponse.Create(CommonErrors.GenericNoValid2);
            }
        }

        #endregion

        #region Modificar referencia de rango
        public async Task<MessageResponse> UpdateReferenceRange(int referenceRangeId, ReferenceRangeRequest request)
        {
            try
            {
                var result = await _dbConnection.ExecuteNonQueryAsync(

                    "CALL api_update_reference_range(@p_id, @p_parameter_id, @p_min_value, @p_max_value, @p_condition_type, @p_condition_value, @p_gender, @p_min_age, @p_max_age, @p_active, @p_notes, @p_key_result)",               
                    new Dictionary<string, object>               
                    {        
                        { "p_id", referenceRangeId },                         
                        { "p_parameter_id", request.ParameterId },
                        { "p_min_value", request.MinValue },
                        { "p_max_value", request.MaxValue },
                        { "p_condition_type", request.ConditionType },
                        { "p_condition_value", request.ConditionValue },
                        { "p_gender", request.Gender },
                        { "p_min_age", request.MinAge },
                        { "p_max_age", request.MaxAge },
                        { "p_active", request.Active },
                        { "p_notes", request.Notes },
                        { "p_key_result", request.KeyResult }
                    }  
                    );

                return MessageResponse.Create(CommonSuccess.GenericUpdateSuccess1, true);
            }
            catch (Exception ex)
            {
                return MessageResponse.Create(CommonErrors.GenericNoValid2);
            }
        }

        #endregion

        #region Eliminar referencia de rango
        public async Task<MessageResponse> DeleteReferenceRange(int referenceRangeId)
        {
            try
            {
                var result = await _dbConnection.ExecuteNonQueryAsync(
                "CALL api_delete_reference_range(@p_id)",
                new Dictionary<string, object>
                {
                    { "p_id", referenceRangeId }
                }
                );

                //Se verifica si se efectuo el cambio en BBDD
                if (result > 0)
                {
                    return MessageResponse.Create(CommonSuccess.GenericDeleteSuccess1, true);
                }
                else
                {
                    return MessageResponse.Create(CommonErrors.GenericNoValid3);
                }
            }
            catch (Exception ex)
            {
                return MessageResponse.Create(CommonErrors.UnexpectedError(ex.Message));
            }
        }
        #endregion

        #region Procesar archivo y analisis por parametros        
        public async Task<List<AnalysisResult>> ProcesarArchivoJsonAsync(Stream jsonStream, string fileKey, int userId)
        {
            var resultados = new List<AnalysisResult>();

            var parametros = await GetParametersFromJsonAsync(jsonStream);

            foreach (var param in parametros)
            {
                await _dbConnection.ExecuteNonQueryAsync(
                    "CALL api_set_parameter_results(@p_nombre, @p_key_result, @p_valor, @p_file_key, @p_user_id);",
                    new Dictionary<string, object>
                    {
                        { "p_nombre", param.Nombre },
                        { "p_key_result", param.KeyResult },
                        { "p_valor", param.Dato },
                        { "p_file_key", fileKey },
                        { "p_user_id", userId }
                    });    
            }
            resultados = await GetProcessedAnalysisResultsAsync(fileKey);

            return resultados;
        }
        #endregion

        #region Consultar analisis ya realizado por cada parametro
        public async Task<List<AnalysisResult>> GetProcessedAnalysisResultsAsync(string fileKey)
        {       

            var spResults = await _dbConnection.ExecuteQueryAsync(
                "CALL api_get_parameter_results(@p_file_key);",
                reader => new SpParameterResult
                {
                    // Campos de la tabla PARAMETER_RESULTS
                    parameter_result_id = reader.GetInt32("parameter_result_id"),                    
                    file_key = reader.GetString("file_key"),
                    value = reader.GetDecimal("value"),
                    analysis_results = reader.IsDBNull("analysis_results") ? null : reader.GetString("analysis_results"),

                    // Campos de la tabla REFERENCE_RANGE
                    reference_range_id = reader.GetInt32("reference_range_id"),                 
                    min_value = reader.IsDBNull("min_value") ? 0.00M : reader.GetDecimal("min_value"), 
                    max_value = reader.IsDBNull("max_value") ? 0.00M : reader.GetDecimal("max_value"), 
                    condition_type = reader.GetInt32("condition_type"),
                    condition_value = reader.IsDBNull("condition_value") ? 0.00M : reader.GetDecimal("condition_value"), 
                    reference_range_key_result = reader.GetString("reference_range_key_result"),
                    condition_type_description = reader.GetString("condition_type_description"),

                    // Campos de la tabla PARAMETER
                    parameter_id = reader.GetInt32("parameter_id"),
                    parameter_name = reader.GetString("parameter_name"),
                    parameter_description = reader.GetString("parameter_description"),
                    unit_of_measure = reader.GetString("unit_of_measure"),
                    parameter_uuid = reader.IsDBNull("parameter_uuid") ? null : reader.GetGuid("parameter_uuid").ToString(),
                },
                new Dictionary<string, object>
                {
                    { "p_file_key", fileKey }
                }
            );

            if (spResults == null || !spResults.Any())
            {
                return new List<AnalysisResult>();
            }

            var groupedResults = spResults
                .GroupBy(r => r.parameter_id)
                .Select(group =>
                {
                    var firstRow = group.First();

                    return new AnalysisResult
                    {
                        Parameter = new ParametersAnalysis
                        {
                            Id = firstRow.parameter_id,
                            Uuid = firstRow.parameter_uuid,
                            Name = firstRow.parameter_name,
                            Description = firstRow.parameter_description,
                            UnitOfMeasure = firstRow.unit_of_measure
                        },
                        Analysis = new AnalysisDetails
                        {
                            Value = firstRow.value,
                            ReferenceRanges = group.Select(item => new ReferenceRangeAnalysis
                            {
                                ConditionType = item.condition_type_description,
                                ConditionValue = item.condition_value,
                                MinValue = item.min_value,
                                MaxValue = item.max_value,
                                KeyResult = item.reference_range_key_result
                            }).ToList(),
                            Results = group.Select(item => new ResultDetail
                            {
                                Id = item.parameter_result_id,
                                Recommendation = item.analysis_results
                            }).ToList()
                        }
                    };
                })
                .ToList();

            return groupedResults;
        }

        public async Task<ExaminationAnalysis?> GetProcessedAnalysisResultsAsync2(string fileKey)
        {
            string decodedFileKey = WebUtility.UrlDecode(fileKey);

            var dbResult = await _dbConnection.ExecuteQueryAsync(
                    "CALL api_get_examination_by_file_key(@p_file_key);",
                    reader => new
                    {
                        AnalysisJson = reader.IsDBNull("analysis_results") ? null : reader.GetString("analysis_results")                   
                    },
                    new Dictionary<string, object>
                    {           
                        { "p_file_key", decodedFileKey }
                    }
                );

            var rawJson = dbResult?.FirstOrDefault()?.AnalysisJson;

            if (string.IsNullOrEmpty(rawJson))
            {
                return null;
            }

            try
            {               
                var processedResult = JsonSerializer.Deserialize<ExaminationAnalysis>(rawJson);

                return processedResult;
            }
            catch (JsonException ex)
            {                
                throw new Exception("Error al procesar el formato de los resultados del análisis.", ex);
            }
        }
        #endregion

        #region Guardar Ai Recommendation
        public async Task<MessageResponse> CreateUserRecommendationAsync(AnalysisRequest request, int userId)
        {          
            var recommendationData = await _dbConnection.ExecuteQueryAsync<AnalysisSaveResponse>(
                "CALL api_insert_user_recommendation(@p_user_id, @p_result_id, @p_recommendation);",
                reader => new AnalysisSaveResponse
                {
                    NewRecommendationId = reader.GetInt32("new_recommendation_id")
                },
                new Dictionary<string, object>
                {
                    { "p_user_id", userId },               
                    { "p_result_id", request.ResultId },                
                    { "p_recommendation", request.AiRecommendation }
                }
            );
            if (recommendationData.FirstOrDefault().NewRecommendationId > 0 ) 
            {
                return MessageResponse.Create(CommonSuccess.GenericCreateSuccess1, true);
            }
            else 
            {
                return MessageResponse.Create(CommonErrors.GenericNoValid2);
            }

        }
        #endregion

        #region Obtener Parameters Results por User Id
        public async Task<IEnumerable<ParameterResult>> GetParameterResults(int userId, int? parameterId)
        {
            int? paramToUse = parameterId > 0 ? parameterId : 0;

            return await _dbConnection.ExecuteQueryAsync<ParameterResult>(
                "CALL api_get_parameter_results_by_user_and_param(@p_user_id, @p_parameter_id)",
                (reader) => new ParameterResult
                {
                    Id = reader.GetInt32("id"),
                    FileKey = reader.IsDBNull(reader.GetOrdinal("file_key")) ? null : reader.GetString("file_key"),
                    Value = reader.GetDecimal("value"),
                    AnalysisResults = reader.IsDBNull(reader.GetOrdinal("analysis_results")) ? null : reader.GetString("analysis_results"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    ReferenceRangeId = reader.GetInt32("reference_range_id"),
                    UserId = reader.GetInt32("user_id"),
                    ParameterId = reader.GetInt32("parameter_id"),
                    ReferenceRangeMin = reader.IsDBNull(reader.GetOrdinal("reference_range_min")) ? null : reader.GetString("reference_range_min"),
                    ReferenceRangeMax = reader.IsDBNull(reader.GetOrdinal("reference_range_max")) ? null : reader.GetString("reference_range_max")
                },
                new Dictionary<string, object>
                {
                    { "p_user_id", userId },            
                    { "p_parameter_id", paramToUse }
                }
            );
           
        }
        #endregion

        #region Metodos/Utilidades
        public ItemDetails GetItemDetailsByTypeAndValue(HealthEvaluationType type, int value, AllHealthResources allResources)
        {
            ItemDetails selectedItem = null;

            switch (type)
            {
                case HealthEvaluationType.Physical:
                    if (allResources.Physical == null) return null;
                    if (value <= 9)
                    {
                        selectedItem = allResources.Physical.Item1;
                    }
                    else if (value >= 10 && value < 15)
                    {
                        selectedItem = allResources.Physical.Item2;
                    }
                    else if (value >= 15 && value < 20)
                    {
                        selectedItem = allResources.Physical.Item3;
                    }
                    else if (value >= 20)
                    {
                        selectedItem = allResources.Physical.Item4;
                    }
                    break;

                case HealthEvaluationType.Emotional:

                    if (allResources.Emotional == null) return null;
                    if (value >= 1 && value <= 2)
                    {
                        selectedItem = allResources.Emotional.Item1;
                    }
                    else if (value >= 2.1 && value <= 3)
                    {
                        selectedItem = allResources.Emotional.Item2;
                    }
                    else if (value >= 3.1 && value <= 4)
                    {
                        selectedItem = allResources.Emotional.Item3;
                    }
                    else if (value >= 4.1 && value <= 5)
                    {
                        selectedItem = allResources.Emotional.Item4;
                    }
                    break;

                case HealthEvaluationType.Finance:

                    if (allResources.Finance == null) return null;
                    if (value == 1 || value == 2)
                    {
                        selectedItem = allResources.Finance.Item1;
                    }
                    else if (value == 3)
                    {
                        selectedItem = allResources.Finance.Item2;
                    }
                    else if (value == 4 || value == 5)
                    {
                        selectedItem = allResources.Finance.Item3;
                    }
                    else
                    {
                        selectedItem = allResources.Finance.Item4;
                    }
                    break;

                case HealthEvaluationType.Goldberg:

                    if (allResources.Goldberg == null) return null;
                    if (value <= 12)
                    {
                        selectedItem = allResources.Goldberg.Item1;
                    }
                    else if (value >= 13 && value <= 20)
                    {
                        selectedItem = allResources.Goldberg.Item2;
                    }
                    else if (value >= 21 && value <= 28)
                    {
                        selectedItem = allResources.Goldberg.Item3;
                    }
                    else if (value >= 29 && value <= 36)
                    {
                        selectedItem = allResources.Goldberg.Item4;
                    }
                    break;

                default:
                    Console.WriteLine($"Tipo de evaluación '{type}' no reconocido.");
                    break;
            }

            if (selectedItem == null)
            {
                Console.WriteLine($"Advertencia: No se pudo seleccionar un ItemDetails para el tipo '{type}' con valor {value}.");
            }

            return selectedItem;
        }           
        public async Task<List<ParameterPlane>> GetParametersFromJsonAsync(Stream jsonStream)
        {
            var parametros = new List<ParameterPlane>();

            using var doc = await JsonDocument.ParseAsync(jsonStream);
            var root = doc.RootElement;

            // Verificamos si la raíz es un array
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in root.EnumerateArray())
                {
                    foreach (var item in element.EnumerateObject())
                    {
                        var value = item.Value;

                        if (value.ValueKind == JsonValueKind.Object)
                        {
                            var hasSubParameters = value.EnumerateObject().Any(p =>
                                p.Value.ValueKind == JsonValueKind.Object ||
                                p.Value.ValueKind == JsonValueKind.String ||
                                p.Value.ValueKind == JsonValueKind.Number
                            );

                            // Caso 1: agrupador con subparámetros
                            if (hasSubParameters && value.EnumerateObject().All(p => p.Value.ValueKind == JsonValueKind.Object))
                            {
                                foreach (var subItem in value.EnumerateObject())
                                {
                                    var nombre = subItem.Name.ToLowerInvariant();
                                    var subValue = subItem.Value;

                                    if (subValue.ValueKind == JsonValueKind.Object)
                                    {
                                        foreach (var inner in subValue.EnumerateObject())
                                        {
                                            var keyResult = inner.Name.ToLowerInvariant();
                                            if (keyResult == "unidad de medida") continue;

                                            if (TryParseValor(inner.Value, out var valor))
                                            {
                                                parametros.Add(new ParameterPlane
                                                {
                                                    Nombre = nombre,
                                                    KeyResult = keyResult,
                                                    Dato = valor
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                            // Caso 2: parámetro directo
                            else
                            {
                                var nombre = item.Name.ToLowerInvariant();
                                foreach (var inner in value.EnumerateObject())
                                {
                                    var keyResult = inner.Name.ToLowerInvariant();
                                    if (keyResult == "unidad de medida") continue;

                                    if (TryParseValor(inner.Value, out var valor))
                                    {
                                        parametros.Add(new ParameterPlane
                                        {
                                            Nombre = nombre,
                                            KeyResult = keyResult,
                                            Dato = valor
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return parametros;
        }
        private bool TryParseValor(JsonElement valorElement, out decimal valor)
        {
            valor = 0;

            try
            {
                if (valorElement.ValueKind == JsonValueKind.Number)
                {
                    return valorElement.TryGetDecimal(out valor);
                }
                else if (valorElement.ValueKind == JsonValueKind.String)
                {
                    var str = valorElement.GetString()?.Replace(",", ".").Replace("%", "").Trim();
                    return decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out valor);
                }
            }
            catch { }

            return false;
        }

        #endregion

    }
}
