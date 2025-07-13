# PetProfiles API

A .NET 9 Web API for managing pet profiles with Azure integration, demonstrating real-world cloud development skills.

## 🚀 Features

- **CRUD Operations** for Pet Profiles
- **Azure Blob Storage** integration for image upload/download
- **Entity Framework Core** with SQL Server
- **Serilog** logging with Azure Application Insights support
- **CORS** enabled for cross-platform client access
- **RESTful API** design with proper HTTP status codes

## 🏗️ Architecture

```
PetProfiles.Api/
├── Controllers/          # API endpoints
│   ├── PetProfilesController.cs
│   └── ImagesController.cs
├── Models/              # Data models
│   └── PetProfile.cs
├── Data/                # Database context
│   └── PetProfilesDbContext.cs
├── Services/            # Business logic services
│   └── BlobStorageService.cs
└── Program.cs           # Application configuration
```

## 🛠️ Technology Stack

- **.NET 9** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API framework
- **Entity Framework Core** - ORM for database operations
- **Azure Storage Blobs** - Cloud storage for images
- **Serilog** - Structured logging
- **SQL Server** - Database (LocalDB for development)

## 📋 Prerequisites

- .NET 9 SDK
- SQL Server LocalDB (included with Visual Studio)
- Azure Storage Account (for production)

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/PetProfiles.Api.git
cd PetProfiles.Api
```

### 2. Install Dependencies

```bash
dotnet restore
```

### 3. Configure Database

The API uses LocalDB by default. Update `appsettings.json` for production:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PetProfilesDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 4. Run the Application

```bash
dotnet run
```

The API will be available at:
- **Swagger UI**: https://localhost:7001/swagger
- **API Base URL**: https://localhost:7001/api

## 📚 API Endpoints

### Pet Profiles

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/PetProfiles` | Get all pet profiles |
| GET | `/api/PetProfiles/{id}` | Get pet profile by ID |
| POST | `/api/PetProfiles` | Create new pet profile |
| PUT | `/api/PetProfiles/{id}` | Update pet profile |
| DELETE | `/api/PetProfiles/{id}` | Delete pet profile |

### Images

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Images/upload` | Upload pet image |
| GET | `/api/Images/{fileName}` | Download image |
| DELETE | `/api/Images/{fileName}` | Delete image |

## 📝 PetProfile Model

```json
{
  "id": 1,
  "name": "Buddy",
  "breed": "Golden Retriever",
  "age": 3,
  "imageUrl": "https://storage.blob.core.windows.net/pet-images/guid.jpg",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

## 🔧 Configuration

### Development Settings

The API is configured for local development with:
- LocalDB for database
- Azure Storage Emulator for blob storage
- Console logging

### Production Settings

For production deployment, update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-azure-sql-server;Database=PetProfilesDb;User Id=username;Password=password;"
  },
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=your-storage-account;AccountKey=your-key;EndpointSuffix=core.windows.net",
    "ContainerName": "pet-images"
  },
  "ApplicationInsights": {
    "ConnectionString": "your-app-insights-connection-string"
  }
}
```

## 🧪 Testing

### Using Swagger UI

1. Navigate to https://localhost:7001/swagger
2. Test endpoints directly from the browser

### Using curl

```bash
# Get all pet profiles
curl -X GET "https://localhost:7001/api/PetProfiles"

# Create a pet profile
curl -X POST "https://localhost:7001/api/PetProfiles" \
  -H "Content-Type: application/json" \
  -d '{"name":"Buddy","breed":"Golden Retriever","age":3}'

# Upload an image
curl -X POST "https://localhost:7001/api/Images/upload" \
  -F "file=@pet-image.jpg"
```

## 📊 Logging

The API uses Serilog for structured logging:

- **Console**: Development logging
- **Application Insights**: Production monitoring (when configured)

Log levels:
- Information: Normal operations
- Warning: Potential issues
- Error: Exceptions and failures

## 🔒 Security

Currently implements basic API key authentication (to be enhanced):
- API key validation via header
- CORS policy for cross-origin requests

## 🚀 Deployment

### Azure App Service

1. Create an Azure App Service
2. Configure connection strings in Application Settings
3. Deploy using GitHub Actions or Azure CLI

### Docker

```bash
# Build image
docker build -t petprofiles-api .

# Run container
docker run -p 8080:80 petprofiles-api
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

## 🆘 Support

For questions or issues:
- Create an issue on GitHub
- Check the documentation
- Review the API logs

---

**Next Steps**: This API is ready to be consumed by a .NET MAUI frontend application! 