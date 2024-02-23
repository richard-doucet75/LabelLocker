# LabelLocker

LabelLocker is a .NET library designed to manage and ensure the uniqueness of labels or identifiers across different entities in your applications. With built-in support for Entity Framework Core (EFCore) and an abstraction layer for custom repository implementations, LabelLocker is highly adaptable to various data storage strategies. It also implements optimistic concurrency control to manage concurrent operations on labels efficiently.

## Key Features

- **Unique Label Management**: Safely reserve and release labels to ensure uniqueness across entities.
- **EFCore Integration**: Leverage Entity Framework Core for seamless integration with relational databases.
- **Custom Repository Support**: Extend LabelLocker with custom repositories to fit any data access strategy.
- **Concurrency Handling**: Manage concurrent operations with built-in optimistic concurrency control.
- **Case Insensitivity**: Label names are treated case-insensitively, ensuring "LABEL" and "label" are considered the same.

## Getting Started

### Installation

```bash
dotnet add package LabelLocker
```

### Configuration

1. **EFCore Setup**: Ensure EFCore is configured in your project to use LabelLocker with your database of choice.
2. **Integrate LabelLocker**: Add the `LabelEntity` to your `DbContext`:

```csharp
public DbSet<LabelEntity> Labels { get; set; }
```

3. **Database Migration**: Apply migrations to create the necessary table for label management:

```bash
dotnet ef migrations add AddLabelEntities
dotnet ef database update
```

## Usage

Reserve a label:

```csharp
var reservationResult = await labelService.ReserveLabelAsync("MyUniqueLabel");
if (reservationResult.Success)
{
    var token = reservationResult.ReservationToken;
    // Handle success
}
```

Release a label:

```csharp
var releaseResult = await labelService.ReleaseLabelAsync("MyUniqueLabel", token);
if (releaseResult.Success)
{
    // Label released successfully
}
```

## Custom Repository Implementation

Create a custom repository by implementing the `ILabelRepository` interface if you prefer using a different ORM or data access strategy than EFCore.

```csharp
public class CustomLabelRepository : ILabelRepository
{
    // Custom implementation
}
```

## Contributing

Contributions are welcome! Feel free to fork the repository, submit pull requests, or report issues.

## License

LabelLocker is distributed under the MIT License. See the [LICENSE](LICENSE) file for more details.