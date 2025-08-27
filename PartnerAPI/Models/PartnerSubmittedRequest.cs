namespace PartnerAPI.Models;

public class PartnerSubmittedRequest
{
    public string partnerkey { get; set; } 
    public string partnerrefno { get; set; } 
    public string partnerpassword { get; set; } 
    public long totalamount { get; set; }
    public List<ItemDetail>? items { get; set; }
    public string timestamp { get; set; }
    public string sig { get; set; }
}
