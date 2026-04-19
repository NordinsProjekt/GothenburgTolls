# Gothenburg Tolls

## Förklaring kring systemet

Jag ville bygga en applikation som simulerar hela vägen från kamera till faktura, samt ha logiken löskopplad så att varje del kan bytas ut om det skulle bli kravändringar.

### API

Tanken är att kameran ska skicka en **POST** till API:et med:

- Registreringsnummer
- Datum/tid
- Typ av fordon

Antagandet är att kameran bestämmer om det är ambulans, diplomat, motorcykel etc. Detta kommer att skapa ett **fordon** i databasen om det inte redan finns, och sedan ett **TollEvent**.

### DailyTollSummary

Dessa ska sedan samlas ihop till en **DailyTollSummary** via en funktion som triggas via ett dagligt intervall.

> I denna applikation testas det manuellt via Blazor Server-appen.

### Månadsvis fakturering

I slutet på varje månad är tanken att en annan funktion ska trigga och samla ihop alla **DailyTollSummary** för varje fordon under tidsperioden och fakturera ägaren av fordonet.

> Kan också triggas via Blazor manuellt.

---

## Tekniker

- **.NET 10** / C# 14
- **Clean Architecture** – gillar att visualisera lagren med mappar och nummer.
- **Repository pattern** med services som använder factories för att skapa objekt. Dessa kastar fel vid validering.
- **API** för att spara TollEvents till databasen.
- **Blazor Server** för att visa datan för admins.

### Medvetna val

- **FluentValidation** och **FluentAssertions** används inte medvetet. Båda biblioteken har en historik av att gå från öppen källkod till betalmodell, vilket gör dem till en risk i ett projekt. Validering och assertions hanteras istället med inbyggda .NET-verktyg.
