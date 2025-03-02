addmigration:
	dotnet ef migrations add $(name) --project Simplebank.Infrastructure --startup-project Simplebank.API

removemigration:
	dotnet ef migrations remove --project Simplebank.Infrastructure --startup-project Simplebank.API

migratedb:
	dotnet ef database update --project Simplebank.Infrastructure --startup-project Simplebank.API

dropdb:
	dotnet ef database drop --project Simplebank.Infrastructure --startup-project Simplebank.API

.PHONY: addmigration, removemigration, migratedb, dropdb