namespace QuanLySinhVien
{
    public partial class login : Form
    {
        public login()
        {
            InitializeComponent();
        }

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            string emailNhap = txtEmail.Text.Trim();
            string matKhauNhap = txtMatKhau.Text.Trim();

            string emailSinhVien = "0015068@st.huce.edu.vn";
            string mssv = "0015068";

            if (emailNhap == emailSinhVien && matKhauNhap == mssv)
            {
                MessageBox.Show("Đăng nhập thành công");

                main frm = new main();
                frm.Show();

                this.Hide();
            }
            else
            {
                MessageBox.Show("Đăng nhập thất bại");
            }
        }
    }
}