using Amazon.DynamoDBv2;
using Amazon.S3;
using AWS.Logger;
using gomind_backend_api.AWS;
using gomind_backend_api.BL;
using gomind_backend_api.DB;
using gomind_backend_api.JWT;
using gomind_backend_api.Models.Errors;
using gomind_backend_api.Models.Utils;
using gomind_backend_api.Resources;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region JWT
// Cargar configuración de JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

// Validaciones básicas para las settings JWT
if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
{
    throw new InvalidOperationException("Missing JWT settings.");
}

// Registrar JwtServices en el contenedor de servicios
builder.Services.AddSingleton<JwtServices>(new JwtServices(secretKey, issuer, audience));

// --- INICIO: AGREGAR CONFIGURACIÓN DE AUTENTICACIÓN JWT ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,       
            ValidAudience = audience,  
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)) 
        };
       
        // Se personaliza el mensaje de error al proporcionar un token no valido
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                // Desactiva el comportamiento predeterminado (no envíes encabezados WWW-Authenticate adicionales)
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                
                var errorResponse = MessageResponse.Create(CommonErrors.InvalidToken);             

                return context.Response.WriteAsJsonAsync(errorResponse);
            }
        };       

    });

builder.Services.AddAuthorization(); // Habilitar el middleware de autorización

#endregion

#region MariaDB
// Registrar MariaDbConnection en el contenedor de servicios.
builder.Services.AddScoped<IMariaDbConnection, MariaDbConnection>();
#endregion

#region AWS Settings
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
#endregion

#region AWS Cloudwatch
var region = builder.Configuration["AWS:Region"];
var logGroup = builder.Configuration["AWS:LogGroup"];

builder.Logging.AddAWSProvider(new AWSLoggerConfig
{
    Region = region,
    LogGroup = logGroup
});
#endregion

#region AWS S3
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<IS3Service, S3Service>();
#endregion

#region AWS DynamoDb
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddScoped<IDynamoDbService, DynamoDbService>();
#endregion

#region BL
builder.Services.AddScoped<BL>();
#endregion

#region Resources Services
builder.Services.AddSingleton<IHealthResourcesService, HealthResourcesService>();

builder.Services.Configure<EmailSettingsOptions>(builder.Configuration.GetSection(EmailSettingsOptions.EmailSettings));
builder.Services.Configure<CorreoFromOptions>(builder.Configuration.GetSection("CorreoFrom"));
builder.Services.AddScoped<INotificacion, Notificacion>();
builder.Services.AddScoped<IEnvioCorreoService, EnvioCorreoService>();

#endregion

#region Controllers
builder.Services.AddControllers()    
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key.Split('.').Last(), // limpia "Physical.q3"
                    kvp => kvp.Value.Errors.Select(err => err.ErrorMessage).ToArray()
                );

            var result = CommonErrors.ValidationError(errors);
            return new BadRequestObjectResult(result);
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
#endregion

#region Configuracion Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Gomind - API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Por favor, introduce tu token.",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.EnableAnnotations();
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

#endregion

#region Build
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gomind API");
    });
}

app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        message = "Gomind API"
    });
});

app.MapHealthChecks("/healthz");
app.UseCors(builder => builder
 .AllowAnyOrigin()
 .AllowAnyMethod()
 .AllowAnyHeader()
);

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
#endregion


