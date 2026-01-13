# Gomind API Backend

## Descripción
Proyecto backend API para gomind que expone servicios REST en .NET 8 (C# 12) para la gestión de acceso y funcionalidades relacionadas con usuarios. Su objetivo principal es autenticar usuarios mediante JWT y soportar flujos de verificación (por ejemplo, envío de códigos de acceso) usando plantillas HTML almacenadas en el repositorio y recursos alojados en AWS (S3).

# Guía de Onboarding técnico — gomind-backend-api

## Tecnologías principales
- .NET 8 (C# 12), ASP.NET Core Web API
- Autenticación JWT (Json Web Tokens)
- MariaDB como base de datos relacional
- AWS: S3, DynamoDB y logging con `AWS.Logger`
- Plantillas de correo HTML en `Templates/`
- Estructura por capas: `BL/`, `DB/`, `Models/`, `Services/`, `Controllers/`

## Estructura clave del repositorio
- `Program.cs` — arranque, configuración de servicios, JWT y middleware.
- `Services/EnvioCorreoService.cs` — lógica de envío de correos y uso de plantillas.
- `Templates/envio-codigo.html` — plantilla HTML del correo de verificación.
- `appsettings.json`, `appsettings.Development.json` — configuración por entorno.
- `BL/`, `DB/`, `Models/`, `Controllers/` — lógica de negocio, acceso a datos, modelos y controladores.

## Requisitos locales
------------------
1. .NET 8 SDK instalado.
2. Visual Studio 2022 (actualizado) o `dotnet` CLI.
3. Acceso a una instancia MariaDB (local o remota).
4. Credenciales AWS con permisos si vas a usar S3/DynamoDB localmente.

### Variables de entorno y secrets (mínimo)
- `JWT_SECRET` — clave simétrica para firmar/validar tokens (se lee desde variables de entorno).
- `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_REGION` — para operaciones AWS.
- Cualquier otro secreto debe almacenarse fuera del repo y cargarse vía variables de entorno o secretos del entorno de despliegue.

## Configuración y ejecución local
-------------------------------
1. Abrir la solución en Visual Studio 2022.
2. Configurar variables de entorno para el perfil de depuración: abrir __Project Properties > Debug__ y añadir `JWT_SECRET` y credenciales AWS según sea necesario.
3. Construir: usar __Build > Build Solution__.
4. Ejecutar con depuración: __Debug > Start Debugging__.
5. Alternativa con CLI:
   - `dotnet restore`
   - `dotenv
   - `dotnet run --project <ruta-al-proyecto>`

## Autenticación JWT
-----------------
- `Program.cs` carga `JwtSettings` desde `appsettings.*` (Issuer/Audience) y requiere `JWT_SECRET` en variables de entorno.
- Validación: issuer, audience, lifetime y firma se verifican.
- Para probar endpoints protegidos, enviar header:
  - `Authorization: Bearer <token>`

## Correo y plantillas
-------------------
- Las plantillas HTML se encuentran en `Templates/`. `EnvioCorreoService` las consume y reemplaza marcadores (ej. `[CODIGO]`).
- La plantilla `Templates/envio-codigo.html` contiene estilos inline y estructura pensada para clientes de correo.

## Logs y diagnóstico
------------------
- `AWS.Logger` está integrado; revisa `appsettings*.json` para niveles y destinos.
- `Program.cs` personaliza la respuesta 401 para tokens inválidos retornando un JSON con error legible.
- Para ver salidas en Visual Studio usa la ventana Output o el panel de tests según lo que ejecutes.

## Tareas y patrones comunes
-------------------------
- Añadir endpoint: crear controlador en `Controllers/`, usar dependencias inyectadas y añadir servicios en `Program.cs` si son nuevos.
- Añadir servicio: implementar en `Services/` y registrarlo en DI en `Program.cs`.
- Nueva plantilla de correo: agregar HTML en `Templates/` y adaptarla en `EnvioCorreoService`.
- Evita commitear secretos; usa variables de entorno o gestor de secretos.

## Archivos a revisar primero
--------------------------
- `Program.cs` — cómo se inicializa la app y se configuran JWT, DB y AWS.
- `Services/EnvioCorreoService.cs` — integración con S3/plantillas y envío.
- `Templates/envio-codigo.html` — ejemplo de plantilla usable.
- `appsettings.json` / `appsettings.Development.json` — valores por entorno y placeholders.

## Comandos rápidos útiles
-----------------------
- Restaurar paquetes: `dotnet restore`
- Compilar: `dotnet build`
- Ejecutar: `dotnet run --project .\gomind-backend-api\`
- Ejecutar tests (si existen): `dotnet test`

## Próximos pasos recomendados
---------------------------
1. Añadir `JWT_SECRET` en tu perfil de depuración local y verificar la generación/validación de tokens.
2. Levantar o conectar a MariaDB y validar conexiones desde `Program.cs`.
3. Probar envío de correos usando `EnvioCorreoService` y la plantilla `envio-codigo.html`.
4. Revisar `appsettings.Development.json` para ajustar valores locales.