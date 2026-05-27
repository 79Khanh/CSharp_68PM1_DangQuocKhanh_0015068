namespace QuanLySinhVien.Views;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void quảnLíSinhViênToolStripMenuItem_Click(object sender, EventArgs e)
    {
        ShowContent(new UCQLSinhVien());
    }

    private void quanlisinhvien_Load(object sender, EventArgs e)
    {
        ShowContent(new UCQLSinhVien());
    }

    private void ShowContent(UserControl content)
    {
        content.Dock = DockStyle.Fill;
        panel1.Controls.Clear();
        panel1.Controls.Add(content);
    }

    private void quảnLýLớpHọcToolStripMenuItem_Click(object sender, EventArgs e)
    {
        ShowContent(new UCQLLH());
    }
}
