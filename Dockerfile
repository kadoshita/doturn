FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /usr/local/src
COPY . .
CMD ["dotnet", "run"]