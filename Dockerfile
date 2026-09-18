FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["AdDiin/AdDiin.csproj", "AdDiin/"]
RUN dotnet restore "AdDiin/AdDiin.csproj"

COPY . .
WORKDIR "/src/AdDiin"
RUN dotnet publish "AdDiin.csproj" --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["sh", "-c", "dotnet AdDiin.dll --urls http://0.0.0.0:${PORT:-8080}"]
