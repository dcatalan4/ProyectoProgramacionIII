ARG PROJECT=SuperBodega.Admin.Api

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG PROJECT
WORKDIR /src

COPY SuperBodega.sln ./
COPY NuGet.Config ./
COPY SuperBodega.Admin.Api/*.csproj SuperBodega.Admin.Api/
COPY SuperBodega.Ecommerce.Api/*.csproj SuperBodega.Ecommerce.Api/
COPY SuperBodega.Domain/*.csproj SuperBodega.Domain/
COPY SuperBodega.Infrastructure/*.csproj SuperBodega.Infrastructure/
COPY SuperBodega.Worker/*.csproj SuperBodega.Worker/
COPY SuperBodega.Tests/*.csproj SuperBodega.Tests/

RUN dotnet restore "${PROJECT}/${PROJECT}.csproj" --configfile NuGet.Config
COPY . .
RUN dotnet publish "${PROJECT}/${PROJECT}.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
ARG PROJECT
ENV PROJECT=${PROJECT}
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "dotnet ${PROJECT}.dll"]
