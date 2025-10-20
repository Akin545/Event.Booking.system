Event Booking System

The Event Booking System is a multi-layered C#/.NET application that allows users to register, browse events, and book tickets.
It features user authentication, role-based access (Admin/User), event management, ticket tracking, and a waiting list mechanism for full events.

Tech Stack		
Layer				Technology
Language			C# (.NET 8 / .NET 9)
Framework			ASP.NET Core Web API
Data Access			Entity Framework Core
Database			SQL Server
Authentication		JWT (JSON Web Token)
Testing				xUnit + Moq
Logging				Microsoft.Extensions.Logging
Architecture		Layered (Presentation,Business,Repository)

Setup & Running Instructions

Clone the repository
git clone https://github.com/Akin545/Event.Booking.system
cd EventBookingSystem

Configure the database
Edit the appsettings.json file in the API project to set your SQL Server connection string:
"ConnectionStrings": {
  "Sql": "Server=localhost;Database=EventDb;Trusted_Connection=True;TrustServerCertificate=True;"
}

Apply migrations
Use the .NET CLI to create and update the database:
cd Event.Booking.System.Repository
Update-Database

A default admin has been seeded to start with the application. check the AppDbcontext for details or the database

If you get a mismatch error, ensure the target project is Event.Booking.System.Repository.

Run the API
cd ../Event.Booking.System
dotnet run

Design Choices
Layered Architecture
The system is split into:

Presentation Layer (API): Handles HTTP requests and responses.

Business Logic Layer: Implements validation, business rules, and service orchestration.

Repository Layer: Manages database operations via EF Core.
This separation improves testability, maintainability, and scalability.

Dependency Injection
Services and repositories are injected using .NET’s built-in DI container.

Makes unit testing simpler (e.g., using Moq to mock dependencies).

Test-Driven Development (TDD
Unit tests were written using xUnit and Moq to validate:

Event creation and validation

Booking creation logic

Waiting list entry when tickets are full

Role-based permission enforcement (Admin/User)

Error Handling
Custom exception classes (e.g. EventException, BookingException) provide clear error messages.

Logging is centralized using ILogger<T> for consistent error tracking.


JWT Authentication
Secure access control using JSON Web Tokens.

Protects event management endpoints from unauthorized users.

Extensibility
The system was designed for future scalability:

Can integrate with email/SMS notifications.

Can easily support payment gateways.

Can scale to microservices or Azure-based architecture later.

Author
Akinfosile Sola
solaakinfosile@gmail.com
https://www.linkedin.com/public-profile/settings?trk=d_flagship3_profile_self_view_public_profile