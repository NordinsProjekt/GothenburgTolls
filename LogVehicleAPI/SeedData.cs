using Entities.Types;
using UseCases.Dtos;

namespace LogVehicleAPI;

public static class SeedData
{
    private static readonly TimeSpan Cet = TimeSpan.FromHours(1);
    private static readonly TimeSpan Cest = TimeSpan.FromHours(2);

    public static IReadOnlyList<VehiclePassageDto> Passages { get; } =
    [
        // === JANUARI 2026 (CET +01:00) ===
        // GBG101 (Car) – pendlare, 5 jan (mån)
        new("GBG101", new DateTimeOffset(2026, 1, 5, 6, 15, 0, Cet), "Backa", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 1, 5, 7, 45, 0, Cet), "Tingstadstunneln", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 1, 5, 16, 10, 0, Cet), "Järntorget", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 1, 5, 17, 30, 0, Cet), "Backa", VehicleType.Car),
        // GBG101 – 6 jan (tis)
        new("GBG101", new DateTimeOffset(2026, 1, 6, 6, 20, 0, Cet), "Backa", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 1, 6, 17, 5, 0, Cet), "Tingstadstunneln", VehicleType.Car),
        // GBG101 – 7 jan (ons)
        new("GBG101", new DateTimeOffset(2026, 1, 7, 6, 30, 0, Cet), "Backa", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 1, 7, 7, 50, 0, Cet), "Tingstadstunneln", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 1, 7, 17, 20, 0, Cet), "Backa", VehicleType.Car),
        // XYZ789 (Car) – enstaka
        new("XYZ789", new DateTimeOffset(2026, 1, 8, 8, 10, 0, Cet), "Älvsborgsbron", VehicleType.Car),
        // MCY456 (Motorbike) – tollfri
        new("MCY456", new DateTimeOffset(2026, 1, 9, 7, 0, 0, Cet), "Backa", VehicleType.Motorbike),
        new("MCY456", new DateTimeOffset(2026, 1, 9, 15, 30, 0, Cet), "Järntorget", VehicleType.Motorbike),
        // TRK321 (Tractor) – tollfri
        new("TRK321", new DateTimeOffset(2026, 1, 12, 9, 0, 0, Cet), "Lundbyleden", VehicleType.Tractor),
        // AMB112 (Emergency) – tollfri
        new("AMB112", new DateTimeOffset(2026, 1, 13, 6, 45, 0, Cet), "Tingstadstunneln", VehicleType.Emergency),
        new("AMB112", new DateTimeOffset(2026, 1, 13, 14, 20, 0, Cet), "Backa", VehicleType.Emergency),
        // DPL007 (Diplomat) – tollfri
        new("DPL007", new DateTimeOffset(2026, 1, 14, 10, 0, 0, Cet), "Järntorget", VehicleType.Diplomat),
        // FOR999 (Foreign) – tollfri
        new("FOR999", new DateTimeOffset(2026, 1, 15, 7, 30, 0, Cet), "Älvsborgsbron", VehicleType.Foreign),
        new("FOR999", new DateTimeOffset(2026, 1, 15, 16, 45, 0, Cet), "Tingstadstunneln", VehicleType.Foreign),
        // MIL500 (Military) – tollfri
        new("MIL500", new DateTimeOffset(2026, 1, 16, 8, 0, 0, Cet), "Lundbyleden", VehicleType.Military),

        // === FEBRUARI 2026 (CET +01:00) ===
        // GBG101 – 2 feb (mån)
        new("GBG101", new DateTimeOffset(2026, 2, 2, 6, 25, 0, Cet), "Backa", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 2, 2, 7, 50, 0, Cet), "Tingstadstunneln", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 2, 2, 16, 30, 0, Cet), "Järntorget", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 2, 2, 17, 45, 0, Cet), "Backa", VehicleType.Car),
        // KLM234 (Car) – tre passager
        new("KLM234", new DateTimeOffset(2026, 2, 3, 6, 50, 0, Cet), "Backa", VehicleType.Car),
        new("KLM234", new DateTimeOffset(2026, 2, 3, 12, 0, 0, Cet), "Älvsborgsbron", VehicleType.Car),
        new("KLM234", new DateTimeOffset(2026, 2, 3, 17, 15, 0, Cet), "Tingstadstunneln", VehicleType.Car),
        // RST567 (Car) – enstaka
        new("RST567", new DateTimeOffset(2026, 2, 5, 15, 20, 0, Cet), "Järntorget", VehicleType.Car),
        // MCY456 (Motorbike)
        new("MCY456", new DateTimeOffset(2026, 2, 10, 8, 30, 0, Cet), "Lundbyleden", VehicleType.Motorbike),
        // AMB112 (Emergency) – nattutryckning
        new("AMB112", new DateTimeOffset(2026, 2, 12, 3, 15, 0, Cet), "Tingstadstunneln", VehicleType.Emergency),
        // NOP890 (Car) – två passager
        new("NOP890", new DateTimeOffset(2026, 2, 16, 7, 15, 0, Cet), "Backa", VehicleType.Car),
        new("NOP890", new DateTimeOffset(2026, 2, 16, 16, 50, 0, Cet), "Tingstadstunneln", VehicleType.Car),

        // === MARS 2026 (CET +01:00 t.o.m. 28 mars, CEST +02:00 fr.o.m. 29 mars) ===
        // GBG101 – 2 mars (mån), tung dag, 5 passager
        new("GBG101", new DateTimeOffset(2026, 3, 2, 6, 10, 0, Cet), "Backa", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 3, 2, 7, 40, 0, Cet), "Tingstadstunneln", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 3, 2, 12, 5, 0, Cet), "Järntorget", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 3, 2, 15, 45, 0, Cet), "Älvsborgsbron", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 3, 2, 17, 55, 0, Cet), "Backa", VehicleType.Car),
        // GBG101 – 3 mars (tis)
        new("GBG101", new DateTimeOffset(2026, 3, 3, 6, 15, 0, Cet), "Backa", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 3, 3, 17, 10, 0, Cet), "Tingstadstunneln", VehicleType.Car),
        // MIL500 (Military) – flera passager
        new("MIL500", new DateTimeOffset(2026, 3, 5, 6, 30, 0, Cet), "Backa", VehicleType.Military),
        new("MIL500", new DateTimeOffset(2026, 3, 5, 9, 0, 0, Cet), "Lundbyleden", VehicleType.Military),
        new("MIL500", new DateTimeOffset(2026, 3, 5, 15, 15, 0, Cet), "Tingstadstunneln", VehicleType.Military),
        // XYZ789 (Car)
        new("XYZ789", new DateTimeOffset(2026, 3, 9, 8, 45, 0, Cet), "Järntorget", VehicleType.Car),
        // DPL007 (Diplomat)
        new("DPL007", new DateTimeOffset(2026, 3, 11, 11, 30, 0, Cet), "Älvsborgsbron", VehicleType.Diplomat),
        new("DPL007", new DateTimeOffset(2026, 3, 11, 16, 0, 0, Cet), "Backa", VehicleType.Diplomat),
        // VWX432 (Car) – enstaka
        new("VWX432", new DateTimeOffset(2026, 3, 16, 14, 30, 0, Cet), "Järntorget", VehicleType.Car),
        // FOR999 (Foreign)
        new("FOR999", new DateTimeOffset(2026, 3, 18, 8, 0, 0, Cet), "Backa", VehicleType.Foreign),
        // KLM234 – fyra passager, 23 mars (mån)
        new("KLM234", new DateTimeOffset(2026, 3, 23, 6, 30, 0, Cet), "Backa", VehicleType.Car),
        new("KLM234", new DateTimeOffset(2026, 3, 23, 7, 35, 0, Cet), "Tingstadstunneln", VehicleType.Car),
        new("KLM234", new DateTimeOffset(2026, 3, 23, 15, 50, 0, Cet), "Älvsborgsbron", VehicleType.Car),
        new("KLM234", new DateTimeOffset(2026, 3, 23, 17, 20, 0, Cet), "Backa", VehicleType.Car),
        // TRK321 (Tractor) – 26 mars (tor)
        new("TRK321", new DateTimeOffset(2026, 3, 26, 10, 30, 0, Cet), "Lundbyleden", VehicleType.Tractor),
        new("TRK321", new DateTimeOffset(2026, 3, 26, 14, 0, 0, Cet), "Backa", VehicleType.Tractor),
        // GBG101 – 30 mars (mån, CEST)
        new("GBG101", new DateTimeOffset(2026, 3, 30, 6, 20, 0, Cest), "Backa", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 3, 30, 17, 10, 0, Cest), "Tingstadstunneln", VehicleType.Car),

        // === APRIL 2026 (CEST +02:00, t.o.m. 15 april) ===
        // GBG101 – 1 april (ons)
        new("GBG101", new DateTimeOffset(2026, 4, 1, 6, 25, 0, Cest), "Backa", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 4, 1, 7, 50, 0, Cest), "Tingstadstunneln", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 4, 1, 16, 30, 0, Cest), "Järntorget", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 4, 1, 17, 45, 0, Cest), "Backa", VehicleType.Car),
        // KLM234 – 2 april (tor)
        new("KLM234", new DateTimeOffset(2026, 4, 2, 6, 50, 0, Cest), "Backa", VehicleType.Car),
        new("KLM234", new DateTimeOffset(2026, 4, 2, 12, 0, 0, Cest), "Älvsborgsbron", VehicleType.Car),
        new("KLM234", new DateTimeOffset(2026, 4, 2, 17, 15, 0, Cest), "Tingstadstunneln", VehicleType.Car),
        // RST567 (Car) – enstaka, 7 april (tis)
        new("RST567", new DateTimeOffset(2026, 4, 7, 15, 20, 0, Cest), "Järntorget", VehicleType.Car),
        // AMB112 (Emergency) – 8 april (ons)
        new("AMB112", new DateTimeOffset(2026, 4, 8, 6, 45, 0, Cest), "Tingstadstunneln", VehicleType.Emergency),
        new("AMB112", new DateTimeOffset(2026, 4, 8, 14, 20, 0, Cest), "Backa", VehicleType.Emergency),
        // MCY456 (Motorbike) – 9 april (tor)
        new("MCY456", new DateTimeOffset(2026, 4, 9, 8, 30, 0, Cest), "Lundbyleden", VehicleType.Motorbike),
        // NOP890 (Car) – 13 april (mån)
        new("NOP890", new DateTimeOffset(2026, 4, 13, 7, 15, 0, Cest), "Backa", VehicleType.Car),
        new("NOP890", new DateTimeOffset(2026, 4, 13, 16, 50, 0, Cest), "Tingstadstunneln", VehicleType.Car),
        // XYZ789 (Car) – 14 april (tis)
        new("XYZ789", new DateTimeOffset(2026, 4, 14, 8, 45, 0, Cest), "Järntorget", VehicleType.Car),
        // GBG101 – 15 april (ons)
        new("GBG101", new DateTimeOffset(2026, 4, 15, 6, 10, 0, Cest), "Backa", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 4, 15, 7, 40, 0, Cest), "Tingstadstunneln", VehicleType.Car),
        new("GBG101", new DateTimeOffset(2026, 4, 15, 17, 55, 0, Cest), "Backa", VehicleType.Car),
    ];
}
