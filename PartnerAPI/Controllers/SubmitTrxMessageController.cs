// Controllers/SubmitTrxMessageController.cs
using Microsoft.AspNetCore.Mvc;
using PartnerAPI.Services;
using PartnerAPI.Models;
using log4net;
using System.Text;
using System.Security.Cryptography;

[ApiController]
[Route("api/[controller]")]
public class SubmitTrxMessageController : ControllerBase
{
    private static readonly ILog logger = LogManager.GetLogger(typeof(SubmitTrxMessageController));
    private readonly PartnerAuthService _auth;
    private readonly SignatureService _sig;
    private readonly DiscountService _disc;

    public SubmitTrxMessageController(PartnerAuthService auth, SignatureService sig, DiscountService disc)
    {
        _auth = auth;
        _sig = sig;
        _disc = disc;
    }


    [HttpPost]
    public IActionResult Index([FromBody] PartnerSubmittedRequest r)
    {
        //Console.WriteLine($"[Check]   DateTime Now : {DateTimeOffset.UtcNow}");
        logger.Info($"=============Received Request ==============");
        logger.Info($"DateTime Now : {DateTimeOffset.UtcNow}");
        logger.Info( $"IN partnerkey={r.partnerkey}, ref={r.partnerrefno}" );
        // Basic field checks
        var missing = RequiredMissing(r);

        //validation here
        if (missing != null)
        {
            logger.Info($"{missing}is required");
            return Fail($"{missing} is Required.");
        }
     

        if (!_auth.IsAllowed(r.partnerkey) || !_auth.VerifyPasswordBase64(r.partnerkey, r.partnerpassword))
        {
            logger.Warn("Access Denied");
            return Fail("Access Denied!");
        }
        

        //hash the password for logging
        const string salt = "Sup3rDemoKey_ChangeMe!";
        var input = (r.partnerpassword ?? string.Empty).Trim();
        byte[] hashBytes;
        using (var sha = SHA256.Create())
        {
            hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(salt + input));
        }
        // take first 16 bytes of SHA-256 and Base64 them for a short, non-reversible mask
        var maskedPwd = "ENC:" + Convert.ToBase64String(hashBytes, 0, 16);

        logger.Info(
            $"IN partnerkey={r.partnerkey}, ref={r.partnerrefno}, total={r.totalamount}, items={r.items?.Count ?? 0}, ts={r.timestamp}, pwd={maskedPwd}, sig={r.sig}"
        );

        if (!DateTimeOffset.TryParse(r.timestamp, out var ts))
        {
            logger.Warn($"Invalid timestamp format: '{r.timestamp}'");
            return Fail("timestamp is invalid.");
        }

        var nowUtc = DateTimeOffset.UtcNow;
        var diffMin = Math.Abs((nowUtc - ts.ToUniversalTime()).TotalMinutes);
        logger.Info($"Timestamp check: now(UTC)={nowUtc:O}, ts(UTC)={ts:O}, diffMin={diffMin:F2} min");

        if (diffMin > 5)
        {
            logger.Warn($"Expired timestamp: diffMin={diffMin:F2} > 5");
            return Fail("Expired.");
        }

        if (!_sig.Verify(r))
        {
            logger.Warn("Signature verification failed.");
            return Fail("Access Denied!"); // signature not correct
        }

        logger.Info("Signature verification passed.");


        // Items validations 
        if (r.items is not null && r.items.Count > 0)
        {
            logger.Info($"Validating items: count={r.items.Count}, declared totalamount={r.totalamount}");

            var idx = 0;
            foreach (var it in r.items)
            {
                idx++;

                if (string.IsNullOrWhiteSpace(it.partneritemref))
                {
                    logger.Warn($"Item#{idx}: partneritemref is missing.");
                    return Fail("partneritemref is Required.");
                }

                if (string.IsNullOrWhiteSpace(it.name))
                {
                    logger.Warn($"Item#{idx} ({it.partneritemref}): name is missing.");
                    return Fail("name is Required.");
                }

                if (it.qty <= 0)
                {
                    logger.Warn($"Item#{idx} ({it.partneritemref}): qty invalid ({it.qty}), must be positive.");
                    return Fail("qty must be positive.");
                }

                if (it.qty > 5)
                {
                    logger.Warn($"Item#{idx} ({it.partneritemref}): qty invalid ({it.qty}), must not exceed 5.");
                    return Fail("qty must not exceed 5.");
                }

                if (it.unitprice <= 0)
                {
                    logger.Warn($"Item#{idx} ({it.partneritemref}): unitprice invalid ({it.unitprice}), must be positive.");
                    return Fail("unit price must be positive.");
                }
            }

            var sum = r.items.Sum(i => (long)i.qty * i.unitprice);
            if (sum != r.totalamount)
            {
                logger.Warn($"Invalid Total Amount: sum(items)={sum} (cents) != totalamount={r.totalamount} (cents).");
                return Fail("Invalid Total Amount.");
            }

            logger.Info($"Items validation passed: sum(items)={sum} (cents).");
        }

        if (r.totalamount <= 0) return Fail("totalamount must be positive.");

        // Compute discount & respond
        var (discount, finalAmount) = _disc.Compute(r.totalamount);
        var ok = new APIReponseMessage
        {
            result = 1,
            totalamount = r.totalamount,
            totaldiscount = discount,
            finalamount = finalAmount
        };

        return Ok(ok);
    }


    //if is not valid it will return a string,but if valid return null
    private static string? RequiredMissing(PartnerSubmittedRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.partnerkey)) return "partnerkey";
        if (string.IsNullOrWhiteSpace(r.partnerrefno)) return "partnerrefno";
        if (string.IsNullOrWhiteSpace(r.partnerpassword)) return "partnerpassword";
        if (r.totalamount == 0) return "totalamount";
        if (r.items != null && r.items.Count == 0) return "items";
        if (string.IsNullOrWhiteSpace(r.timestamp)) return "timestamp";
        if (string.IsNullOrWhiteSpace(r.sig)) return "sig";
        return null;
    }

    private IActionResult Fail(string msg)
    {
        return BadRequest(new { result = 0, resultMessage = msg });
    }

}
