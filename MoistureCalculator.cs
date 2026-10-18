namespace SeedIntakeCalculator;

public static class MoistureCalculator
{
    public static decimal CalculateDiscount(decimal moisture)
    {
        if (moisture < 4m || moisture > 100m)
            throw new ArgumentOutOfRangeException(nameof(moisture), "Влажность должна быть от 4 до 100 %.");

        if (moisture < 6m)
            return (6m - moisture) * 2.5m;

        decimal discount = 0m;
        discount += Portion(moisture, 8m, 12m) * 1m;
        discount += Portion(moisture, 12m, 15m) * 1.5m;
        discount += Portion(moisture, 15m, 20m) * 2m;
        discount += Math.Max(0m, moisture - 20m) * 3m;
        return discount;
    }

    public static decimal CalculateAcceptedWeight(decimal physicalWeight, decimal discountPercent)
    {
        if (physicalWeight <= 0m)
            throw new ArgumentOutOfRangeException(nameof(physicalWeight));
        if (discountPercent < 0m || discountPercent > 100m)
            throw new ArgumentOutOfRangeException(nameof(discountPercent));
        return Math.Round(physicalWeight * (1m - discountPercent / 100m), 3, MidpointRounding.AwayFromZero);
    }

    public static IReadOnlyList<(string Range, decimal Amount)> Breakdown(decimal moisture)
    {
        CalculateDiscount(moisture);
        if (moisture < 6m)
            return [("4–6 % · 2,5 % за 1 %", (6m - moisture) * 2.5m)];
        return
        [
            ("8–12 % · 1 % за 1 %", Portion(moisture, 8m, 12m)),
            ("12–15 % · 1,5 % за 1 %", Portion(moisture, 12m, 15m) * 1.5m),
            ("15–20 % · 2 % за 1 %", Portion(moisture, 15m, 20m) * 2m),
            ("Свыше 20 % · 3 % за 1 %", Math.Max(0m, moisture - 20m) * 3m)
        ];
    }

    private static decimal Portion(decimal value, decimal from, decimal to) =>
        Math.Max(0m, Math.Min(value, to) - from);
}
