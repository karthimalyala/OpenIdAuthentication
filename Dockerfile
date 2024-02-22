FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5197

ENV ASPNETCORE_URLS=http://+:5197

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["OpenIDConfigExample.csproj", "./"]
RUN dotnet restore "OpenIDConfigExample.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "OpenIDConfigExample.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "OpenIDConfigExample.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenIDConfigExample.dll"]

