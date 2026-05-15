namespace QuanLySinhVien
{
    public partial class Form1 : Form
    {
        public Form1()
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

                quanlisinhvien frm = new quanlisinhvien();
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