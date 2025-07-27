# PetProfiles

A full-stack .NET application for managing pet profiles. Built with .NET MAUI for the frontend and .NET 9 Web API for the backend, deployed to Azure.

## What it does

This is a pet profile management system where you can add, view, and manage pet information. The app works on mobile devices (iOS, Android) and desktop (Windows, macOS).

## Tech stack

- **Frontend**: .NET MAUI with MVVM pattern
- **Backend**: ASP.NET Core 9 Web API with Entity Framework Core
- **Database**: Azure SQL Database
- **Storage**: Azure Blob Storage for images
- **Cache**: In-memory caching with ConcurrentDictionary
- **Deployment**: Azure App Service
- **CI/CD**: GitHub Actions

## Features

- Add and manage pet profiles with photos
- Cross-platform mobile app
- RESTful API with authentication
- Cloud storage for images
- Dark/light theme support
- Real-time data sync

## Live demo

- API: https://petprofiles-api-ms-ddhsemepgwdahxcm.canadacentral-01.azurewebsites.net/
- API docs: https://petprofiles-api-ms-ddhsemepgwdahxcm.canadacentral-01.azurewebsites.net/swagger

## Getting started

```bash
# Clone the repo
git clone https://github.com/yourusername/PetProfiles.git
cd PetProfiles

# Run the MAUI app (API is already hosted on Azure)
cd PetProfiles.Maui
dotnet run
```

## Project structure

```
PetProfiles/
├── PetProfiles.Api/          # Web API backend
│   ├── Controllers/          # API endpoints
│   ├── Services/             # Business logic
│   ├── Models/               # Data models
│   └── Data/                 # Database context
├── PetProfiles.Maui/         # Mobile app
│   ├── Views/                # UI pages
│   ├── ViewModels/           # MVVM view models
│   └── Services/             # API client
└── .github/workflows/        # CI/CD
```

## What I learned

This project helped me understand:
- Building cross-platform apps with .NET MAUI
- Creating REST APIs with ASP.NET Core
- Azure cloud services integration
- CI/CD pipelines with GitHub Actions
- Clean architecture and SOLID principles

## Future plans

I'm planning to expand this into a full pet health tracker. Eventually I want to add:
- User authentication and sign-in
- Health tracking features (vaccinations, vet visits, medications)
- Weight and activity monitoring
- Heart rate monitoring for dogs
- Reminders and notifications
- Vet appointment scheduling

## Attributions

Sun and moon icons from Flaticon 