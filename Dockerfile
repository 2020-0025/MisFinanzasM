# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar archivos del proyecto
COPY *.csproj ./
RUN dotnet restore

# Copiar todo el código
COPY . ./
RUN dotnet publish -c Release -o out

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Exponer el puerto que usa Render
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

# Comando de inicio
ENTRYPOINT ["dotnet", "MisFinanzas.dll"]