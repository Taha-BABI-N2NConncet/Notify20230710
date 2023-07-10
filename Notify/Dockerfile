#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443


#ENV NOVUURL=http://172.20.1.31:3100/
#ENV NOVUVersion=v1
##ENV NOVUAPIKey=1310af94eafa29d62bbeb697b66397e4
#ENV NOVUAPIKeyPrefix=ApiKey

#ENV MacrokioskUserID=ubpin2n
#ENV MacrokioskPassword=-G7yAEon
#ENV MacrokioskURL=https://uat.secure.etracker.cc/ElitePH/BulkPush.aspx

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Notify/Notify.csproj", "Notify/"]
RUN dotnet restore "Notify/Notify.csproj"
COPY . .
WORKDIR "/src/Notify"
RUN dotnet build "Notify.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notify.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notify.dll"]