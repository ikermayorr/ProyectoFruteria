using System.Drawing;
using System.Windows.Forms;
using FruteriaWinForms.Models;

namespace FruteriaWinForms;

public class VentasForm : Form
{
    private readonly ApiClient api;
    private readonly ComboBox selector = new();
    private readonly NumericUpDown kilos = new();
    private readonly DataGridView tabla = new();
    private readonly Label totalLabel = new();
    private List<Producto> productos = new();
    private readonly List<DetalleVenta> carrito = new();

    public VentasForm(ApiClient cliente)
    {
        api = cliente;
        Text = "Registro de ventas";
        Size = new Size(1000, 700);
        MinimumSize = new Size(800, 550);
        CrearInterfaz();
        Load += async (s,e) => await CargarProductos();
    }

    private void CrearInterfaz()
    {
        Controls.Add(new Label {
            Text = "Registro de ventas",
            Font = new Font("Segoe UI", 23, FontStyle.Bold),
            ForeColor = Color.DarkGreen,
            AutoSize = true,
            Location = new Point(10, 10)
        });

        Controls.Add(new Label {
            Text = "Producto:",
            Location = new Point(15, 85),
            AutoSize = true
        });

        selector.SetBounds(100, 80, 220, 30);
        selector.DropDownStyle = ComboBoxStyle.DropDownList;
        selector.SelectedIndexChanged += (s,e) => {
            if (selector.SelectedItem is Producto p)
                kilos.Maximum = Math.Max(0.001m, (decimal)p.StockKg);
        };
        Controls.Add(selector);

        Controls.Add(new Label {
            Text = "Kilos:",
            Location = new Point(340, 85),
            AutoSize = true
        });

        kilos.SetBounds(390, 80, 100, 30);
        kilos.DecimalPlaces = 3;
        kilos.Minimum = 0.001m;
        kilos.Maximum = 100000;
        Controls.Add(kilos);

        Button anadir = new() {
            Text = "Añadir al carrito",
            Location = new Point(510, 78),
            Size = new Size(150, 35),
            BackColor = Color.DarkGreen,
            ForeColor = Color.White
        };
        anadir.Click += (s,e) => Anadir();
        Controls.Add(anadir);

        tabla.SetBounds(15, 145, 950, 350);
        tabla.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                       AnchorStyles.Left | AnchorStyles.Right;
        tabla.ReadOnly = true;
        tabla.AllowUserToAddRows = false;
        tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabla.RowHeadersVisible = false;
        tabla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        Controls.Add(tabla);

        totalLabel.Text = "TOTAL: 0,00 €";
        totalLabel.Font = new Font("Segoe UI", 19, FontStyle.Bold);
        totalLabel.ForeColor = Color.DarkGreen;
        totalLabel.AutoSize = true;
        totalLabel.Location = new Point(15, 520);
        Controls.Add(totalLabel);

        Button quitar = new() {
            Text = "Quitar producto",
            Location = new Point(15, 570),
            Size = new Size(145, 40)
        };
        quitar.Click += (s,e) => Quitar();
        Controls.Add(quitar);

        Button confirmar = new() {
            Text = "CONFIRMAR VENTA",
            Location = new Point(700, 565),
            Size = new Size(200, 50),
            BackColor = Color.DarkGreen,
            ForeColor = Color.White
        };
        confirmar.Click += async (s,e) => await Confirmar();
        Controls.Add(confirmar);
    }

    private async Task CargarProductos()
    {
        productos = await api.ObtenerProductos();
        selector.DataSource = null;
        selector.DataSource = productos.Where(p => p.StockKg > 0).ToList();
        selector.DisplayMember = "Nombre";
    }

    private void Anadir()
    {
        if (selector.SelectedItem is not Producto p) {
            MessageBox.Show("Selecciona un producto.");
            return;
        }

        double cantidad = (double)kilos.Value;
        var existente = carrito.FirstOrDefault(d => d.ProductoId == p.Id);
        double actual = existente?.Kilos ?? 0;

        if (actual + cantidad > p.StockKg) {
            MessageBox.Show("No hay suficiente stock.");
            return;
        }

        if (existente != null) {
            existente.Kilos += cantidad;
            existente.Subtotal = Math.Round(existente.Kilos * existente.PrecioKg, 2);
        } else {
            carrito.Add(new DetalleVenta {
                ProductoId = p.Id,
                NombreProducto = p.Nombre,
                Kilos = cantidad,
                PrecioKg = p.PrecioKg,
                Subtotal = Math.Round(cantidad * p.PrecioKg, 2)
            });
        }

        MostrarCarrito();
    }

    private void MostrarCarrito()
    {
        tabla.DataSource = null;
        tabla.DataSource = carrito.Select(d => new {
            d.ProductoId,
            Producto = d.NombreProducto,
            d.Kilos,
            d.PrecioKg,
            d.Subtotal
        }).ToList();

        totalLabel.Text = $"TOTAL: {carrito.Sum(d => d.Subtotal):F2} €";
    }

    private void Quitar()
    {
        if (tabla.CurrentRow == null) return;

        int id = Convert.ToInt32(tabla.CurrentRow.Cells["ProductoId"].Value);
        carrito.RemoveAll(d => d.ProductoId == id);
        MostrarCarrito();
    }

    private async Task Confirmar()
    {
        if (carrito.Count == 0) {
            MessageBox.Show("El carrito está vacío.");
            return;
        }

        if (MessageBox.Show(
            $"¿Confirmar venta por {carrito.Sum(d => d.Subtotal):F2} €?",
            "Confirmar", MessageBoxButtons.YesNo) != DialogResult.Yes)
            return;

        try {
            Venta venta = await api.RegistrarVenta(carrito);
            MessageBox.Show(
                $"Venta #{venta.Id} registrada.\nTotal: {venta.Total:F2} €"
            );
            carrito.Clear();
            MostrarCarrito();
            await CargarProductos();
        } catch (Exception ex) {
            MessageBox.Show(ex.Message, "Error de venta");
            await CargarProductos();
        }
    }
}
