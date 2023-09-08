FROM node:14-alpine3.15 as clientBuild
ARG CONFIGURATION=Release
ARG ARTIFACTORY_USERNAME
ARG ARTIFACTORY_PASSWORD
ARG ARTIFACTORY_TOKEN
ENV ARTIFACTORY_USERNAME ${ARTIFACTORY_USERNAME}
ENV ARTIFACTORY_PASSWORD ${ARTIFACTORY_PASSWORD}
ENV ARTIFACTORY_TOKEN ${ARTIFACTORY_TOKEN}
WORKDIR /client
COPY client/.npmrc client/package*.json ./
RUN npm install -d
COPY client .
RUN npm run build:client:${CONFIGURATION}

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.14 as netCoreBuild
ENV IS_DOCKER_BUILD 1
ARG CONFIGURATION=Release
ARG ARTIFACTORY_USERNAME
ARG ARTIFACTORY_PASSWORD
ARG ARTIFACTORY_TOKEN
ENV ARTIFACTORY_USERNAME ${ARTIFACTORY_USERNAME}
ENV ARTIFACTORY_PASSWORD ${ARTIFACTORY_PASSWORD}
ENV ARTIFACTORY_TOKEN ${ARTIFACTORY_TOKEN}
WORKDIR /build
COPY 311RequestSearch.csproj NuGet.config ./
RUN dotnet restore -v normal
COPY . .
COPY --from=clientBuild client/build spa
RUN dotnet publish 311RequestSearch.csproj -c ${CONFIGURATION} -o out -v normal

# Generated on 09/08/2023 with Armored Armadillo 1.0.0-dev

FROM tylertech-docker.jfrog.io/tyler-apm-aspnetcore:6.0-3.5-alpine
ARG VERSION="0.0.0"
ENV Version ${VERSION}
WORKDIR /app
EXPOSE 8080
COPY --from=netCoreBuild build/out .
ENV ASPNETCORE_URLS "http://*:8080"
ENTRYPOINT ["dotnet", "311RequestSearch.dll"]
