using System.Drawing;
using System.Windows.Forms;
using FruteriaWinForms.Models;

namespace FruteriaWinForms;

public class StockForm : Form
{
    private readonly ApiClient api;
    private readonly DataGridView tabla = new();
    private readonly CheckBox soloBajo = new() {
        Text = "Mostrar stock bajo (menos de 5 kg)",
        AutoSize = true
    };

    private List<Producto> productos = new();

    public StockForm(ApiClient cliente)
    {
        api = cliente;
        Text = "Control de stock";
        Size = new Size(1000, 700);
        MinimumSize = new Size(800, 550);
        CrearInterfaz();
        Load += async (s,e) => await Cargar();
    }

    private void CrearInterfaz()
    {
        Controls.Add(new Label {
            Text = "Control de stock",
            Font = new Font("Segoe UI", 23, FontStyle.Bold),
            ForeColor = Color.DarkGreen,
            AutoSize = true,
            Location = new Point(10, 10)
        });

        soloBajo.Location = new Point(15, 85);
        soloBajo.CheckedChanged += (s,e) => Mostrar();
        Controls.Add(soloBajo);

        Button refrescar = new() {
            Text = "Refrescar",
            Location = new Point(400, 80),
            Size = new Size(110, 35)
        };

        refrescar.Click += async (s,e) => await Ejecutar(Cargar);
        Controls.Add(refrescar);

        tabla.SetBounds(15, 140, 950, 480);
        tabla.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                       AnchorStyles.Left | AnchorStyles.Right;
        tabla.ReadOnly = true;
        tabla.AllowUserToAddRows = false;
        tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabla.RowHeadersVisible = false;
        tabla.CellFormatting += Pintar;
        Controls.Add(tabla);
    }

    private async Task Ejecutar(Func<Task> accion)
    {
        try { await accion(); }
        catch (Exception ex) { MessageBox.Show(ex.Message); }
    }

    private async Task Cargar()
    {
        productos = await api.ObtenerProductos();
        Mostrar();
    }

    private void Mostrar()
    {
        var datos = productos.Where(p =>
            !soloBajo.Checked || p.StockKg < 5
        ).Select(p => new {
            p.Id, p.Nombre, p.Categoria,
            p.PrecioKg, p.StockKg,
            Estado = p.StockKg < 5 ? "CRÍTICO" :
                     p.StockKg < 10 ? "BAJO" : "NORMAL"
        }).ToList();

        tabla.DataSource = datos;
    }

    private void Pintar(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 ||
            e.ColumnIndex < 0 ||
            tabla.Columns[e.ColumnIndex].Name != "StockKg" ||
            e.Value == null)
        {
            return;
        }

        var estilo = e.CellStyle;

        if (estilo == null)
            return;

        double stock = Convert.ToDouble(e.Value);

        if (stock < 5)
        {
            estilo.BackColor = Color.LightCoral;
            estilo.ForeColor = Color.DarkRed;
        }
        else if (stock < 10)
        {
            estilo.BackColor = Color.LightYellow;
        }
        else
        {
            estilo.BackColor = Color.Honeydew;
        }
    }
}

