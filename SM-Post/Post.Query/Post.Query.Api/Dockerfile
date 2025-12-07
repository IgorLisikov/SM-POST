# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy only the project files needed for restore (faster caching)
COPY Post.Query/Post.Query.Api/Post.Query.Api.csproj Post.Query/Post.Query.Api/
COPY Post.Common/Post.Common.csproj Post.Common/
COPY CQRS.Core/CQRS.Core.csproj CQRS.Core/

# Restore dependencies for Post.Query.Api
RUN dotnet restore Post.Query/Post.Query.Api/Post.Query.Api.csproj

# Copy the rest of the source code
COPY . .

# Publish Post.Query.Api
WORKDIR /src/Post.Query/Post.Query.Api
RUN dotnet publish -c Release -o /app

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Post.Query.Api.dll"]
