# CareerHub API

This is the foundational backend engine for the CareerHub platform, built using **.NET 10** and native OpenAPI tooling.

## Architectural Choice: Minimal APIs

For this project, **Minimal APIs** were chosen over traditional Controllers.

### Justification
Minimal APIs drastically reduce boilerplate code, offer superior performance, and allow routes to be mapped explicitly and cleanly directly within the configuration pipeline. This approach aligns well with modern, lightweight microservice architectures.

---

##  Prerequisites

To run this project locally, you need to have the following installed on your system:
* **.NET 10.0 SDK** (Software Development Kit) or higher
* **Visual Studio Code** (or Visual Studio 2022)
* **C# Dev Kit Extension** for VS Code 

---
# How to Run the Project

1. Open your terminal (PowerShell, Command Prompt, or VS Code Integrated Terminal).

2. Navigate to the project root directory:

```bash
cd CareerHub.Api
```

3. Compile and run the application:

```bash
dotnet run
```

4. Look for output similar to this in the terminal:

```plaintext
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5011
```

---

# Testing the API Endpoints

Use the following URLs in your browser or API testing tool:

## Get All Jobs

```plaintext
http://localhost:5011/jobs
```

## Get Job by ID (Existing)

```plaintext
http://localhost:5011/jobs/1
```

## Get Job by ID (Non-Existing)

```plaintext
http://localhost:5011/jobs/99
```

---

# Scalar API Dashboard

Access the Scalar API documentation dashboard here:

```plaintext
http://localhost:5011/scalar/v1

```

## Assignment 1.2 :API Design Decisions

### 1. PostedAt Field Placement
The PostedAt timestamp is set automatically by the server at the exact time a job is stored to preserve data integrity and audit logs, so it belongs in the JobResponse for clients to see, but must never be in CreateJobRequest to prevent users from forging posting timelines.