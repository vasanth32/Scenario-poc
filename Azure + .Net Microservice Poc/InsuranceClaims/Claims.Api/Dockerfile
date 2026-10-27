FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY InsuranceClaims/Claims.Api/Claims.Api.csproj InsuranceClaims/Claims.Api/
RUN dotnet restore InsuranceClaims/Claims.Api/Claims.Api.csproj

COPY InsuranceClaims/Claims.Api/ InsuranceClaims/Claims.Api/
WORKDIR /src/InsuranceClaims/Claims.Api
RUN dotnet build Claims.Api.csproj -c Release --no-restore -o /app/build

FROM build AS publish
RUN dotnet publish Claims.Api.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "Claims.Api.dll"]
