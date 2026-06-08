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

## Assignment 1.4 :
**1. Stateless Auth:**
JWT-based is lmost like having a digital passport, everytime the user does something the server checks the signature and if your signature is valid it lets you in. Session-based on the other hand is when the server keeps a file about the user in its memory and gives the user a ticket number. Everytime the user does something the server stops and looks at the file in memory. 

For scaling, problems can arrise in sessions. For example if the API gets duplicated accross different servers. If you log into server 1, but then your next click takes you to server 2, you will immediately be logged out. JWT, has this covered, becasue all three servers would share the secret key. 

**2. 401 VS 403:**
401 Unauthorized is when a user either has no token or the token they have is invalid. This happens in UseAuthentication() in the middleware pipeline. On the other hand, 403 Forbidden is given when a users token is checked and they don't have permission. This happens in the UseAuthorization() in the middleware pipeline.

**3. Token Storage:**
LocalStorage has not built-in security barriers against Javascript and anything stored there are public to any script running on the site. A hacker could easily run a javaScript Code and steal a users JWT.

Alternatively HttpOnly Cookies needs to be used for safe storage. Browser JavaScript is physically blokced from touching or reading the cookie. Your frontend never handles the token manually, making it very secure.

## Assignment 2.1 :
**1. Change Tracker:**
 The change tracker sits between the database and code. When a user pulls data out of the database the change tracker watches it and should the user make changes to the data it tracks it in it's memory without informing the database. Calling the SaveChangesAsync() at the end ensures that all network requests are send to the database once. All edits are bundled into one efficient trip to the database. This helps improve performance.

 **2. Mitigations:**
 Mitigations is like git for your database, keeping version control for your database schema. A change is written down in a migration file. If a userName property is added too your code it also needs to be added to your database. Should someone pull code without the mitigation, the C# queries will essentially be writing to tables and colums that dont exist, which will cause SQL exceptions.

 **3. Connection String Security:**
 The connection string is the master key to the database. appsettings.json is the global faalback file whereas the asppsettings.Development.json is strictly for your local machine. By placing local connection strings in your development file, will ensure that production credentials arent accidentally leaked. 
 In production, no production connection strings should be hardcoded. If it is leakekd this could compromise the integrity of your data and compromises the database. Secure alternatives such as Environment Variebales should rather be used on your hosting platform. This helps to keep the key out of any code bases.

 ## Assignment 2.2:
**Question 1:**
Company - Joblistings is a one-many relationship

Company can have many joblistings, but each job listing can only belong to one company.

Joblistings - Applications is a one-many relationship
One joblisting can have manu applications, but a single application can only have one job listing.

Applicant - Applications  is a one-many relationship
An applicant can make many applications, but one application can only have one applicant.

**Question 2**
Application needs to contain business logic, such as staus and submitted_at, because an Application needs more additional information the table should rather be modeled as a full entity and not a hidden join table.

**Question 3**
While Joblistings exist, a company cannot be deleted. The company is the one making the joblistings. Should a company be deleted the joblistings should be removed alongside it. A joblisting without a company does not have a purpose. No company will be infored should someone apply. The joblisting will have no purpose.

**Relationship Design Decisions**
Company - Joblistings I implemented DeleteBehavior.Restrict between the Company and Joblistings relationship. This ensures that a Company cannot be deleted if it still has active associated joblistings connected to it. This ensures that there will be no orphan Job Listings, forcing the system to delete Joblistings before a company can be removed. 

**The N+1 Problem**
Before implementing the eager loading the terminal output showed a single SELECT to fetch the list of job listings. Followed by N subsequent Select queries to fetch the company name for each record. After implementing the projection the Single SELECT query utilized an Inner join to retrieve both the Job and Company data in one trip.

This N+ 1 problem can deceive developers as they perform good on small datasets, but in a production environment will result in increased database loading, high latency and can ever crash the service. 

**Read vs Write**
With change tracking, EF core is tracking each object that is retrieved and maintains a single snapshot of the entity, which in turn allows the context to automatically detect changes. On the other hand without change tracking EF core does not maintain a snapshot, which reduces memory usage and CPU overhead. This happens due to the context not needing to evaluate the snapshots with previous ones. 

If you treat a photo (NoTracking) like a live document (Tracking), you'll make changes that never actually get saved to the database, and the system won't warn you.

 ## Assignment 2.3:
 **Boundary Decision**
 I'm choosing a One Repository per Entity approach, for example, IJobListingRepository, IApplicantRepository and IApplicationRepository. When the ApplicationService need to check if JobListings exists, it will inject IJobListingRepository alongside its own repository. This will ensure the clean seperation of concerns

  **Return Types**
  Returning IQueryable<T> fro the repository layer breaks abstraction as it leaves the database execution defered. It forces the Service layer to import Misrosoft.EntityFrameworkCore to resolve. ToListAsyc() or FirstOrDefaultAsync(). This completely defeats the purpose of the repository layer.

 **Lifetime Choices**
 CareerHubContext: should be scoped, otherwise it could cause multiple parallel API requests to run that will cross threads on the same context instance, which will cause runtime crashes.

 JobListingService: Scoped: to match the request pipeline scope

 ApplicationRepository: Scoped: it neess to be injected into the Scoped Careerhub.

 ApplicationStatusCache: Singleton: Hardcoded rules that won't change based in database transactions.

  **Status Transitions**
  The service layer should enforce this. Controllers cannot own this as background and other entrypoints might update statusees outside of HTTP. Repositories jobs are data access and not to dictate the core corporate workflow behavior. Therefore they should not own it.


  **Question 5: Deliberate crash**
  ***Observation of Error***
  I deliberately changed IApplicantService from a Scoped to a Singleton. IApplicantRepository I left as Scoped lifetime. When I tried running the application, it immediately refused to boot up and threw the following error: 

  System.AggregateException: Some services are not able to be constructed (Error while validating the service descriptor 'ServiceType: CareerHub.Api.Services.IApplicantService Lifetime: Singleton ImplementationType: CareerHub.Api.Services.ApplicantService': Cannot consume scoped service 'CareerHub.Api.Repositories.IApplicantRepository' from singleton 'CareerHub.Api.Services.IApplicantService'.)
 ---> System.InvalidOperationException: Error while validating the service descriptor 'ServiceType: CareerHub.Api.Services.IApplicantService Lifetime: Singleton ImplementationType: CareerHub.Api.Services.ApplicantService': Cannot consume scoped service 'CareerHub.Api.Repositories.IApplicantRepository' from singleton 'CareerHub.Api.Services.IApplicantService'.
 ---> System.InvalidOperationException: Cannot consume scoped service 'CareerHub.Api.Repositories.IApplicantRepository' from singleton 'CareerHub.Api.Services.IApplicantService'.
   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteValidator.VisitCallSite(ServiceCallSite callSite, CallSiteValidatorState argument)
   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteValidator.VisitConstructor(ConstructorCallSite constructorCallSite, CallSiteValidatorState state)
   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteValidator.VisitCallSite(ServiceCallSite callSite, CallSiteValidatorState argument)
   at Microsoft.Extensions.DependencyInjection.ServiceProvider.ValidateService(ServiceDescriptor descriptor)
   --- End of inner exception stack trace ---
   at Microsoft.Extensions.DependencyInjection.ServiceProvider.ValidateService(ServiceDescriptor descriptor)
   at Microsoft.Extensions.DependencyInjection.ServiceProvider..ctor(ICollection`1 serviceDescriptors, ServiceProviderOptions options)
   --- End of inner exception stack trace ---
   at Microsoft.Extensions.DependencyInjection.ServiceProvider..ctor(ICollection`1 serviceDescriptors, ServiceProviderOptions options)
   at Microsoft.Extensions.DependencyInjection.ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(IServiceCollection services, ServiceProviderOptions options)
   at Microsoft.Extensions.Hosting.HostApplicationBuilder.Build()
   at Microsoft.AspNetCore.Builder.WebApplicationBuilder.Build()
   at Program.<Main>$(String[] args) in C:\Users\alika\CareerHub.Api\Program.cs:line 96

***Fix for the Error***
I was able to resolve the error by changing the AddSingleton to AddScoped. The service lifetime needs to match their underlying dependencies and therefore the DI engine was able to start up once their lifetimes correlated.

**Repository Design Decision**
I went with the concept of domain aggregation. I established explicit boundaries for the data access layer. For example: IApplicationRepository - handles all applicant applications, state transition and lifecycle updates. IJobListingRepository - manages operational queries specifically focussing on open/closed job postings and listing details.

#### The Choice Regarding ICompanyRepository
I explicitly chose to implement a dedicated ICompanyRepository alongside the other domain repositories. This boundary was drawn because:
- Strict Separation of Concerns: Giving Company its own repository ensures that company-specific data mutations (such as managing corporate profiles or onboarding new clients) are completely isolated from job listings and applications.
- Domain Autonomy:A Company represents a core root aggregate within the CareerHub system. By provisioning a distinct repository layer, this prevents the database query methods from becoming bloated or mixed with unrelated listing logic, making the data access layer highly maintainable and clean.

### What the Controller Lost
During the architectural cleanup, the controllers were stripped of all heavy logic, leaving them strictly to perform three basic infrastructure tasks: Parse the HTTP Request $\rightarrow$ Call the Service Layer $\rightarrow$ Return the HTTP Status. 

The following logic pieces were stripped out of the controllers and re-homed:

| Logic Extracted | New Layer Location | Architectural Justification |
| :--- | :--- | :--- |
| **Business & Rule Validation** *(e.g., checking if a listing is closed or an application is a duplicate)* | **Service Layer (`ApplicationService`)** | The controller should never dictate domain-specific workflows. Moving this here isolates business integrity from our HTTP framework. |
| **State Machine Verification** *(Validating status transitions)* | **Domain / Engine Layer (`ApplicationStatusValidator`)** | Status transition sequencing is an internal system rule. It must live in a centralized, framework-agnostic space. |
| **Manual Data Mapping / Assignment** *(Instantiating domain models)* | **Service Layer / DTO Layer** | Controllers shouldn't manually bind internal property parameters; abstracting this keeps API schemas completely decoupled from DB shapes. |
| **Try-Catch Blocks & HTTP Error Mapping** | **Middleware Layer (`GlobalExceptionHandler`)** | Eliminates cluttered defensive code blocks across action methods. Centralizing error handling ensures a unified API error shape. |



### 3. Status Transition Design

To enforce valid application lifecycle states without introducing messy procedural code pathways, the work flow rules were treated as a Directed Graph Map. 

Instead of writing complex, nested if/else statements or cascading switch blocks that are highly prone to edge-case bugs, the rules are configured entirely as data inside a static, read-only HashSet<(ApplicationStatus From, ApplicationStatus To)> collection container.

#### One-Line Changes
Because the evaluation logic uses a highly optimized, constant-time HashSet.Contains() lookup method, the validation engine itself never needs to change when a state rule evolves. 

For example, if business requirements change tomorrow to allow an application to move directly from `Offered` $\rightarrow$ `Accepted`, a developer only needs to modify exactly one line of code inside our central configuration file:

### 4. Lifetime Misconfiguration
The way I understand this is that a Singleton is created once when the app opens and lives forever, a scoped on the other hand is created freshly for every HTTP request and then destoyed right after. If the Scoped repository is inside a singleton the service the singleton will capture it and force it to live forever, resulting in the system crashing immediately on startup.

DbContext is not thread safe, and when a singleton holds a scoped, when different users try hitting the API at the same time they will all end up sharing the same database tracking context. This will lead to data corruption, crash queries and mixed up user sessions.

The database context will be locked in memory forever, instead of being cleaned up. This will casue the apps data to keep climbing and accumilating until the entire server crashes.


## Assignment 2.4 Decisions:
### 1. Constraint Placement
Why- Service-layer validation runs inside your applications memory. It can however, be completely bypassed if a developer runs a direct SQL script in pgAdmin during a production incident, a raw data migration script runs with bad data, or a bug is introduced in a new API service variant

Consequently - Without a database-level check constraint. PostgreSQL will blindly save a listing where SalaryMin is greater than SalaryMax. This corrupts the data integrity, break public job-board filtering, and causes your frontend to display invalid salary ranges to applicants.

### 2. Index Column Ordering
PostgreSQL uses B-Tree composite indexes from left to right. Think of it like a telephone book sorted by Last Name, then First Name.

Example: 
- Index 1 (CompanyId, Status): Put CompanyId first. It is highly selective (it instantly narrows down thousands of rows to just one company's rows). Then Status filters down the remaining tiny subset. 

- Index 2 (Status, ExpiresAt): Put Status first. Status = 'Active' dramatically slices your table size down to only open listings. PostgreSQL can then instantly traverse the ExpiresAt values for that active subset. If a query filters only on ExpiresAt (the non-leftmost column), PostgreSQL will bypass the index entirely and default to a slow sequential table scan.


### 3. Identify Hot Paths
A hot path is a repository method executed constantly under normal user activity.
Ways of Identifying Hot paths:
- HasAppliedAsync: Called every single time an applicant opens a job detail page or clicks "Apply". High frequency matters here because if 1,000 users are browsing listings, this query fires continuously. Compiled queries eliminate the overhead of EF Core parsing the LINQ expression into SQL every split second.

- GetActiveListingsAsync: Called on every single page load of the public job board. Because it is the entry point of the entire application, caching its query execution plan saves massive CPU cycles on your database server.

### 4. FromSql Scope
The Limitation: EF Core's LINQ translator cannot translate advanced SQL window functions like RANK() OVER (PARTITION BY ... ORDER BY ...).

The PostgreSQL Feature: It requires PostgreSQL's native RANK() window function combined with conditional filtering syntax (COUNT(*) FILTER (WHERE...)). Writing it in raw SQL via Database.SqlQuery<T> is the only way to execute this efficiently on the database side without pulling thousands of rows into memory to sort them via C#.

## Part 2 -Database Check Constraints Verification Guide

This project utilizes PostgreSQL check constraints at the database level to ensure data integrity for salary structures and listing dates. To test and verify these rules manually, follow the guide below to interface directly with the database container.

### Prerequisites
Ensure your Docker desktop application is running and the database container is active.

### Execution Steps

1. **Log into the PostgreSQL Container**
   Open your terminal or PowerShell and access the `psql` interface inside the running container:
```powershell
   docker exec -it careerhub-postgres psql -U postgres
```
2. **Connect to the Application Database**
``plaintext``
   \c CareerHub

3. **Test Case 1: Expiry Date Constraint (ck_job_listings_expiry_date)**
INSERT INTO job_listings ("Id", "Title", "Description", "Location", "CompanyId", "Type", "IsActive", "SalaryMin", "SalaryMax", "PostedAt", "ExpiresAt") 
VALUES (gen_random_uuid(), 'Invalid Date Job', 'Testing constraints', 'Remote', '91111111-1111-1111-1111-111111111111', 0, true, 40000, 50000, now(), now() - interval '1 day');

4. **Test Case 2: Salary Range Constraint (ck_job_listings_salary_range)**
INSERT INTO job_listings ("Id", "Title", "Description", "Location", "CompanyId", "Type", "IsActive", "SalaryMin", "SalaryMax", "PostedAt", "ExpiresAt") 
VALUES (gen_random_uuid(), 'Invalid Salary Job', 'Testing constraints', 'Remote', '91111111-1111-1111-1111-111111111111', 0, true, 50000, 40000, now(), now() + interval '10 days');

5. **Clean Up**
``plaintext``
      \q

## EXPLAIN ANALYZE Findings
***Before Optimization (Without Index)***
```text
 Seq Scan on job_listings  (cost=0.00..15.22 rows=54 width=366) (actual time=0.030..0.104 rows=105.00 loops=1)
   Filter: ("IsActive" AND ("ExpiresAt" > now()))
   Rows Removed by Filter: 110
   Buffers: shared hit=12
 Planning:
   Buffers: shared hit=93
 Planning Time: 0.805 ms
 Execution Time: 0.135 ms
(8 rows)

(END)
```

***After Optimization(With Composite Index)***
```text
 Bitmap Heap Scan on job_listings  (cost=4.69..17.49 rows=53 width=366) (actual time=0.054..0.081 rows=105.00 loops=1)
   Recheck Cond: ("IsActive" AND ("ExpiresAt" > now()))
   Heap Blocks: exact=12
   Buffers: shared hit=13
   ->  Bitmap Index Scan on ix_job_listings_status_expires_at  (cost=0.00..4.68 rows=53 width=0) (actual time=0.032..0.032 rows=129.00 loops=1)
         Index Cond: (("IsActive" = true) AND ("ExpiresAt" > now()))
         Index Searches: 1
         Buffers: shared hit=1
 Planning Time: 0.192 ms
 Execution Time: 1.979 ms
(10 rows)
```

### 5. FromSql Parameterization Proof

Why String Interpolation inside SqlQuery<T> is Safe:** Entity Framework Core intercepts the interpolated string at compile-time and automatically extracts the variables into true database command parameters (`FormattableString`), ensuring that user inputs are handled strictly as isolated values rather than executable database syntax.

Why string.Format or + Concatenation is Unsafe:Pre-building the string using raw concatenation or `string.Format` physically flattens the input parameters directly into the raw text before it ever reaches EF Core, enabling malicious actors to inject arbitrary SQL control characters that permanently alter the command's structural logic.

## Connection Pool Configurations & Mathematical Scaling

To ensure high-availability and prevent resource exhaustion under heavy traffic loads, I configured connection pooling boundaries inside our database infrastructure layer.

### 1. Scaling Architecture Calculation

Our maximum connection pool parameters were calculated using the following server capacity boundaries:

Total PostgreSQL Server Capacity: $100$ maximum concurrent connections.
Superuser/Telemetry Reservation: $10$ connections strictly reserved for administrators and background telemetry monitoring.
Available Application Headroom: $100 - 10 = 90$ total connections available for application servers.

Because the production web layer scales out symmetrically across 3 active, live instances, the remaining connection overhead is divided equally among them to prevent instance conflict:

$$90 \div 3 = 30 \text{ connections per instance}$$

Therefore, our production connection string explicitly defines `Maximum Pool Size=30`.

#### C. Multi-Environment Pool Layout Proof

To ensure our application behaves predictably across distinct operational spaces, the connection pool configuration is split into two distinct tiers:


### 2. Runtime Behavior Under Full Pool Exhaustion

When an application instance experiences a traffic spike and all 30 pool connections are actively leased out, subsequent database queries undergo the following lifecycle:

1. Queueing (Blocking State): The incoming request does not instantly fail. It enters a synchronous blocking wait queue, waiting for an executing thread to finish its operation and yield its connection back to the pool manager.
2. Observable Symptom: From the end-user's perspective, the application experiences a severe latency spike. The API request will appear to spin endlessly without returning data.
3. Timeout Failure: If no connection is freed within the default timeout threshold window, the request drops out of the queue and throws a `Npgsql.PostgresException: Connection pool exhaustion timeout`. The API handles this exception by returning an HTTP 500 Internal Server Error to the client



## Assignment 3.2:
** 1.Pagination Strategy **
- Offset Pragnation (skip/take)
- Why: For a jobListings board that CareerHub will have, where users will be able to view the active postings, an occasional duplicate jobListign will be more tolerable as long as users are able to skip to deeper pages.
- Drawbacks: If a job listing is added between a users fetching page 1 and 2, the newly inserted jobListing will shift the existing items down. When clients call page 2 they will see the last item of Page 1 repeating on page 2

** 2. PATCH vs PUT Race Condition **
In a scenario where recruiter A and B, open joblisting #1 simmultaneously. Recruiter A updates the SalaryMin, and recruiter B  fix a typo in the Description. Both of their requests will be PUT requests which sends the entire payload. 

If recruiter A's request finishes second, it will silently overwrite recruiter B's typo fix, and recruiter B;s changes are lost.

- The Nullable DTO Resolution: This solution, only sends non-null fields, so rqruiter A's SalaryMin and recruiter B's Description. Because all other fields are null, the backend completely ignores them and allows bothe updates to continue independently. 

- The Nullable solution do However have some drawbacks, such as: you cannot clear a field, becasue the field will look the same as null. JSON Patch can resolve this by using operation objects.

** 3. Versioning Strategy **
Breaking Change disrupts the existing frontend integrations. Example: Renaming SalaryMin to MinimumSalary.
Non-breaking change maintains backwards compatibility. Example: Adding new field like Remote Work to the response.

Default Versioning: Setting the AssumeDefaultVersionWhenUnspecified = true. This forces the middleware routing to process requests without a version prefix, as if they target /api/v1/jobs, ensuring existing clients dont break when versioned routing is introduced.

** 4. Rate Limiting Algorithm **
- I'm choosing the Sliding Window or the Token Bucket as it is ideal for high-traffic endpoints
- Fixed windows are vulnerable for bursting at boundary edges. A sliding window spreads traffic out across segments to prevent boundary spikes.

** ETag Evaluation **
- A strong ETag relies on a tracking property that changes whenever any field in the row is modified. You would add a ConcurrencyToken field like a Guid VersionId or an incremental RowVersion timestamp directly onto the JobListing domain model. This value would regenerate via an entity lifecycle interceptor every single time an update or save operation is run on that database row.  
- Why the current approach can produce a stale 304: If a recruiter modifies only the job description text or location, the PostedAt timestamp and SalaryMin remain completely unchanged. The computed ETag remains exactly the same, causing a user to receive a misleading 304 Not Modified and miss out on seeing the updated job details.  