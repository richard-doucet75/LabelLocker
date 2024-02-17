# LabelLocker

## Overview

LabelLocker is a .NET library designed to manage the uniqueness of strings across various contexts in a disconnected manner, independent of specific database tables or columns. Utilizing Entity Framework Core, LabelLocker simplifies enforcing string uniqueness and provides a streamlined interface for CRUD operations on label entities. This makes it an ideal solution for projects requiring efficient and flexible management of unique string identifiers, such as usernames, product codes, or any unique labels across different parts of an application or across different applications.

## Features

- **String Uniqueness Management**: Ensures the uniqueness of strings across different contexts, making it perfect for scenarios like unique username across multiple systems.
- **CRUD Operations**: Provides a simplified interface for creating, reading, updating, and deleting label entities.
- **Disconnected Operation**: Works independently of specific database tables or columns, offering flexibility in application architecture.
- **Entity Framework Core Integration**: Leverages EF Core for robust data access and manipulation, ensuring compatibility with a wide range of database providers.

## Getting Started

### Prerequisites

- .NET 5.0 SDK or later.
- An existing .NET project or the ability to create one.
- Basic knowledge of Entity Framework Core.

### Installation

To use LabelLocker in your project, add it as a dependency via NuGet:

```sh
dotnet add package LabelLocker --version <version_number>
```

Replace `<version_number>` with the desired version of LabelLocker.

### Configuration

1. **DbContext Configuration**: Ensure your project's `DbContext` is properly set up to connect to your database. LabelLocker will use this context for managing labels.

2. **Service Registration**: In your ASP.NET Core application's startup class (`Startup.cs`), register the LabelLocker services with the dependency injection container:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddLabelManagementEntityFrameworkRepositories(Configuration.GetConnectionString("YourConnectionStringName"));
    services.AddLabelManagementServices();
}
```

Replace `"YourConnectionStringName"` with the name of your connection string in `appsettings.json`.

## Usage

### Reserving a Label

To reserve a label, ensuring its uniqueness:

```csharp
var isSuccess = await labelService.ReserveLabelAsync("uniqueLabel", clientRowVersion);
```

### Releasing a Label

To release a previously reserved label:

```csharp
var isSuccess = await labelService.ReleaseLabelAsync("uniqueLabel", clientRowVersion);
```

## Contributing

Contributions to LabelLocker are welcome! Please refer to the contributing guidelines in the repository for more information on how to contribute.

## License

LabelLocker is licensed under the MIT License. See the LICENSE file in the repository for more details.