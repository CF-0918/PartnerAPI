namespace PartnerAPI.Models;

public class APIReponseMessage
{
    public int result { get; set; }      
    public long? totalamount { get; set; }
    public long? totaldiscount { get; set; }
    public long? finalamount { get; set; }
    public string? resultmessage { get; set; }
}
