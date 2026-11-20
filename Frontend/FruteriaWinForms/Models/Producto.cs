namespace FruteriaWinForms.Models;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public double PrecioKg { get; set; }
    public double StockKg { get; set; }
}
