# HNG-Task2
#Overview
A RESTful API for storing, retrieving, filtering, and deleting strings, built for HNG Internship Stage 1 Backend Task. Uses ASP.NET Core 8.0 with C#, in-memory storage, and JSON persistence (/tmp/strings.json on Railway, data/strings.json locally). Deployed on Railway at https://hng-task2-production-c6b7.up.railway.app/.

#Tech Stack
Framework: ASP.NET Core 8.0
Language: C#
Storage: In-memory + JSON
Dependencies: Swashbuckle.AspNetCore
Deployment: Railway (Dockerfile/Nixpacks)

#Setup
Local
Clone: git clone [https://github.com/yourusername/HNG-Task2.git](https://github.com/Precious-Adeoye/HNG-Task2)
Restore: dotnet restore HNG-Task2/HNG-Task2.csproj
Run: dotnet run --project HNG-Task2/HNG-Task2.csproj
Test (PowerShell):

#License
MIT

#Link to endpoint
https://hng-task2-production-c6b7.up.railway.app/
