# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore CareNest_Order.sln
RUN dotnet publish CareNest_Order.API/CareNest_Order.API.csproj -c Release -o /app /p:UseAppHost=false

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CareNest_Order.API.dll"]