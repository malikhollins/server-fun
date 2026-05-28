# Use the official .NET core runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["player.csproj", "./"]
RUN dotnet restore "./player.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "player.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "player.csproj" -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "player.dll"]