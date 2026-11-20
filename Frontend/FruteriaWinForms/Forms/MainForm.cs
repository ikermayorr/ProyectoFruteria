using System.Drawing;
using System.Windows.Forms;

namespace FruteriaWinForms;

public class MainForm : Form
{
    private readonly ApiClient api = new();

    public MainForm()
    {
        Text = "Frutería - Sistema de Gestión";
        Size = new Size(760, 500);
        MinimumSize = new Size(650, 430);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.Honeydew;

        Controls.Add(new Label
        {
            Text = "SISTEMA DE GESTIÓN DE FRUTERÍA",
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = Color.DarkGreen,
            AutoSize = true,
            Location = new Point(90, 50)
        });
        Controls.Add(new Label
        {
            Text = "Selecciona una sección. La aplicación necesita que la API esté iniciada.",
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Location = new Point(95, 105)
        });

        CrearBoton("Catálogo de productos", 100, 165, () => new ProductosForm(api));
        CrearBoton("Control de stock", 390, 165, () => new StockForm(api));
        CrearBoton("Registro de ventas", 100, 245, () => new VentasForm(api));
        CrearBoton("Informes del día", 390, 245, () => new InformesForm(api));
        FormClosed += (_, _) => api.Dispose();
    }

    private void CrearBoton(string texto, int x, int y, Func<Form> crearFormulario)
    {
        var boton = new Button
        {
            Text = texto,
            Location = new Point(x, y),
            Size = new Size(250, 55),
            BackColor = Color.DarkGreen,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12, FontStyle.Bold)
        };
        boton.Click += (_, _) =>
        {
            using var formulario = crearFormulario();
            formulario.StartPosition = FormStartPosition.CenterParent;
            formulario.ShowDialog(this);
        };
        Controls.Add(boton);
    }
}
