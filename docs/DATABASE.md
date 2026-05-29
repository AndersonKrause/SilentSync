# Database

## Provider

PostgreSQL (Neon)

## ORM

Entity Framework Core

## Migrations

Create migration:

```bash
dotnet ef migrations add MigrationName
```

Apply migration:

```bash
dotnet ef database update
```

## Main Tables

### Users

Stores:

* Email
* Role
* Password hash
* Verification status

### LoginCodes

Stores:

* One-time verification codes
* Expiration timestamps

### Rooms

Stores:

* Room identifiers
* Owners

### RoomMembers

Stores:

* User-room relationships

## Production Database

Hosted on Neon PostgreSQL.
