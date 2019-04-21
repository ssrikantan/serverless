FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY OrderFlowBusinessMonitor/OrderFlowBusinessMonitor.csproj OrderFlowBusinessMonitor/
RUN dotnet restore OrderFlowBusinessMonitor/OrderFlowBusinessMonitor.csproj
COPY . .
WORKDIR /src/OrderFlowBusinessMonitor
RUN dotnet build OrderFlowBusinessMonitor.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish OrderFlowBusinessMonitor.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "OrderFlowBusinessMonitor.dll"]
