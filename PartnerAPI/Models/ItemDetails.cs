namespace PartnerAPI.Models;

public sealed class ItemDetail
{
    public string partneritemref { get; set; } 
    public string name { get; set; }           
    public int qty { get; set; }                           
    public long unitprice { get; set; }                   
}
