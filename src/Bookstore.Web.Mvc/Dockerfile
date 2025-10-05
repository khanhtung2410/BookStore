FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli
RUN ln -s ~/.dotnet/tools/libman /usr/local/bin/libman

WORKDIR /src
COPY ["src/Bookstore.Web.Mvc/Bookstore.Web.Mvc.csproj", "src/Bookstore.Web.Mvc/"]
COPY ["src/Bookstore.Web.Core/Bookstore.Web.Core.csproj", "src/Bookstore.Web.Core/"]
COPY ["src/Bookstore.Application/Bookstore.Application.csproj", "src/Bookstore.Application/"]
COPY ["src/Bookstore.Core/Bookstore.Core.csproj", "src/Bookstore.Core/"]
COPY ["src/Bookstore.EntityFrameworkCore/Bookstore.EntityFrameworkCore.csproj", "src/Bookstore.EntityFrameworkCore/"]
WORKDIR "/src/src/Bookstore.Web.Mvc"
RUN dotnet restore 

WORKDIR /src
COPY ["src/Bookstore.Web.Mvc", "src/Bookstore.Web.Mvc"]
COPY ["src/Bookstore.Web.Core", "src/Bookstore.Web.Core"]
COPY ["src/Bookstore.Application", "src/Bookstore.Application"]
COPY ["src/Bookstore.Core", "src/Bookstore.Core"]
COPY ["src/Bookstore.EntityFrameworkCore", "src/Bookstore.EntityFrameworkCore"]
WORKDIR "/src/src/Bookstore.Web.Mvc"
RUN libman restore
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "Bookstore.Web.Mvc.dll"]
