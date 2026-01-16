# Distributed-pub-sub

> This is a distibuted version of the Publisher/Subscriber pattern.

# Create a new service

To create a new service in the project, move in `distributed_pub_sub/src` and paste the following in your terminal
changing `<ServiceName>` with the name of the service you want to create

```
# 1. CREATION FOLDER AND SOLUTION
mkdir <ServiceName>
cd <ServiceName>
dotnet new sln -n <ServiceName>

# 2. CREATION OF THE 5 PROJECTS
dotnet new webapi   -n <ServiceName>.Api
dotnet new classlib -n <ServiceName>.Business
dotnet new classlib -n <ServiceName>.Repository
dotnet new classlib -n <ServiceName>.ClientHttp
dotnet new classlib -n <ServiceName>.Shared

# 3. ADDITION OF THE PROJECTS TO THE SOLUTION
dotnet sln add <ServiceName>.Api/<ServiceName>.Api.csproj
dotnet sln add <ServiceName>.Business/<ServiceName>.Business.csproj
dotnet sln add <ServiceName>.Repository/<ServiceName>.Repository.csproj
dotnet sln add <ServiceName>.ClientHttp/<ServiceName>.ClientHttp.csproj
dotnet sln add <ServiceName>.Shared/<ServiceName>.Shared.csproj

# 4. CONFIGURATION OF DEPENDENCIES (References)
# Api depends on logic and data
dotnet add <ServiceName>.Api/<ServiceName>.Api.csproj reference <ServiceName>.Business/<ServiceName>.Business.csproj <ServiceName>.Repository/<ServiceName>.Repository.csproj

# Business depends on Repository and Shared models
dotnet add <ServiceName>.Business/<ServiceName>.Business.csproj reference <ServiceName>.Repository/<ServiceName>.Repository.csproj <ServiceName>.Shared/<ServiceName>.Shared.csproj

# Repository uses Shared models
dotnet add <ServiceName>.Repository/<ServiceName>.Repository.csproj reference <ServiceName>.Shared/<ServiceName>.Shared.csproj

# ClientHttp uses Shared models to map responses
dotnet add <ServiceName>.ClientHttp/<ServiceName>.ClientHttp.csproj reference <ServiceName>.Shared/<ServiceName>.Shared.csproj

# 5. INSTALLATION OF NEEDED PACKAGES (EF Core & Http)
# Repository: Entity Framework Core
dotnet add <ServiceName>.Repository/<ServiceName>.Repository.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add <ServiceName>.Repository/<ServiceName>.Repository.csproj package Microsoft.EntityFrameworkCore.Design

# ClientHttp: extensions for HttpClient
dotnet add <ServiceName>.ClientHttp/<ServiceName>.ClientHttp.csproj package Microsoft.Extensions.Http
```

In the future I will create a simple bash script to make all this commands automated
