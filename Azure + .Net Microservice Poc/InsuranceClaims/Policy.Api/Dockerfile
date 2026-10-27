FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY InsuranceClaims/Policy.Api/Policy.Api.csproj InsuranceClaims/Policy.Api/
RUN dotnet restore InsuranceClaims/Policy.Api/Policy.Api.csproj

COPY InsuranceClaims/Policy.Api/ InsuranceClaims/Policy.Api/
WORKDIR /src/InsuranceClaims/Policy.Api
RUN dotnet build Policy.Api.csproj -c Release --no-restore -o /app/build

FROM build AS publish
RUN dotnet publish Policy.Api.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "Policy.Api.dll"]
