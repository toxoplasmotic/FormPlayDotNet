# FormPlay - TPS Report Management System

FormPlay is a private web application for creating, reviewing, and approving intimacy proposal forms (TPS Reports) between partners. This README contains instructions for setting up and running the application locally on a Mac Mini.

## Overview

FormPlay allows partners to:
- Create new TPS Reports using the embedded PDF form
- Submit reports for partner review
- Respond to submitted reports (approve or deny)
- Finalize approved reports and add them to calendars
- Track the status of all reports

## Prerequisites

To run FormPlay locally on your Mac Mini, you'll need:

1. **.NET 8.0 SDK**: Required to build and run the .NET application
2. **Visual Studio Code** (optional): Recommended editor for code modifications
3. **Git**: For source code version control (optional)

## Installation Steps

### 1. Install .NET 8.0 SDK

```bash
# Using Homebrew
brew install dotnet-sdk

# Or download from Microsoft's website
# Visit https://dotnet.microsoft.com/download/dotnet/8.0
```

Verify the installation by running:
```bash
dotnet --version
```

### 2. Clone or Download the Project

If you're using Git:
```bash
git clone [your-repository-url]
cd FormPlay
```

Or simply extract the project files to a directory of your choice.

### 3. Configure the Application

The main configuration is in `appsettings.json`. Review and update as needed:
- Email settings (for notifications)
- PDF template path
- User information

### 4. Set Up PDF Template

Make sure the template directory exists and contains the TPS Report PDF template:
```bash
mkdir -p wwwroot/templates
mkdir -p wwwroot/reports
# Copy your TPS Report PDF template to wwwroot/templates/vanilla.pdf
```

### 5. Run the Application

From the project directory, run:
```bash
dotnet restore
dotnet run --urls="http://0.0.0.0:5000"
```

This will:
1. Restore all necessary packages
2. Build the application
3. Start the server on port 5000, accessible from any device on your local network

### 6. Access the Application

Open a web browser and navigate to:
- `http://localhost:5000` (from the same machine)
- `http://[your-mac-ip-address]:5000` (from other devices on your network)

## Project Structure

- **Controllers/**: Contains MVC controllers that handle HTTP requests
- **Models/**: Data models including TpsReport, User, and related entities
- **Views/**: Razor views for the user interface
- **Services/**: Business logic services for PDF handling, email, etc.
- **wwwroot/**: Static files (CSS, JavaScript, PDFs)
  - **templates/**: Contains the TPS Report PDF templates
  - **reports/**: Stores completed PDF reports

## Dependencies

The application uses the following NuGet packages:
- **Microsoft.EntityFrameworkCore (8.0.0)**: ORM for database operations
- **Microsoft.EntityFrameworkCore.InMemory (8.0.0)**: In-memory database provider
- **iTextSharp.LGPLv2.Core (3.4.2)**: PDF manipulation library
- **PDF.js**: Client-side PDF rendering (included in wwwroot/lib)

## PDF Templates

The application uses a PDF template with form fields that users can fill out. The default template is located at `wwwroot/templates/vanilla.pdf`. 

When running locally, make sure this directory exists and contains the necessary template file.

## Running for Intranet Only

To restrict access to your local network only:
1. Ensure your firewall allows incoming connections on port 5000
2. Run the application with `--urls="http://0.0.0.0:5000"` as shown above
3. Access via local IP rather than exposing to the internet

## Troubleshooting

### Common Issues

1. **Port conflicts**: If port 5000 is already in use, change the port in the run command:
   ```bash
   dotnet run --urls="http://0.0.0.0:5001"
   ```

2. **Missing PDF template**: Ensure the PDF template exists at `wwwroot/templates/vanilla.pdf`

3. **Database errors**: The application uses an in-memory database by default. If you encounter errors, check the database configuration in `Program.cs`.

4. **Email sending issues**: If email notifications aren't working, verify the SMTP settings in `appsettings.json`.

## Support

For questions or issues, please refer to the project documentation or contact the project maintainer.