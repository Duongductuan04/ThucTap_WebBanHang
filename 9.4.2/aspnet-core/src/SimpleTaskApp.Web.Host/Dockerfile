FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["src/SimpleTaskApp.Web.Host/SimpleTaskApp.Web.Host.csproj", "src/SimpleTaskApp.Web.Host/"]
COPY ["src/SimpleTaskApp.Web.Core/SimpleTaskApp.Web.Core.csproj", "src/SimpleTaskApp.Web.Core/"]
COPY ["src/SimpleTaskApp.Application/SimpleTaskApp.Application.csproj", "src/SimpleTaskApp.Application/"]
COPY ["src/SimpleTaskApp.Core/SimpleTaskApp.Core.csproj", "src/SimpleTaskApp.Core/"]
COPY ["src/SimpleTaskApp.EntityFrameworkCore/SimpleTaskApp.EntityFrameworkCore.csproj", "src/SimpleTaskApp.EntityFrameworkCore/"]
WORKDIR "/src/src/SimpleTaskApp.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/SimpleTaskApp.Web.Host", "src/SimpleTaskApp.Web.Host"]
COPY ["src/SimpleTaskApp.Web.Core", "src/SimpleTaskApp.Web.Core"]
COPY ["src/SimpleTaskApp.Application", "src/SimpleTaskApp.Application"]
COPY ["src/SimpleTaskApp.Core", "src/SimpleTaskApp.Core"]
COPY ["src/SimpleTaskApp.EntityFrameworkCore", "src/SimpleTaskApp.EntityFrameworkCore"]
WORKDIR "/src/src/SimpleTaskApp.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "SimpleTaskApp.Web.Host.dll"]
