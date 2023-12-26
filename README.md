# LEAP

> Library ExchAnge Platform

A service for distributing and downloading [STEP] libraries.

## Overview

This repository contains several projects providing different parts for the LEAP ecosystem:

- `StepLang.Leap.API`: An ASP.NET project for hosting a LEAP instance
- `StepLang.Leap.API.DB`: The database layer for the `StepLang.Leap.API`
- `StepLang.Leap.Client`: A HTTP client for the `StepLang.Leap.API`
- `StepLang.Leap.Common`: A library of common code for the client and the API

Both `StepLang.Leap.Client` and `StepLang.Leap.Common` are distributed using NuGet packages and can be used to interact with a LEAP
instance.

### Running your own LEAP instance

To run your own LEAP instance, you do either of the following:

- **From Sources**:
  - Check out the repository
  - Run `dotnet run --project StepLang.Leap.API/StepLang.Leap.API.csproj`
- **From Docker**:
  - ```
    docker run \
    --rm \
    --name leap \
    -p 5000:80 \
    -v ./packages:/app/packages \
    -v ./data:/app/data \
    -e "LEAP__Database__ConnectionString=Data Source=/app/data/leap.db" \
    -e "LEAP__Database__Provider=sqlite" \
    ghcr.io/ricardoboss/leap
    ```
- **From Docker Compose**:
  - Copy the `docker-compose.yml` file from below
  - Run `docker-compose up -d`

<details>
    <summary>Docker Compose</summary>

```yaml
version: "3.9"

services:
  leap:
    image: ghcr.io/ricardoboss/leap
    ports:
      - 5000:80
    volumes:
      - ./data:/app/data
```

</details>

## Development

### Prerequisites

- .NET 8
- (optional) Docker

### Building

To build the project, run the following command:

```bash
dotnet build -c Release
```

### Testing

To run the tests, run the following command:

```bash
dotnet test
```

## License

This project is licensed under the MIT license. See the [LICENSE](LICENSE) file for more info.

[STEP]: https://github.com/ricardoboss/STEP
