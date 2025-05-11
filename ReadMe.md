# Carvisto – Carpooling Platform
![Language](https://img.shields.io/badge/Language-C%23-brightgreen?style=for-the-badge&logo=sharp&logoColor=813787&color=813787&labelColor=FCFCFC)
![Framework](https://img.shields.io/badge/Framework-.NET-brightgreen?style=for-the-badge&logo=dotnet&logoColor=813787&color=813787&labelColor=FCFCFC)
![Size](https://img.shields.io/github/repo-size/Akineyshen/Carvisto.NET?label=Size&style=for-the-badge&color=813787&labelColor=FCFCFC)
![Last Commit](https://img.shields.io/github/last-commit/Akineyshen/Carvisto.NET?label=Last%20Commit&style=for-the-badge&color=813787&labelColor=FCFCFC)

## Features
### Core Functionality
- **User Authentication**: User registration, login, and profile management.
- **Trip Creation**: Drivers can publish trips by specifying route, date, and price.
- **Trip Search**: Passengers can search for available trips by route, date, and cost.
- **Driver Search:** Passengers can find drivers by phone number, name, or email.
- **Reviews and Comments**: Users can leave reviews and comments after a completed trip.
- **Password Change:** Users can securely update their password in the account settings.
- **Booking trip:** Passengers can book seats on the selected trip with confirmation of the reservation.

### Advanced Features
- **PDF Documents**: Automatic generation of trip agreements in PDF format.
- **User Ratings**: Rating system based on user feedback.
- **Interactive Map**: Visual representation of trip routes using Google Maps API.

## Requirements
- .NET 6.0 or later
- SQLITE database

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
    ├── Controllers/                 # MVC controllers handling HTTP requests
    ├── Data/                        # Database context and seed data
    ├── Migrations/                  # Entity Framework database migrations
    ├── Models/                      # Data models representing application entities
    ├── Views/                       # Razor views (UI templates)
    ├── .gitignore                   # Git ignore rules
    ├── appsettings.json             # Application configuration
    ├── appsettings.Development.json # Development-specific settings
    ├── Carvisto.db                  # SQLite database file
    ├── Program.cs                   # Main entry point of the application
    └── ReadMe.md                    # Project documentation
```

## Screenshots

to be added later
