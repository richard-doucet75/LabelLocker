# LabelLocker

## Overview

LabelLocker is a .NET library crafted to manage the uniqueness of strings across various applications or contexts in a disconnected architecture, without direct dependency on specific database tables or columns. Utilizing the robustness of Entity Framework Core, it offers an efficient way to ensure string uniqueness (e.g., usernames, product codes) and simplifies CRUD operations on label entities. LabelLocker is perfect for projects that demand flexible and reliable management of unique identifiers.

## Features

- **String Uniqueness Management**: Guarantees string uniqueness across different applications or parts of an application.
- **CRUD Operations**: Streamlines creating, reading, updating, and deleting label entities through a simplified interface.
- **Disconnected Operation**: Functions independently from specific database structures, offering architectural flexibility.
- **Entity Framework Core Support**: Built on EF Core to provide strong data access capabilities and support for various database providers.

## Getting Started

### Prerequisites

- .NET 5.0 SDK or newer.
- An existing .NET project or the setup for a new one.
- Familiarity with Entity Framework Core is beneficial.

### Installation

Add LabelLocker as a dependency to your project using NuGet:

```bash
dotnet add package LabelLocker --version <version_number>
```

Ensure to replace `<version_number>` with the actual version you intend to use.

### Configuration

1. **Configure DbContext**: Make sure your project's `DbContext` is configured to connect to your database. LabelLocker will utilize this context.

2. **Register Services**: In the `Startup.cs` of your ASP.NET Core application, register LabelLocker services:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddLabelManagementEntityFrameworkRepositories(Configuration.GetConnectionString("YourConnectionString"));
    services.AddLabelManagementServices();
}
```

Change `"YourConnectionString"` to your actual database connection string defined in `appsettings.json`.

## Usage

### Reserve a Label

To ensure a label's uniqueness:

```csharp
var isSuccess = await labelService.ReserveLabelAsync("myUniqueLabel", clientRowVersion);
```

### Release a Label

To make a reserved label available again:

```csharp
var isSuccess = await labelService.ReleaseLabelAsync("myUniqueLabel", clientRowVersion);
```

## Contributing

We warmly welcome contributions to the LabelLocker project. Whether it's bug reports, feature requests, or code contributions, please visit [our GitHub repository](https://github.com/richard-doucet75/LabelLocker) to get started. See the CONTRIBUTING.md file for more details on how to contribute.

## License

LabelLocker is made available under the MIT License. For more details, see the [LICENSE](https://github.com/richard-doucet75/LabelLocker/blob/main/LICENSE) file in the repository.