FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY . .
RUN cp cert/*.crt /usr/local/share/ca-certificates/ 2>/dev/null \
5&& update-ca-certificates \
6|| echo "No specific certificate given."
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENTRYPOINT ["dotnet", "MarginTrading.TradingHistory.dll"]
