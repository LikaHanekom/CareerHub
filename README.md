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

### 2. Salary Cross-Field Validation Approach
To enforce the business rule that SalaryMax must be greater than SalaryMin when both are provided, we implemented the IValidatableObject interface directly inside our request DTOs (CreateJobRequest and UpdateJobRequest). 

**Why this approach was chosen:**
 - Keeps Controllers Clean: It prevents validation logic from cluttering our controller actions, adhering to the Single Responsibility.
 - Fails Fast: This framework immediately intercepts invalid payloads and rejects them with a 400 Bad Request problem details reponse, before it can get to any controller or service code.



### 3. PUT Status Code Choice: 
I decided to return a 200 OK status accompanied by the fully updated JobResponse in the response body, rather than a silent 204 No Content.

**Why this is the right call:**
* **Frontend Efficiency:** Returning the updated resource means the React frontend would immediately receive the new state. It can update its local UI state instantly without being forced to execute a secondary GET /jobs/{id} request, to fetch the changes.Because our API generates a dynamic, read-only SalaryDisplay string on the fly during mapping, returning a 200 OK with the body allows the client to instantly see and render the newly formatted string directly after an update.

### 4. DELETE Behavior for a Missing ID:

When a client attempts to delete a job ID that does not exist in the database, the API returns a 404 Not Found Problem Details payload instead of a generic 204 No Content.

**Why this is the right call:**
While some REST patterns argue that a delete on a missing item has a "successful" outcome, a job board is highly dynamic. If an ID is missing, it means the client is operating on old data. Throwing a 404 immediately alerts the frontend application or API consumer if they have a routing bug or if another administrator already deleted that exact job listing a few seconds prior eliminating any silent failures

## Assignment 1.3 :
**Controller thinning:**
Before, controllers could get really messy, with each method checking if(job == null) and then manually returning an HTTP action result. This caused a lot of duplicate code and made the controller too tightly coupled to the web layer. By refactoring the code to throw custom domain exceptions like JobNotFoundException, the business logic is successfully decoupled from the transport layer. The controller endpoints are thinned out so they only have to focus on the good ath. Meanwhile, a centralized middleware pipeline using .NET 10's IExceptionHandler acts as a global safety net, automatically catching these exceptions and translating them into uniform, RFC 7807-compliant Problem Details JSON payloads

**Structured Logging:**
Relying on standard Console.WriteLine string concatenation makes debugging production errors incredibly difficult because it only outputs flat, unstructured text strings. To a server, a flat text log is just a random sequence of characters, meaning you have to manually read through thousands of lines of messy text to find a bug. Integrating Serilog solves this by introducing structured logging. Instead of smashing variables into a plain string, Serilog captures contextual data as distinct properties and outputs them as a clean JSON object.You can run instant database-style searches to filter by specific status codes or error types. Plus, using Serilog's Log.CloseAndFlush() guarantees that if the application suffers a fatal crash during startup, the diagnostic data is fully saved before the process shuts down.