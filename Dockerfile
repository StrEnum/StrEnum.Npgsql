FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY src/StrEnum.Npgsql/StrEnum.Npgsql.csproj ./src/StrEnum.Npgsql/StrEnum.Npgsql.csproj
COPY test/StrEnum.Npgsql.UnitTests/StrEnum.Npgsql.UnitTests.csproj ./test/StrEnum.Npgsql.UnitTests/StrEnum.Npgsql.UnitTests.csproj
RUN dotnet restore ./src/StrEnum.Npgsql/StrEnum.Npgsql.csproj
RUN dotnet restore ./test/StrEnum.Npgsql.UnitTests/StrEnum.Npgsql.UnitTests.csproj

# copy everything else and build app
COPY ./ ./
WORKDIR /source
RUN dotnet build ./src/StrEnum.Npgsql/StrEnum.Npgsql.csproj -c release --no-restore /p:maxcpucount=1
RUN dotnet build ./test/StrEnum.Npgsql.UnitTests/StrEnum.Npgsql.UnitTests.csproj -c release --no-restore /p:maxcpucount=1

FROM build AS test
RUN dotnet test ./test/StrEnum.Npgsql.UnitTests/StrEnum.Npgsql.UnitTests.csproj --no-build -c release /p:maxcpucount=1

FROM build AS pack-and-push
WORKDIR /source

ARG PackageVersion
ARG NuGetApiKey

RUN dotnet pack ./src/StrEnum.Npgsql/StrEnum.Npgsql.csproj -o /out/package -c Release
RUN dotnet nuget push /out/package/StrEnum.Npgsql.$PackageVersion.nupkg -k $NuGetApiKey -s https://api.nuget.org/v3/index.json
