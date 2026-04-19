# Gothenburg Tolls

En applikation som simulerar hela flödet för trängselskatt i Göteborg – från kamera till faktura. Systemet är byggt med löskopplad arkitektur så att varje del kan bytas ut oberoende vid kravändringar.

---

## Förklaring kring systemet

Jag ville bygga en applikation som simulerar hela vägen från kamera till faktura, samt ha logiken löskopplad så att varje del kan bytas ut om det skulle bli kravändringar.

### API

Tanken är att kameran ska skicka en **POST** till API:et med:

- Registreringsnummer
- Datum/tid
- Typ av fordon
- Zone (Kamera)

Antagandet är att kameran bestämmer om det är ambulans, diplomat, motorcykel etc. Detta kommer att skapa ett **fordon** i databasen om det inte redan finns, och sedan ett **TollEvent**.

> API:et exponerar en minimal endpoint (`POST /TollEvent`) som tar emot en `VehiclePassageDto` och delegerar direkt till `ITollEventService.RegisterAsync()`. Endpointen är medvetet tunn – all logik lever i Application-lagret.

### DailyTollSummary

Dessa ska sedan samlas ihop till en **DailyTollSummary** via en funktion som triggas via ett dagligt intervall. En `DailyTollSummary` innehåller det totala avgiftsbeloppet för ett specifikt fordon under en dag, och kopplar ihop alla enskilda `TollEvent` som skett den dagen.

> I denna applikation testas det manuellt via Blazor Server-appen.

### Månadsvis fakturering

I slutet på varje månad är tanken att en annan funktion ska trigga och samla ihop alla **DailyTollSummary** för varje fordon under tidsperioden och skapa en `TollInvoice` som fakturerar ägaren av fordonet. Fakturan beräknar sin totalsumma dynamiskt från de kopplade dagssummorna.

> I denna applikation testas det manuellt via Blazor Server-appen.

---

## Tekniker

- **.NET 10** / C# 14
- **Onion Architecture** – lagren är organiserade i separata projekt med tydliga beroendepilar som alltid pekar inåt.
- **Repository pattern** med services som använder factories för att skapa objekt. Dessa kastar fel vid validering.
- **API** (Minimal API) för att spara TollEvents till databasen.
- **Blazor Server** för att visa datan för admins.
- **EF Core** med SQL Server och `IDbContextFactory` för databasåtkomst.
- **xUnit** + **NSubstitute** för enhetstester, **SQLite InMemory** för repositorytester.

### Medvetna val

- **FluentValidation** och **FluentAssertions** används inte medvetet. Båda biblioteken har en historik av att gå från öppen källkod till betalmodell, vilket gör dem till en risk i ett projekt. Validering och assertions hanteras istället med inbyggda .NET-verktyg.

---

## Arkitektur

Lösningen följer **Onion Architecture** med fyra lager. Beroenden går **endast inåt**:
