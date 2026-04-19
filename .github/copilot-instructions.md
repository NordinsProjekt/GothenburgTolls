# Copilot Code Reviewer – Instruktioner för GothenburgTolls

Detta dokument styr hur GitHub Copilot ska granska och generera kod i denna lösning.
Allt nedan är **bindande regler** – avvik endast om det finns en tydlig motivering i PR:en.

---

## 1. Arkitektur – Onion Architecture

Lösningen följer **Onion Architecture** med fyra lager. Beroenden går **endast inåt**:

```
Presentation  ─►  Application  ─►  Domain  ◄─  Infrastructure
(Blazor, API)     (UseCases)       (Entities,    (EF Core,
                                    Factories,    Repositories)
                                    Interfaces)
```

| Lager            | Projekt                                       | Får referera till         |
|------------------|-----------------------------------------------|---------------------------|
| Domain           | `Entities`, `Factories`                       | Inget annat lager         |
| Application      | `UseCases`                                    | Domain                    |
| Infrastructure   | `EF`                                          | Domain (implementerar interface) |
| Presentation     | `GothenburgTolls` (Blazor), `LogVehicleAPI`   | Application, Domain       |

### Regler
- **Domain får aldrig** referera till EF Core, ASP.NET, Blazor eller andra infrastrukturbibliotek.
- **Repository-interface** definieras i `Entities/Interfaces` och implementeras i `EF/Repositories`.
- **Application** orkestrerar – den innehåller ingen persistenslogik och inga UI-beroenden.
- **Presentation** anropar **endast** `UseCases`-tjänster, aldrig repositories direkt.

---

## 2. Domänmodeller med beteende

- Domänmodeller ska **innehålla logik** – inga anemiska modeller.
- Validering, regler och tillståndsövergångar bor på entiteten själv.
- `Vehicle` är abstrakt; varje fordonstyp (`Car`, `Motorbike`, `Tractor`, `Emergency`, `Diplomat`, `Foreign`, `Military`) är en egen subklass (TPH i EF).
- Avgiftsfrihet uttrycks via `VehicleType` + `VehicleTypeExtensions.IsTollFree()` – inte via duplicerade enum/listor.
- Setters på domänegenskaper ska vara `private`/`protected` när det är möjligt; tillstånd ändras via metoder.

---

## 3. Factory pattern – endast konstruktion

`Factories/VehicleFactory` och kommande factories följer dessa regler:

- En factory är **ren konstruktion**. Inga I/O-anrop, inga repositories, ingen DbContext.
- Validera indata (t.ex. `ArgumentException` för tom regnr).
- Vid okänt enum-värde: använd `Enum.IsDefined` och kasta `ArgumentOutOfRangeException` med **det numeriska värdet** i meddelandet och `ActualValue`.
- Factories ska vara `static` om de inte har beroenden.

---

## 4. Command pattern (planerat)

Kommande kommandon (t.ex. `CreateTollEventCommand`) ska:

- Ligga i `UseCases` (Application-lagret).
- Ha en handler som tar emot kommandot och returnerar ett resultat eller `void`.
- **Inte** innehålla domänlogik – delegera till entiteter och factories.
- Vara enhetstestbara med mocks (NSubstitute) för repositories.

---

## 5. Huvudflöde – DTO ► TollEvent ► Vehicle

API:et tar emot en `VehiclePassageDto` (record) och flödet är:

1. Controller/endpoint validerar payload.
2. `IVehicleService.GetOrCreateAsync(dto, ct)`:
   - Hämtar fordon via `IVehicleRepository.GetVehicleByRegistrationNumberAsync`.
   - Om det inte finns: anropa `VehicleFactory.Create(regNr, type)` och persistera.
3. `TollEvent` skapas och kopplas till fordonet.
4. Resultat returneras till anroparen.

### Regler
- **Factory bygger** – **Service orkestrerar** – **Repository persisterar**. Blanda aldrig dessa ansvar.
- Get-or-create-logik bor i Service, **inte** i Factory eller Repository.

---

## 6. Repositories

- Använd `IDbContextFactory<TollDbContext>` (inte injicerad DbContext).
- Async-metoder ska ta `CancellationToken` som sista parameter.
- Registrera repositories via `EF/Extensions/ServiceCollectionExtensions.AddEfRepositories()` (assembly-scanning).
- Lägg unika constraints (t.ex. `RegistrationNumber`) i `EF/Configurations/*Configuration.cs`.

---

## 7. Tester – krav

### Allmänt
- **Alla publika metoder ska ha tester.** PR utan tester för ny publik yta avvisas.
- Namnkonvention: **`Method_Scenario_ShouldExpectedResult`**.
- **Ett `Assert` per enhetstest.** Integrationstester får ha flera asserts om de verifierar samma operation.
- Använd **xUnit** + **NSubstitute** för mocks.

### Faktortester (Domain.Factories.Tests)
- Mocka inget – factory är ren.
- Täck happy path + alla valideringsfel (tom regnr, okänd enum, etc.).

### Servicetester (Application.UseCases.Tests)
- Mocka repositories och factories med NSubstitute.
- Verifiera både returvärde **och** att rätt repo-metoder anropades (i separata tester).

### Repositorytester (Infrastructure.EF.Tests)
- Använd **SQLite InMemory** via `SqliteTollDbContextFactory` (delad öppen `SqliteConnection` + `EnsureCreated`).
- **Använd inte** EF Core InMemory-providern – den döljer translation-fel och constraint-brott.
- Verifiera att Add/Save faktiskt persisterar (läs tillbaka via ny context-instans när relevant).
- Inkludera tester för unique constraints och query-translation.

---

## 8. Presentation

### LogVehicleAPI
- Endpoints är tunna – delegera direkt till `UseCases`-tjänster.
- Ingen domän- eller persistenslogik i endpoint-handlers.

### Blazor (GothenburgTolls)
- Appen ska kunna **lista fordon** och **skapa nya fordon** vid behov.
- All data hämtas och skapas via `UseCases`-tjänster (t.ex. `IVehicleService`).
- Inga direkta anrop till repositories eller `DbContext` från komponenter.

---

## 9. Kodstil

- .NET 10 / C# 14. Använd primary constructors och records där det passar.
- `async`/`await` hela vägen – inga `.Result` eller `.Wait()`.
- `CancellationToken` propageras alltid.
- Inga magiska strängar för enum-värden – använd `Enum.TryParse`/`IsDefined`.
- Undvik `var` när typen inte är uppenbar från höger sida.

---

## 10. Checklista för code review

Copilot ska flagga PR om något av följande inte är uppfyllt:

- [ ] Onion-lagrens beroenderiktning respekteras.
- [ ] Domänlogik ligger på domänentiteter, inte i services.
- [ ] Factories innehåller ingen I/O.
- [ ] Get-or-create-flöde går via service, inte factory/repository.
- [ ] Alla nya publika metoder har tester.
- [ ] Testnamn följer `Method_Scenario_ShouldExpectedResult`.
- [ ] Enhetstester har **ett** assert.
- [ ] Repositorytester använder SQLite InMemory och verifierar add/save.
- [ ] Blazor-komponenter anropar endast UseCases-tjänster.
- [ ] `CancellationToken` skickas vidare i alla async-anrop.
