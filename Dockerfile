FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /usr/local/src
COPY . .
CMD ["dotnet", "run"]