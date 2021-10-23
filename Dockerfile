FROM mcr.microsoft.com/dotnet/sdk:5.0

WORKDIR /usr/local/src
COPY . .
CMD ["dotnet", "run"]