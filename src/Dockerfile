## BUILD
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src

ARG TELERIK_LICENSE
ARG proj="**/ItreeNet.csproj"
ARG ASSEMBLY_VERSION
ARG ASSEMBLY_INFO_VERSION

# copy csproj and restore as distinct layers
COPY /ItreeNet ./ItreeNet
COPY ItreeNet.sln .
COPY nuget.config .

RUN dotnet restore ItreeNet.sln --configfile nuget.config

# copy everything
COPY . .

# publish
RUN dotnet publish $proj -c Release -o /app/publish -p:Version=$ASSEMBLY_VERSION -p:InformationalVersion=$ASSEMBLY_INFO_VERSION -p:TelerikLicense=$TELERIK_LICENSE

## RUN
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS base
WORKDIR /app

# Copy published solution
COPY --from=build /app/publish .

# Install libgdiplus
RUN apt-get update && apt-get -y install libgdiplus

# Install windows font
RUN echo "ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true" | debconf-set-selections
RUN apt-get install -y --no-install-recommends fontconfig ttf-mscorefonts-installer

# Entferne perl
RUN apt-get remove -y perl && apt-get autoremove -y

# Entferne gnupg
RUN apt-get remove -y gnupg && apt-get autoremove -y

# Upgrade all remaining packages
RUN apt-get update && \
    apt-get upgrade -y && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Install for SSL
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf

# Configure Ports / Language settings
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV http_port 8080

EXPOSE 8080

# delete tmp dir
RUN rm -rf /tmp/*

ENTRYPOINT [ "dotnet", "ItreeNet.dll"]