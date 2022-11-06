FROM mcr.microsoft.com/dotnet/sdk:6.0

WORKDIR /usr/local/src
COPY . .
CMD ["dotnet", "run"]