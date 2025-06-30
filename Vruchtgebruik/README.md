
# Vruchtgebruik Calculator

A calculation tool with a .NET 8 Web API backend and Angular frontend.

---

## Prerequisites

Before you start, make sure you have:

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Node.js (LTS version recommended)](https://nodejs.org/en)
- [npm](https://www.npmjs.com/get-npm) (comes with Node.js)
- [Git](https://git-scm.com/downloads)
- PowerShell 5.1+ (Windows) or [PowerShell Core](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell) (Mac/Linux)

---

## Copy the run-local.ps1 script from Git

Open a PowerShell prompt in your preferred working directory (for example, `D:\test`) and run the following command:

```powershell
Invoke-WebRequest -Uri "https://github.com/GCroes/Vruchtgebruik/raw/main/Vruchtgebruik/run-local.ps1" -OutFile "run-local.ps1"
```

## Clone the Repository (if this is your preferred option, and would like to skip the previous step)

Open a terminal or PowerShell prompt in your preferred working directory (for example, `D:\test`):

```powershell
git clone https://github.com/GCroes/Vruchtgebruik.git
```

---

## Run Everything Locally

You can use the included PowerShell script to automate the build and startup of both the API and the Angular frontend.

### Run the script from your working directory:

```powershell
.\run-local.ps1
```

**This will:**
- Clone the repo (if not already present)
- Restore and build the .NET API
- Start the API on port **5005**
- Restore and build the Angular frontend
- Start the Angular dev server on port **4200**
- Perform preflight checks to ensure required ports are free

---

## Using the Application

- **API Swagger UI:** [http://localhost:5005/swagger](http://localhost:5005/swagger)
- **Frontend UI:** [http://localhost:4200](http://localhost:4200)

---

## Troubleshooting

- **Port in use:**  
  The script checks if ports 5005 (API) or 4200 (Angular) are already in use. Free them or change the port in your settings.

- **CORS errors:**  
  Ensure both the API and Angular app are running, and that CORS is correctly configured.

- **Missing prerequisites:**  
  Ensure .NET 8 SDK and Node.js are installed and available in your PATH.

---

## License

This project is for educational and demonstration purposes only.
