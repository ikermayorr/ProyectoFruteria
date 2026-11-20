using System.Net.Http.Json;
using System.Text.Json;
using FruteriaWinForms.Models;

namespace FruteriaWinForms;

public class ApiClient : IDisposable
{
    private readonly HttpClient http;

    private readonly JsonSerializerOptions opciones = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient()
    {
        http = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:8000/"),
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    public async Task<List<Producto>> ObtenerProductos()
    {
        return await http.GetFromJsonAsync<List<Producto>>(
            "productos", opciones
        ) ?? new List<Producto>();
    }

    public async Task<Producto> CrearProducto(Producto producto)
    {
        var respuesta = await http.PostAsJsonAsync(
            "productos", producto
        );

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content.ReadFromJsonAsync<Producto>(opciones)
            ?? throw new Exception("Respuesta vacía de la API");
    }

    public async Task<Producto> ActualizarProducto(Producto producto)
    {
        var respuesta = await http.PutAsJsonAsync(
            $"productos/{producto.Id}", producto
        );

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content.ReadFromJsonAsync<Producto>(opciones)
            ?? throw new Exception("Respuesta vacía de la API");
    }

    public async Task EliminarProducto(int id)
    {
        var respuesta = await http.DeleteAsync($"productos/{id}");
        respuesta.EnsureSuccessStatusCode();
    }

    public async Task<List<Venta>> ObtenerVentas()
    {
        return await http.GetFromJsonAsync<List<Venta>>(
            "ventas", opciones
        ) ?? new List<Venta>();
    }

    public async Task<Venta> RegistrarVenta(List<DetalleVenta> detalles)
    {
        var datos = new
        {
            productos = detalles.Select(d => new
            {
                productoId = d.ProductoId,
                kilos = d.Kilos
            }).ToList()
        };

        var respuesta = await http.PostAsJsonAsync("ventas", datos);

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content.ReadFromJsonAsync<Venta>(opciones)
            ?? throw new Exception("No se pudo registrar la venta");
    }

    public async Task<JsonElement> ObtenerInformeHoy()
    {
        return await http.GetFromJsonAsync<JsonElement>(
            "informes/ventas-hoy"
        );
    }

    public void Dispose()
    {
        http.Dispose();
    }
}
