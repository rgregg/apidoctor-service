# What we need....
# 1. GIT so we can retrieve the contents of the remote repo
# 2. the .NET binary for apidoctor-service

FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY opensource/apidoctor-cs/ApiDoctor.Validation/*.csproj ./opensource/apidoctor-cs/ApiDoctor.Validation/
RUN dotnet restore ./opensource/apidoctor-cs/ApiDoctor.Validation/ApiDoctor.Validation.csproj

COPY service-runner/service-runner.csproj ./service-runner/
RUN dotnet restore ./service-runner/service-runner.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish ./service-runner/service-runner.csproj -c Release -o ./out

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime

# Install GIT into the container
#RUN apt-get install -y software-properties-common
#RUN apt-add-repository -y ppa:git-core/ppa
RUN apt-get update
RUN apt-get install -y git

WORKDIR /app
COPY --from=build-env /app/service-runner/out .

EXPOSE 8080
ENTRYPOINT ["dotnet", "service-runner.dll"]