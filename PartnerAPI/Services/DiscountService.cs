namespace PartnerAPI.Services;

public class DiscountService
{
    //  convert to cents for comparison
    private static long MYR(decimal d)
    {
        return (long)(d * 100m);
    }

    public (long totalDiscount, long finalAmount) Compute(long totalamountCents)
    {
        decimal basePct = 0m;

        if (totalamountCents < MYR(200)) basePct = 0m;
        else if (totalamountCents <= MYR(500)) basePct = 0.05m;
        else if (totalamountCents <= MYR(800)) basePct = 0.07m;
        else if (totalamountCents <= MYR(1200)) basePct = 0.10m;
        else basePct = 0.15m;

        decimal conditionalPct = 0m;

        var myr = totalamountCents / 100m;

        // Prime check on MYR value above 500
        if (myr > 500 && IsPrime((long)myr)) conditionalPct += 0.08m;

        // Ends with digit 5 and above 900 MYR
        if (myr > 900 && ((long)myr) % 10 == 5) conditionalPct += 0.10m;

        var totalPct = basePct + conditionalPct;
        if (totalPct > 0.20m) totalPct = 0.20m;

        var discount = (long)Math.Round(totalamountCents * totalPct, MidpointRounding.AwayFromZero);
        var finalAmt = totalamountCents - discount;
        return (discount, finalAmt);
    }

    private static bool IsPrime(long n)
    {
        if (n <= 1) return false;
        if (n <= 3) return true;
        if (n % 2 == 0 || n % 3 == 0) return false;
        for (long i = 5; i * i <= n; i += 6)
            if (n % i == 0 || n % (i + 2) == 0) return false;
        return true;
    }
}
