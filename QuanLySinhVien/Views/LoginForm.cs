namespace QuanLySinhVien.Views;

public partial class LoginForm : Form
{
    public LoginForm()
    {
        InitializeComponent();
    }

    private void btnDangNhap_Click(object sender, EventArgs e)
    {
        string emailNhap = txtEmail.Text.Trim();
        string matKhauNhap = txtMatKhau.Text.Trim();

        if (emailNhap == "0015068@st.huce.edu.vn" && matKhauNhap == "0015068")
        {
            MessageBox.Show("Đăng nhập thành công.");
            MainForm mainForm = new();
            mainForm.Show();
            Hide();
            return;
        }

        MessageBox.Show("Đăng nhập thất bại.");
    }
}
