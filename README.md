📦 EShop — Full-Stack .NET E-Commerce Demo

A clean, modern, full-stack e-commerce application designed to showcase backend engineering, frontend development, cloud-native architecture, DevOps practice, and developer-experience excellence.

✨ Overview

EShop is a demonstration project built to highlight professional-grade software engineering using:

.NET for backend and orchestration

Blazor Server for modern, interactive UI

SQL Server + EF Core for data access

.NET Aspire for local service orchestration and developer experience

This project is part of my professional portfolio on GitHub and LinkedIn, showcasing clean architecture, maintainability, reliability, and modern .NET practices.

🎯 Goals

EShop aims to demonstrate:

Clean Architectural Foundation
Domain-driven structure with clear separation of concerns.

Realistic Backend API
ASP.NET Core following industry-standard patterns.

Modern Frontend
Blazor Server + component library (MudBlazor).

Database Integration
SQL Server, EF Core, and automated migrations.

Developer Experience Excellence
.NET Aspire orchestrating all services with a single command.

Production-minded Practices
Clear boundaries, testability, modularity, scalability.

🚀 Features

Product catalog with categories and brands

Clean domain + application layering

EF Core with migrations and SQL Server

Blazor UI with component library

Orchestrated services via .NET Aspire

Future expansion for checkout, orders, and inventory

🏁 Getting Started

EShop uses .NET Aspire to run and orchestrate all services locally.
With Aspire, the backend, frontend, and (soon) database/migrations) start together with a single command.

✔ Prerequisites

Before running the project, ensure the following are installed:

1. .NET SDK 9.0+

Backend services and Aspire target .NET 10

Blazor Server targets .NET 9 due to MudBlazor compatibility

Download:
https://dotnet.microsoft.com/download

2. .NET Aspire Workloads

Install Aspire:

dotnet workload update
dotnet workload install aspire


More info:
https://aspire.dev/get-started/prerequisites/

3. Container Runtime (required for SQL Server + Aspire resources)

You can use either Docker Desktop or Rancher Desktop.

🟦 Option 1 — Docker Desktop

Recommended by Microsoft.

✔ Easiest setup
✔ Strong integration with .NET
✔ Great tooling support

⚠️ Licensing restrictions apply for commercial use.
Use it only if it fits your license model.

Download:
https://www.docker.com/products/docker-desktop/

🟩 Option 2 — Rancher Desktop (my personal choice)

A fully open-source alternative with no license constraints.

✔ Free & open-source
✔ No usage restrictions
✔ Works seamlessly with .NET Aspire
✔ Supports both containerd and dockerd

Download:
https://rancherdesktop.io/

Recommended settings (if using Rancher Desktop):

Runtime: dockerd

Kubernetes: off (not required)

WSL2 enabled (on Windows)


![Aspire Dashboard](docs/images/dashboard.jpg)