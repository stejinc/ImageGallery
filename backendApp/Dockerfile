FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

WORKDIR /src
COPY *.sln ./
COPY PhotoApp/PhotoApp.csproj PhotoApp/
COPY ImageGalleryDbHelper/ImageGalleryDbHelper.csproj ImageGalleryDbHelper/
RUN dotnet restore
COPY . .
WORKDIR /src/PhotoApp
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:5.0
EXPOSE 8080
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "PhotoApp.dll"]