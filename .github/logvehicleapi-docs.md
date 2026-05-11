# LogVehicleAPI – Dokumentation

Minimalt ASP.NET Core API för att registrera fordonspassager i Göteborgs trängselskattesystem.

---

## Anslutning

| Miljö       | URL                          |
|-------------|------------------------------|
| HTTP (dev)  | `http://localhost:5030`      |
| HTTPS (dev) | `https://localhost:7021`     |

OpenAPI-schema är tillgängligt i Development via:
```
GET /openapi/v1.json
```

Ingen autentisering krävs.

---

## Konfiguration (appsettings)

API:et kräver en SQL Server-connection string med nyckeln `TollDb`:

```json
{
  "ConnectionStrings": {
    "TollDb": "Server=...;Database=...;..."
  }
}
```

EF Core-migrationer körs automatiskt vid uppstart (utom i miljön `"Testing"`).

---

## Endpoints

### `POST /TollEvent`

Registrerar en fordonspassage. Skapar fordonet automatiskt om det inte redan finns.

**Request body** (`application/json`):

```json
{
  "registrationNumber": "ABC123",
  "eventDateTime": "2026-01-05T06:15:00+01:00",
  "zone": "Backa",
  "vehicleType": 6
}
```

| Fält                | Typ            | Krav       | Beskrivning                                          |
|---------------------|----------------|------------|------------------------------------------------------|
| `registrationNumber`| `string`       | Obligatorisk | Fordonets registreringsnummer                       |
| `eventDateTime`     | `DateTimeOffset` | Obligatorisk | Tidpunkt för passagen, inklusive tidszonsoffset    |
| `zone`              | `string`       | Obligatorisk | Namn på betalstationen/zonen                        |
| `vehicleType`       | `integer`      | Obligatorisk | Fordonstyp som heltal, se tabell nedan              |

#### VehicleType-värden

| Värde | Namn        | Avgiftsfri |
|-------|-------------|------------|
| `0`   | Motorbike   | Ja         |
| `1`   | Tractor     | Ja         |
| `2`   | Emergency   | Ja         |
| `3`   | Diplomat    | Ja         |
| `4`   | Foreign     | Ja         |
| `5`   | Military    | Ja         |
| `6`   | Car         | Nej        |

**Svar**:

| HTTP-statuskod | Situation                                              | Body                              |
|----------------|--------------------------------------------------------|-----------------------------------|
| `201 Created`  | Passagen registrerades                                 | `{ "id": "<guid>" }`             |
| `400 Bad Request` | Ogiltig eller saknad data i request              | `{ "error": "<meddelande>" }`    |
| `409 Conflict` | Databaskonflikt (t.ex. dubblett)                       | `{ "error": "A database conflict occurred." }` |

**Exempelanrop (curl)**:
```bash
curl -X POST http://localhost:5030/TollEvent \
  -H "Content-Type: application/json" \
  -d '{
    "registrationNumber": "ABC123",
    "eventDateTime": "2026-01-05T06:15:00+01:00",
    "zone": "Backa",
    "vehicleType": 6
  }'
```

**Exempelanrop (C# HttpClient)**:
```csharp
var dto = new
{
    registrationNumber = "ABC123",
    eventDateTime = DateTimeOffset.Now,
    zone = "Backa",
    vehicleType = 6
};

var response = await httpClient.PostAsJsonAsync("http://localhost:5030/TollEvent", dto);
```

---

### `POST /seed` *(Development only)*

Fyller databasen med testdata (fordonspassager från jan–apr 2026).
Returnerar `409 Conflict` om det redan finns toll events i databasen.

**Svar**:

| HTTP-statuskod | Situation                    |
|----------------|------------------------------|
| `200 OK`       | Testdata skapades            |
| `409 Conflict` | Databasen innehåller redan data |

---

## Felhantering

Alla valideringsfel kastas som `ArgumentException`, `ArgumentNullException` eller `ArgumentOutOfRangeException` i domän-/applikationslagret och mappas till `400 Bad Request` av endpointen.
