namespace FruteriaWinForms.Models;

public class Venta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public List<DetalleVenta> Productos { get; set; } = new();
    public double Total { get; set; }
}
