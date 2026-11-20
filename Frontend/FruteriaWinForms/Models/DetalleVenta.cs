namespace FruteriaWinForms.Models;

public class DetalleVenta
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = "";
    public double Kilos { get; set; }
    public double PrecioKg { get; set; }
    public double Subtotal { get; set; }
}
