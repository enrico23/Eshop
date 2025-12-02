# 📦 EShop — Full-Stack .NET E-Commerce Demo

A clean, modern, full-stack e-commerce application showcasing backend engineering, frontend development, 
loud-native architecture, DevOps, and developer experience.

## ✨ Overview

EShop highlights professional-grade software engineering using:

- .NET for backend and orchestration
- Blazor Server for interactive UI
- SQL Server + EF Core for data access
- .NET Aspire for local service orchestration

## 🚀 Features

- Product catalog with categories and brands
- Clean domain + application layering
- EF Core with migrations and SQL Server
- Blazor UI with component library
- Orchestrated services via .NET Aspire
- Future expansion: checkout, orders, inventory

## 🏁 Quick Start

EShop uses .NET Aspire to orchestrate backend, frontend, and database with a single command.

➡️ See the full guide here: [Quick Start](docs/quickstart.md)

## 🗂️ EF Core migrations

Migrations live in the `EShop.Data` class library, while the API project provides the runtime services
and configuration needed to build the model. Use the API project as the startup assembly when
creating or applying migrations:

```bash
# from the repo root
dotnet ef migrations add <MigrationName> -p src/EShop.Data -s src/EShop.API -o Migrations

# apply the migrations to your configured database
dotnet ef database update -p src/EShop.Data -s src/EShop.API
```

This ensures EF Core can locate the `CatalogDbContext` and any related configuration while keeping
the migrations alongside the data layer.

## API

➡️ [Error Handling](docs/api/errorhandling.md)