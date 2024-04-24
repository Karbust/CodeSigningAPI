# Code Signing API

This project was idealized to provide a simple and easy way to sign your code using a simple API. It started as a way for me to sign my code remotely with the new CA/B Forum requirements with a HSM compatible module on CA's that don't have the possibility to use cloud providers, like Azure's KeyVault HSM (like Sectigo).

This project was started for my use case, sign Electron applications that are built within windows docker containers (TeamCity Pipeline) on Windows Server 2022 (running on IIS), but it can be used for any code signing needs, even over the internet.

## Features

* Sign code using a simple API
* IP Whitelisting
* Token Authentication

## Disclaimer

You shouldn't expose this API to the internet without proper security measures. It's recommended to use a reverse proxy like Nginx or IIS with a SSL certificate.

Also, do not rely solely on the provided security measures. Always use the best practices for security.

## Requirements

- .NET 8.0
- PostgreSQL
- HSM physical module (like YubiKey)
- Code Signing Certificate
- [jSign](https://github.com/ebourg/jsign)

## How it works

Just needs to call the `/api/Sign` endpoint and send the file to be signed as a `multipart/form-data` request. The API will return the signed file.

Example:
```bash
curl --location 'https://localhost:7165/api/Sign' \
--header 'Authorization: Bearer S70UjtjgkDMJzQdHXbbBRSNQRMxPu+YMFGyOVtOzufk=' \
--form 'file=@"/C:/Users/Karbust/Desktop/file_to_sign.exe"' \
--form 'request="{\"algorithms\":[\"sha1\",\"sha256\"]}"'
```

## Quick Start

1. Download the latest release from the [releases page](https://github.com/Karbust/CodeSigningAPI/releases)
2. Rename the `appsettings.Sample.json` to `appsettings.json` and fill the required fields
3. Run the application using `dotnet CodeSigningAPI.dll`
4. Navigate to https://localhost:7165/swagger to see the API documentation

## How to build

1. Clone this repository
2. Open the solution in Visual Studio or JetBrains Rider
3. Rename the `appsettings.Sample.json` to `appsettings.json` and fill the required fields
4. Build the solution
5. Run the project
6. Use the API
7. (Optional) Deploy the API to a server
8. (Optional) Use a reverse proxy like Nginx or IIS
9. (Optional) Use a SSL certificate

## Configuration

### Database

The application uses PostgreSQL as the database. You can change the connection string in the `appsettings.json` file.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=CodeSigningAPI;Username=postgres;Password=postgres"
  }
}
```

### Enable Swagger

The application uses Swagger for documentation. You can enable or disable it in the `appsettings.json` file.

**Note:** Swagger is always enabled in the development environment. This setting is only for the production environment.

```json
{
  "CodeSigningAPI": {
    "EnableSwagger": true
  }
}
```

### Authorization Settings

```json
{
  "Settings": {
    "enableIPsWhitelist": true, // Enable or disable IP Whitelisting
    "allowAllPrivateIanaReservedIPs": true, // Allow all private IANA reserved IPs (class A, B and C)
    "allowClassAReservedIPs": true, // Allow Class A reserved IPs (cannot be false if allowAllPrivateIanaReservedIPs is true)
    "allowClassBReservedIPs": true, // Allow Class B reserved IPs (cannot be false if allowAllPrivateIanaReservedIPs is true)
    "allowClassCReservedIPs": true, // Allow Class C reserved IPs (cannot be false if allowAllPrivateIanaReservedIPs is true)
    "allowCGNatIPs": true, // Allow CGNAT IPs, like within a Tailscale network
    "enableAuthentication": true, // Enable or disable token authentication
    "bypassAuthenticationLoopback": false // Bypass authentication for loopback requests
  }
}
```

### Signing Settings

```json
{
  "Signing": {
    "basePath": "C:\\jsign", // Base path for the jSign binary
    "binaryName": "jsign.jar", // jSign binary name
    "keystoreType": "PIV", // Keystore type
    "keystorePassword": "<pin>", // Keystore password
    "certificateFile": "user.crt", // Certificate file
    "timestampUrl": "http://timestamp.sectigo.com", // Timestamp URL
    "algorithms": [ // Algorithms
      "SHA-1",
      "SHA-256"
    ]
  }
}
```

## Create first auth token

To be able to create the first auth token you need to either disable the authentication in the `appsettings.json` file or enable `bypassAuthenticationLoopback`.

```bash
curl --location 'https://localhost:7165/api/Settings/CreateAuthToken' \
--header 'Content-Type: application/json' \
--data '{"description":"First auth token"}'
```

Alternatively, the first auth token can be created directly in the database.

## IIS Configuration

1. Install the [.NET Core Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Create a new website
3. Set the physical path to the folder where the application is located
4. Restart the website
5. (Optional) Use a SSL certificate and enable HTTPS ([win-acme](https://www.win-acme.com))

May also be needed to increase `maxRequestEntityAllowed` and `uploadReadAheadSize` in the **Configuration Editor** at `system.webServer/serverRuntime` depending on the file size to be signed.
The same applies to `maxAllowedContentLength` at `web.config`.

## Contributing

If you want to contribute to this project and make it better, your help is very welcome. Create a pull request with your changes and I will review it.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
