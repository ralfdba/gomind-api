# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# Dependiendo del sistema operativo de las máquinas host que vayan a compilar o ejecutar los contenedores, puede que sea necesario cambiar la imagen especificada en la instrucción FROM.
# Para más información, consulte https://aka.ms/containercompat

# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG BUILD_PATH=''
WORKDIR /src
COPY ["${BUILD_PATH}gomind-backend-api.csproj", "."]
RUN dotnet restore "./gomind-backend-api.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./${BUILD_PATH}gomind-backend-api.csproj" -c %BUILD_CONFIGURATION% -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG BUILD_PATH=''
RUN dotnet publish "./${BUILD_PATH}gomind-backend-api.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false

# Copiar recursos
COPY ${BUILD_PATH}Templates/ /app/publish/templates

# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
USER root
RUN apt-get update && apt-get install -y libfontconfig
WORKDIR /app
COPY --from=publish /app/publish .

# Agregar argumento de build
ARG ENV_ENVIRONMENT=Development
ARG ENV_JWT_SECRET
ARG ENV_MARIADB_CONNECTION
ARG ENV_SMTP_PASSWORD
ARG ENV_SMTP_USER

# Agregar variables de entorno
ENV ASPNETCORE_HTTP_PORTS=80
ENV ASPNETCORE_ENVIRONMENT=$ENV_ENVIRONMENT
ENV JWT_SECRET=$ENV_JWT_SECRET
ENV MARIADB_CONNECTION=$ENV_MARIADB_CONNECTION
ENV SMTP_PASSWORD=$ENV_SMTP_PASSWORD
ENV SMTP_USER=$ENV_SMTP_USER

ENTRYPOINT ["dotnet", "gomind-backend-api.dll"]
