FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy csproj and restore
COPY APMoodle.csproj .
RUN dotnet restore "APMoodle.csproj" --disable-parallel

# Copy everything else
COPY . .

# Publish
RUN dotnet publish "APMoodle.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "APMoodle.dll"]