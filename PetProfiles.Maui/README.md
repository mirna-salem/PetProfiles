# Pet Profiles MAUI App

A cross-platform .NET MAUI application that demonstrates real-world cloud development skills by connecting to an Azure-hosted backend API.

## Features

- **Cross-platform**: Runs on Android, iOS, Windows, and macOS
- **Modern UI**: Beautiful, responsive design with dark/light theme support
- **Cloud Integration**: Connects to Azure-hosted backend API
- **CRUD Operations**: Create, read, update, and delete pet profiles
- **MVVM Architecture**: Clean separation of concerns
- **Dependency Injection**: Proper service registration and management

## Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 17.8+ or Visual Studio Code
- For Android development: Android SDK
- For iOS development: Xcode (macOS only)
- For Windows development: Windows 10/11

## Setup Instructions

### 1. Clone and Navigate to Project

```bash
cd PetProfiles.Maui
```

### 2. Configure API Settings

Before running the app, you need to set up the API configuration:

1. **Copy the example configuration:**
   ```bash
   cp Services/ApiConfiguration.Example.cs Services/ApiConfiguration.cs
   ```

2. **Edit the configuration file** `Services/ApiConfiguration.cs` and uncomment the code, then update with your actual values:
   ```csharp
   public const string DevelopmentApiKey = "your-actual-api-key";
   public const string ProductionBaseUrl = "https://your-api-url.azurewebsites.net/api";
   public const string ProductionApiKey = "your-azure-api-key";
   ```

**Note:** The `ApiConfiguration.cs` file is excluded from Git for security. Only the example file is committed.

### 3. Build and Run

```bash
# Restore packages
dotnet restore

# Build the project
dotnet build

# Run on Windows
dotnet run -f net8.0-windows10.0.19041.0

# Run on Android (requires Android SDK)
dotnet run -f net8.0-android

# Run on iOS (macOS only, requires Xcode)
dotnet run -f net8.0-ios
```

## Project Structure

```
PetProfiles.Maui/
├── Models/
│   └── PetProfile.cs              # Data model with property change notifications
├── ViewModels/
│   ├── BaseViewModel.cs           # Base class for all ViewModels
│   └── PetProfilesViewModel.cs    # Main ViewModel for pet profiles
├── Views/
│   └── PetProfilesPage.xaml       # Main UI page
├── Services/
│   ├── IPetProfilesService.cs     # Service interface
│   └── PetProfilesService.cs      # HTTP service implementation
└── Resources/
    └── Images/                    # App images and icons
```

## Architecture

This app follows the **MVVM (Model-View-ViewModel)** pattern:

- **Models**: Data entities with property change notifications
- **Views**: XAML pages that display the UI
- **ViewModels**: Business logic and data binding
- **Services**: HTTP communication with the backend API

## Key Components

### PetProfile Model
- Implements `INotifyPropertyChanged` for data binding
- Matches the backend API model structure
- Supports real-time UI updates

### PetProfilesService
- HTTP client for API communication
- Handles CRUD operations
- Includes error handling and logging
- Uses API key authentication

### PetProfilesViewModel
- Manages the list of pet profiles
- Handles user interactions (add, delete, refresh)
- Provides commands for UI binding
- Implements loading states

### PetProfilesPage
- Modern, responsive UI design
- Supports both light and dark themes
- Pull-to-refresh functionality
- Empty state handling
- Loading indicators

## API Integration

The app connects to your Azure-hosted backend API with the following endpoints:

- `GET /api/PetProfiles` - Get all pet profiles
- `GET /api/PetProfiles/{id}` - Get specific pet profile
- `POST /api/PetProfiles` - Create new pet profile
- `PUT /api/PetProfiles/{id}` - Update pet profile
- `DELETE /api/PetProfiles/{id}` - Delete pet profile

## Customization

### Adding New Features

1. **New Pages**: Create XAML files in the `Views/` folder
2. **New ViewModels**: Extend `BaseViewModel` in the `ViewModels/` folder
3. **New Services**: Implement interfaces in the `Services/` folder
4. **New Models**: Add to the `Models/` folder

### Styling

The app uses MAUI's built-in theming system:
- Light/dark theme support
- Consistent color scheme
- Responsive design patterns

## Troubleshooting

### Common Issues

1. **API Connection Errors**: Verify your API URL and key in `PetProfilesService.cs`
2. **Build Errors**: Ensure you have the correct .NET SDK version
3. **Platform-specific Issues**: Check platform-specific prerequisites

### Debugging

- Use `System.Diagnostics.Debug.WriteLine()` for logging
- Check the Output window in Visual Studio for detailed error messages
- Use the MAUI Hot Reload feature for rapid development

## Next Steps

To enhance this project, consider adding:

1. **Image Upload**: Implement photo capture and upload to Azure Blob Storage
2. **Offline Support**: Add local caching with SQLite
3. **Authentication**: Implement user login/logout
4. **Push Notifications**: Add Azure Notification Hubs integration
5. **Analytics**: Integrate Azure Application Insights
6. **CI/CD**: Set up GitHub Actions for automated deployment

## Attributions

### Icons and Graphics
- **Paw Icon**: [Paw icons created by Maan Icons - Flaticon](https://www.flaticon.com/free-icons/paw)

## Icon Attribution

<a href="https://www.freepik.com/icon/circle_13759398#fromView=search&page=1&position=10&uuid=bc896c21-750f-4893-ab6e-d13e709ab5a1">Icon by manshagraphics</a>
<a href="https://www.freepik.com/icon/paws_11810073#fromView=search&page=1&position=56&uuid=9eccb77b-5415-4c9a-8f6f-ba73f7dbb808">Icon by fancykeith</a>
<a href="https://www.freepik.com/icon/flower_346167#fromView=search&page=1&position=4&uuid=c75a6d23-0acd-4631-8874-fc28c256e9b1">Icon by Freepik</a>
<a href="https://www.flaticon.com/free-icons/sync" title="sync icons">Sync icons created by graphicmall - Flaticon</a>
<a href="https://www.flaticon.com/free-icons/read-more" title="read more icons">Read more icons created by Fathema Khanom - Flaticon</a>
<a target="_blank" href="https://icons8.com/icon/QADawoQS3Rcg/plus-math">Plus</a> icon by <a target="_blank" href="https://icons8.com">Icons8</a>

## Contributing

This is a demonstration project showcasing cloud development skills. Feel free to fork and enhance it for your portfolio!

## License

This project is for educational and portfolio purposes. 