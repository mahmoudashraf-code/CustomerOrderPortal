using Microsoft.EntityFrameworkCore.Migrations;

namespace Backend.Data;

// Note: This is just an example file. 
// To create actual migrations, run these commands in the terminal:
// dotnet ef migrations add InitialMigration
// dotnet ef database update

public static class MigrationCommands
{
    /* 
    To create and apply migrations, run the following commands in the terminal:
    
    1. Install the Entity Framework Core tools (if not already installed):
       dotnet tool install --global dotnet-ef
    
    2. Create the initial migration:
       dotnet ef migrations add InitialMigration
    
    3. Apply the migration to create the database:
       dotnet ef database update
    
    Make sure SQL Server is running and accessible with the connection string 
    specified in appsettings.json before running these commands.
    */
}
