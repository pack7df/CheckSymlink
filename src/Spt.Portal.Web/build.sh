#!/bin/bash
set -e
dotnet build "Spt.Portal.Web.csproj" -c Release
dotnet publish "Spt.Portal.Web.csproj" -c Release -o obj/Docker/publish /p:LinkDuringPublish=true
docker image build -t portal-web .
for tag in {${BUILD_BUILDNUMBER:-0.0.1},latest}; do
    docker tag portal-web sptcloud.azurecr.io/portal-web:${tag}
    docker push sptcloud.azurecr.io/portal-web:${tag}
    docker image rm sptcloud.azurecr.io/portal-web:${tag}
done
docker image rm portal-web