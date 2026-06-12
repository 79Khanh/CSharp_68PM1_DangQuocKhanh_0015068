namespace QuanLySinhVien.Views;

public partial class MainForm : Form
{
    private readonly LoginForm? loginForm;
    private bool isLoggingOut;

    public MainForm(LoginForm? loginForm = null)
    {
        this.loginForm = loginForm;
        InitializeComponent();
        menuStrip1.Items[2].Click += Logout_Click;
        FormClosed += MainForm_FormClosed;
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

    private void Logout_Click(object? sender, EventArgs e)
    {
        if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        isLoggingOut = true;
        loginForm?.Show();
        Close();
    }

    private void MainForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        if (!isLoggingOut)
        {
            Application.Exit();
        }
    }
}
