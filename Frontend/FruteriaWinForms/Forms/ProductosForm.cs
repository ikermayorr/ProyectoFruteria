using System.Drawing;
using System.Windows.Forms;
using FruteriaWinForms.Models;

namespace FruteriaWinForms;

public class ProductosForm : Form
{
    private readonly ApiClient api;
    private readonly DataGridView tabla = new();
    private readonly TextBox busqueda = new();
    private readonly ComboBox filtro = new();
    private List<Producto> productos = new();

    public ProductosForm(ApiClient cliente)
    {
        api = cliente;
        Text = "Catálogo de productos";
        Size = new Size(1000, 700);
        MinimumSize = new Size(800, 550);
        CrearInterfaz();
        Load += async (s, e) => await CargarProductos();
    }

    private void CrearInterfaz()
    {
        Controls.Add(new Label {
            Text = "Catálogo de productos",
            Font = new Font("Segoe UI", 23, FontStyle.Bold),
            ForeColor = Color.DarkGreen,
            AutoSize = true,
            Location = new Point(10, 10)
        });

        busqueda.SetBounds(15, 85, 230, 30);
        busqueda.PlaceholderText = "Buscar producto...";
        busqueda.TextChanged += (s,e) => MostrarProductos();
        Controls.Add(busqueda);

        filtro.SetBounds(260, 85, 150, 30);
        filtro.Items.AddRange(new object[] {
            "Todas", "Fruta", "Verdura", "Otro"
        });
        filtro.SelectedIndex = 0;
        filtro.SelectedIndexChanged += (s,e) => MostrarProductos();
        Controls.Add(filtro);

        Boton("Refrescar", 425, async () => await CargarProductos());
        Boton("Nuevo", 535, async () => await Nuevo());
        Boton("Editar", 645, async () => await Editar());
        Boton("Eliminar", 755, async () => await Eliminar());

        tabla.SetBounds(15, 140, 950, 480);
        tabla.Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                       AnchorStyles.Left | AnchorStyles.Right;
        tabla.ReadOnly = true;
        tabla.AllowUserToAddRows = false;
        tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabla.MultiSelect = false;
        tabla.RowHeadersVisible = false;
        Controls.Add(tabla);
    }

    private void Boton(string texto, int x, Func<Task> accion)
    {
        Button b = new() {
            Text = texto,
            Location = new Point(x, 82),
            Size = new Size(100, 35),
            BackColor = Color.DarkGreen,
            ForeColor = Color.White
        };
        b.Click += async (s,e) => await Ejecutar(accion);
        Controls.Add(b);
    }

    private async Task Ejecutar(Func<Task> accion)
    {
        try { await accion(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
    }

    private async Task CargarProductos()
    {
        productos = await api.ObtenerProductos();
        MostrarProductos();
    }

    private void MostrarProductos()
    {
        string texto = busqueda.Text.Trim();
        string categoria = filtro.SelectedItem?.ToString() ?? "Todas";

        var datos = productos.Where(p =>
            p.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) &&
            (categoria == "Todas" || p.Categoria == categoria)
        ).Select(p => new {
            p.Id, p.Nombre, p.Categoria,
            p.PrecioKg, p.StockKg
        }).ToList();

        tabla.DataSource = datos;
    }

    private Producto? Seleccionado()
    {
        if (tabla.CurrentRow == null) return null;
        int id = Convert.ToInt32(tabla.CurrentRow.Cells["Id"].Value);
        return productos.FirstOrDefault(p => p.Id == id);
    }

    private async Task Nuevo()
    {
        using var dialogo = new ProductoDialog();
        if (dialogo.ShowDialog() != DialogResult.OK) return;
        await api.CrearProducto(dialogo.ObtenerProducto());
        await CargarProductos();
    }

    private async Task Editar()
    {
        var p = Seleccionado();
        if (p == null) {
            MessageBox.Show("Selecciona un producto.");
            return;
        }

        using var dialogo = new ProductoDialog(p);
        if (dialogo.ShowDialog() != DialogResult.OK) return;

        var actualizado = dialogo.ObtenerProducto();
        actualizado.Id = p.Id;

        await api.ActualizarProducto(actualizado);
        await CargarProductos();
    }

    private async Task Eliminar()
    {
        var p = Seleccionado();
        if (p == null) {
            MessageBox.Show("Selecciona un producto.");
            return;
        }

        if (MessageBox.Show($"¿Eliminar {p.Nombre}?",
            "Confirmar", MessageBoxButtons.YesNo) != DialogResult.Yes)
            return;

        await api.EliminarProducto(p.Id);
        await CargarProductos();
    }
}

public class ProductoDialog : Form
{
    private readonly TextBox nombre = new();
    private readonly ComboBox categoria = new();
    private readonly NumericUpDown precio = new();
    private readonly NumericUpDown stock = new();

    public ProductoDialog(Producto? producto = null)
    {
        Text = producto == null ? "Nuevo producto" : "Editar producto";
        Size = new Size(420, 390);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;

        Campo("Nombre", nombre, 30);

        categoria.Items.AddRange(new object[] {
            "Fruta", "Verdura", "Otro"
        });
        categoria.DropDownStyle = ComboBoxStyle.DropDownList;
        Campo("Categoría", categoria, 90);

        precio.DecimalPlaces = 2;
        precio.Minimum = 0.01m;
        precio.Maximum = 100000;
        Campo("Precio por kg (€)", precio, 150);

        stock.DecimalPlaces = 3;
        stock.Maximum = 100000;
        Campo("Stock (kg)", stock, 210);

        if (producto != null) {
            nombre.Text = producto.Nombre;
            categoria.SelectedItem = producto.Categoria;
            precio.Value = (decimal)producto.PrecioKg;
            stock.Value = (decimal)producto.StockKg;
        } else {
            categoria.SelectedIndex = 0;
            precio.Value = 1;
        }

        Button guardar = new() {
            Text = "Guardar",
            Location = new Point(140, 290),
            Size = new Size(120, 40),
            DialogResult = DialogResult.OK
        };

        guardar.Click += (s,e) => {
            if (string.IsNullOrWhiteSpace(nombre.Text) ||
                categoria.SelectedIndex < 0) {
                MessageBox.Show("Completa todos los campos.");
                DialogResult = DialogResult.None;
            }
        };

        Controls.Add(guardar);
        AcceptButton = guardar;
    }

    private void Campo(string texto, Control control, int y)
    {
        Controls.Add(new Label {
            Text = texto,
            Location = new Point(25, y),
            Size = new Size(145, 30)
        });

        control.Location = new Point(180, y);
        control.Size = new Size(190, 30);
        Controls.Add(control);
    }

    public Producto ObtenerProducto() => new() {
        Nombre = nombre.Text.Trim(),
        Categoria = categoria.Text,
        PrecioKg = (double)precio.Value,
        StockKg = (double)stock.Value
    };
}
