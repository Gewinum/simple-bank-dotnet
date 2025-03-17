# Simplebank Application

## Overview
Simplebank is a personal project aimed at gaining experience in building a banking application. It provides functionalities such as account management, user management, and transaction processing. The application is built using C# and leverages ASP.NET Core for the API and Entity Framework Core for database interactions. **Note: This application is not intended for actual banking use.**

## Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/)
- [dotnet-ef](https://docs.microsoft.com/en-us/ef/core/cli/dotnet)

## Running the Application

### Step 1: Create a New Database
Before running the application, ensure that you have created a new database. You can use any database management tool to create a new database.

### Step 2: Configure the Database Connection
Update the connection string in the `appsettings.json` file located in the `Simplebank.API` project to point to your newly created database.

### Step 3: Run the Application via Docker Compose
To run the application using Docker Compose, follow these steps:

1. **Build the Docker Images**:
   ```sh
   docker-compose build
   ```

2. **Run the Docker Containers**:
   ```sh
   docker-compose up
   ```

This will start the application along with any required services (e.g., database) defined in the `docker-compose.yml` file.

### Step 4: Apply Migrations
After the containers are up and running, apply the database migrations to set up the database schema. The database port is exposed externally, and you can use the `make migratedb` command to apply the migrations:

1. **Open a new terminal**:
2. **Run the migration command**:
   ```sh
   make migratedb
   ```

## Makefile Commands
The `Makefile` provides several commands to manage database migrations:

- **Add a new migration**:
  ```sh
  make addmigration name=MigrationName
  ```

- **Remove the last migration**:
  ```sh
  make removemigration
  ```

- **Update the database**:
  ```sh
  make migratedb
  ```

- **Drop the database**:
  ```sh
  make dropdb
  ```

## Postman Files
You can find the Postman collection and environment files in the `./Postman` directory. For detailed instructions on how to import and use these files, refer to the [Postman Documentation](./Postman/postman.md).

## Contributing
Contributions are welcome! Please fork the repository and submit a pull request.

## License
This project is licensed under the MIT License. See the `LICENSE` file for more details.
