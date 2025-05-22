# Carvisto – Carpooling Platform
![Language](https://img.shields.io/badge/Language-C%23-brightgreen?style=for-the-badge&logo=sharp&logoColor=813787&color=813787&labelColor=FCFCFC)
![Framework](https://img.shields.io/badge/Framework-.NET-brightgreen?style=for-the-badge&logo=dotnet&logoColor=813787&color=813787&labelColor=FCFCFC)
![Size](https://img.shields.io/github/repo-size/Akineyshen/Carvisto.NET?label=Size&style=for-the-badge&color=813787&labelColor=FCFCFC)
![Last Commit](https://img.shields.io/github/last-commit/Akineyshen/Carvisto.NET?label=Last%20Commit&style=for-the-badge&color=813787&labelColor=FCFCFC)

## Features
### Core Functionality
- **User Authentication**: User registration, login, and profile management.
- **Profile Avatar:** Users can upload and set a profile avatar to personalize their account and enhance visibility.
- **Trip Creation**: Drivers can publish trips by specifying route, date, and price.
- **Trip Search**: Passengers can search for available trips by route, date, and cost.
- **Driver Search:** Passengers can find drivers by phone number, name, or email.
- **Password Change:** Users can securely update their password in the account settings.
- **Booking trip:** Passengers can book seats on the selected trip with confirmation of the reservation.
- **Booking History:** Users can view a detailed history of their past trips, including dates, routes, and booking details, for easy reference.
- **Automatic adjustment of reservations:** When the total number of seats decreases, the system automatically cancels the last reservations.
- **View Passenger List:** Drivers can see a list of passengers for their trips.
- **FAQ Page:** A dedicated FAQ page provides answers to common user questions, improving support and platform usability.

### Advanced Features
- **Driver Reviews and Ratings:** Passengers can leave feedback and rate drivers to provide feedback and build trust.
- **PDF Documents**: Automatic generation of trip agreements in PDF format.
- **Interactive Map**: Visual representation of trip routes using Google Maps API.
- **Moderator Profile:** A dedicated moderator profile with administrative privileges to manage users, trips, reviews, and platform content.

## Requirements
- .NET 8.0 or later
- SQLITE database
- Entity Framework Core
- Google Maps API key
- DinkToPdf

## Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/Akineyshen/Carvisto.NET.git
   ```
2. Navigate to the project directory:
   ```bash
    cd Carvisto.NET
    ```
3. Apply database migrations
    ```bash
    dotnet ef database update
    ```
4. Run the application:
   ```bash
   dotnet run
   ```

## Project Structure
```bash
Carvisto/
    Carvisto/
    ├── Dependencies/                # Project dependencies
    ├── Properties/                  # Project configuration (launchSettings.json)
    ├── wwwroot/                     # Static files (CSS, JS, images)
    ├── Carvisto.Tests/              # Unit and integration tests for the application
    ├── Controllers/                 # MVC controllers handling HTTP requests
    ├── Data/                        # Database context and seed data
    ├── Migrations/                  # Entity Framework database migrations
    ├── Models/                      # Data models representing application entities
    ├── Services/                    # Application services and business logic
    ├── Views/                       # Razor views (UI templates)
    ├── .gitignore                   # Git ignore rules
    ├── appsettings.json             # Application configuration
    ├── appsettings.Development.json # Development-specific settings
    ├── appsettings.Production.json  # Production-specific settings
    ├── Carvisto.csproj              # Project file defining dependencies and settings
    ├── Carvisto.sin.DotSettings     # ReSharper or IDE-specific settings (optional)
    ├── Program.cs                   # Main entry point of the application
    └── ReadMe.md                    # Project documentation
```

## Test Coverage
Tests are implemented exclusively for the `Services` folder within the `Carvisto.Tests/` project. This focus is intentional, as services contain the primary business logic and core functionality of the application, making them the priority for testing.

### Coverage Details
<img src="https://imgur.com/kwsX6IQ.png" alt="Tests">

**Coverage Summary**: The majority of services achieve test coverage above 80%, ensuring robust testing of the application’s core functionality.

## Screenshots
<img src="https://imgur.com/2RIUQWs.png" alt="Main Page">

<img src="https://imgur.com/51zYq93.png" alt="Trips Search">

<img src="https://imgur.com/unPiLFj.png" alt="Rewiews Driver">

<img src="https://imgur.com/re60CAl.png" alt="Trip Details">

<img src="https://imgur.com/lnz5eCg.png" alt="Manage Bookings">
