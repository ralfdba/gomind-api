using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal.Transform;
using BCrypt.Net;
using gomind_backend_api.Bcrypt;
using gomind_backend_api.DB;
using gomind_backend_api.Models.Appointments;
using gomind_backend_api.Models.Errors;
using gomind_backend_api.Models.Health;
using gomind_backend_api.Models.Parameters;
using gomind_backend_api.Models.Products;
using gomind_backend_api.Models.ReferenceRange;
using gomind_backend_api.Models.User;
using gomind_backend_api.Models.Utils;
using gomind_backend_api.Resources;
using MySqlConnector;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text.Json;
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
        private readonly IMariaDbConnection _dbConnection;
        private readonly IHealthResourcesService _healthResourcesService; 

        private readonly BcryptServices _bcrypt;
       
        public BL(IMariaDbConnection dbConnection, IHealthResourcesService healthResourcesService)
        {
            _dbConnection = dbConnection;
            _healthResourcesService = healthResourcesService;
            _bcrypt = new BcryptServices();
        }
        #region Consultar Analisis por Parametros
        public async Task<List<ResultParameterRangeReference>> ProcesarArchivoJsonAsync(Stream jsonStream)
        {            
            var resultados = new List<ResultParameterRangeReference>();

            using var reader = new StreamReader(jsonStream);
            var json = await reader.ReadToEndAsync();

            var data = JsonSerializer.Deserialize<Dictionary<string, ResultadoAnalisis>>(json)
                             ?? new Dictionary<string, ResultadoAnalisis>();

            foreach (var kvp in data)
            {
                var nombre = kvp.Key;
                decimal valor = decimal.Parse(kvp.Value.Valor, CultureInfo.InvariantCulture);

                var notas = await _dbConnection.ExecuteQueryAsync(
                    "CALL api_get_parameter_notes(@p_nombre, @p_valor);",            
                   
                    reader =>   
                    {                
                        return new              
                        {
                            Uuid = reader.IsDBNull("uuid") ? null : reader.GetGuid("uuid").ToString(),
                            Name = reader.IsDBNull("name") ? null : reader.GetString("name"),
                            Description = reader.IsDBNull("description") ? null : reader.GetString("description"),
                            UnitOfMeasure = reader.IsDBNull("unit_of_measure") ? null : reader.GetString("unit_of_measure"),
                            Note = reader.IsDBNull("note") ? null : reader.GetString("note")
                        };
                    },
                    new Dictionary<string, object>
                    {
                        { "p_nombre", nombre },
                        { "p_valor", valor }
                    });
                
                if (!notas.Any() || notas.First().Uuid == null)
                    continue;

                // Se toman los datos del parámetro de la primera fila
                var first = notas.First();

                resultados.Add(new ResultParameterRangeReference
                {
                    Parameter = new ParametersRangeReference
                    {
                        Uuid = first.Uuid,
                        Name = first.Name,
                        Description = first.Description,
                        UnitOfMeasure = first.UnitOfMeasure
                    },
                    Analysis = new Analysis
                    {
                        Value = valor,
                        Results = notas.Select(n => n.Note).ToList()
                    }
                });
            }

            return resultados;
        }
        #endregion

        #region Obtener Usuario Email
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

        #region Agendar cita a usuario
        public async Task<AppointmentsResponse> CreateAppointmentByUser(AppointmentsRequest request)
        {
           
            var appointmentData = await _dbConnection.ExecuteQueryAsync<AppointmentsResponse>(               
                "CALL api_insert_users_appointment(@p_users_id, @p_schedule_day, @p_health_provider_id, @p_product_id)",
                reader => new AppointmentsResponse
                {                    
                    AppointmentId = reader.GetInt32("appointment_id")
                },
                new Dictionary<string, object>              
                {
                    { "p_users_id", request.UserId },
                    { "p_schedule_day", request.DateTime },
                    { "p_health_provider_id", request.HealthProviderId },
                    { "p_product_id", request.ProductId }
                }
            );
            return appointmentData.FirstOrDefault();
        }
        #endregion

        #region Guardar Perfil Integral de Salud
        public async Task<MessageResponse> CreateHealthProfile(Health.HealthProfileRequest request)
        {         
            try
            {
                // Inicia la transacción
                // TransactionScope es ideal para coordinar múltiples operaciones de DB
                // en una sola transacción, incluyendo transacciones distribuidas (MSDTC si es necesario).
                using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    if (request.Physical != null) {
                        // SP1: Guardar datos físicos
                        await _dbConnection.ExecuteNonQueryAsync(
                            "CALL api_insert_users_health_phisically(@p_user_id, @q1, @q2, @q3, @q4, @q5, @q6, @q7, @q8, @q9, @q10)",
                            new Dictionary<string, object>
                            {
                            { "p_user_id", request.UserId },
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
                            { "p_user_id", request.UserId },
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
                            { "p_user_id", request.UserId },
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
                            { "p_user_id", request.UserId },
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
                (reader) => new Parameters
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Description = reader.GetString("description"),
                    UnitOfMeasure = reader.GetString("unit_of_measure")
                }
            );
        }
        #endregion

        #region Obtener parametro por su id
        public async Task<Parameters?> GetParametersById(int parameterId)
        {
            var dataParameter = await _dbConnection.ExecuteQueryAsync<Parameters>(
               "CALL api_get_parameter_by_id(@p_id)",
               (reader) => new Parameters
               {
                   Id = reader.GetInt32("id"),
                   Name = reader.GetString("name"),
                   Description = reader.GetString("description"),
                   UnitOfMeasure = reader.GetString("unit_of_measure")
               },
                new Dictionary<string, object>
                {
                    { "p_id", parameterId }
                }
           );
            return dataParameter.FirstOrDefault() ?? new Parameters();
        }
        #endregion

        #region Crear parametros
        public async Task<MessageResponse> CreateParameter(ParameterRequest request)
        {
            try 
            {
                var result = await _dbConnection.ExecuteNonQueryAsync(
                "CALL api_insert_parameter(@p_name, @p_description, @p_unit_of_measure)",
                new Dictionary<string, object>
                {
                    { "p_name", request.Name },
                    { "p_description", request.Description },
                    { "p_unit_of_measure", request.UnitOfMeasure }
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
                var result = await _dbConnection.ExecuteNonQueryAsync(
                "CALL api_update_parameter(@p_id, @p_name, @p_description, @p_unit_of_measure)",
                new Dictionary<string, object>
                {
                    { "p_id", parameterId },
                    { "p_name", request.Name },
                    { "p_description", request.Description },
                    { "p_unit_of_measure", request.UnitOfMeasure }
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

        #region Crear referencia de rango
        public async Task<MessageResponse> CreateReferenceRange(ReferenceRangeRequest request)
        {
            try
            {
                var result = await _dbConnection.ExecuteNonQueryAsync(                
                    "CALL api_insert_reference_range(@p_parameter_id, @p_min_value, @p_max_value, @p_condition_type, @p_condition_value, @p_gender, @p_min_age, @p_max_age, @p_active, @p_notes)",
                    
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
                        { "p_notes", request.Notes }
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

                    "CALL api_update_reference_range(@p_id, @p_parameter_id, @p_min_value, @p_max_value, @p_condition_type, @p_condition_value, @p_gender, @p_min_age, @p_max_age, @p_active, @p_notes)",               
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
                        { "p_notes", request.Notes }
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
        #endregion
    }
}
