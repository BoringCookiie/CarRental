# 🚗 CarRental - Car Rental Management System

A modern car rental management system built with .NET 10, featuring a customer-facing web application and an admin WPF desktop application.

## 📋 Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows (required for BackOffice WPF application)
- Visual Studio 2022+ or VS Code (optional)

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/BoringCookiie/CarRental.git
cd CarRental
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build
```

### 4. Run the Applications

#### FrontOffice (Customer Web App)
```bash
dotnet run --project CarRental.FrontOffice
```
Then open **https://localhost:5001** in your browser.

#### BackOffice (Admin Desktop App)
```bash
dotnet run --project CarRental.BackOffice
```

## 🏗️ Project Structure

| Project | Description |
|---------|-------------|
| `CarRental.Core` | Domain entities, enums, and interfaces |
| `CarRental.Data` | Entity Framework Core context and migrations |
| `CarRental.Services` | Business logic services |
| `CarRental.FrontOffice` | ASP.NET Core MVC web application for customers |
| `CarRental.BackOffice` | WPF desktop application for administrators |
| `CarRental.Tests` | Unit tests |

## ✨ Features

### FrontOffice (Web)
- 🔐 Client registration and authentication
- 🚙 Browse and search vehicles by brand/type
- 📅 Book vehicles with date selection
- 👤 Edit profile
- 📋 View rental history

### BackOffice (Desktop)
- 📊 Dashboard with statistics
- 🚗 Vehicle management (CRUD + images)
- 👥 Client management
- 📝 Rental management
- 💰 Prices in MAD (Moroccan Dirham)

## 🎨 Design

- Modern glassmorphism UI design
- Responsive web interface
- Material Design-inspired desktop app
- Custom background images

## 📧 Contact

Created by [@BoringCookiie](https://github.com/BoringCookiie)
