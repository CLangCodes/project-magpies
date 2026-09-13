namespace ProjectMagpie.Models;
public class Thing
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public decimal? RestorationCost { get; set; }
    public decimal? SoldFor { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public void SetImageUrl() => ImageUrl = "ThingImages/" + Id + ".jpg";
    public void DeleteImageUrl() => ImageUrl = null; 
}