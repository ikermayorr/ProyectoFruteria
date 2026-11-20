
using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;

namespace FruteriaWinForms;

public class InformesForm : Form
{
    private readonly ApiClient api;

    private Label total = new();
    private Label numero = new();
    private Label producto = new();

    private DataGridView tabla = new();

    public InformesForm(ApiClient cliente)
    {
        api = cliente;
        Text = "Informes de ventas";
        Size = new Size(1000, 700);
        MinimumSize = new Size(800, 550);
        CrearInterfaz();
        Load += async (s, e) => await CargarInforme();
    }

    private void CrearInterfaz()
    {
        Label titulo = new()
        {
            Text = "Informes de ventas",
            Font = new Font("Segoe UI", 23, FontStyle.Bold),
            ForeColor = Color.DarkGreen,
            AutoSize = true,
            Location = new Point(10, 10)
        };

        Controls.Add(titulo);

        CrearIndicador(total, "Total vendido", 15, 85);
        CrearIndicador(numero, "Número de ventas", 330, 85);
        CrearIndicador(producto, "Producto más vendido", 645, 85);

        tabla.Location = new Point(15, 260);
        tabla.Size = new Size(950, 330);
        tabla.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                       AnchorStyles.Left | AnchorStyles.Right;
        tabla.ReadOnly = true;
        tabla.AllowUserToAddRows = false;
        tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabla.RowHeadersVisible = false;

        Button refrescar = new()
        {
            Text = "Actualizar informe",
            Location = new Point(15, 210),
            Size = new Size(160, 35)
        };

        refrescar.Click += async (s, e) => await Ejecutar(CargarInforme);

        Controls.Add(tabla);
        Controls.Add(refrescar);
    }

    private void CrearIndicador(Label etiqueta, string titulo, int x, int y)
    {
        Panel panel = new()
        {
            Location = new Point(x, y),
            Size = new Size(280, 105),
            BackColor = Color.White
        };

        Label encabezado = new()
        {
            Text = titulo,
            Location = new Point(15, 15),
            AutoSize = true,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 10)
        };

        etiqueta.Text = "...";
        etiqueta.Location = new Point(15, 48);
        etiqueta.AutoSize = true;
        etiqueta.Font = new Font("Segoe UI", 18, FontStyle.Bold);
        etiqueta.ForeColor = Color.DarkGreen;

        panel.Controls.Add(encabezado);
        panel.Controls.Add(etiqueta);

        Controls.Add(panel);
    }

    private async Task Ejecutar(Func<Task> accion)
    {
        try
        {
            await accion();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error");
        }
    }

    private async Task CargarInforme()
    {
        JsonElement informe = await api.ObtenerInformeHoy();

        double vendido = informe.GetProperty("totalVendido").GetDouble();
        int ventas = informe.GetProperty("numeroVentas").GetInt32();

        JsonElement masVendido =
            informe.GetProperty("productoMasVendido");

        string nombre = masVendido.ValueKind == JsonValueKind.String
            ? masVendido.GetString() ?? "Ninguno"
            : "Ninguno";

        total.Text = $"{vendido:F2} €";
        numero.Text = ventas.ToString();
        producto.Text = nombre;

        JsonElement kilos = informe.GetProperty("kilosPorProducto");

        var datos = new List<object>();

        foreach (JsonProperty elemento in kilos.EnumerateObject())
        {
            datos.Add(new
            {
                Producto = elemento.Name,
                KilosVendidos = elemento.Value.GetDouble()
            });
        }

        tabla.DataSource = datos;
    }
}
