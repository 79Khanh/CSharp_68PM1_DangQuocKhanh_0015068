using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLySinhVien.Views;

public partial class UCQLLH : UserControl
{
    private int? selectedClassroomId;

    public UCQLLH()
    {
        InitializeComponent();
        ConfigureControls();
        WireEvents();
        LoadClassrooms();
        ClearInputs();
    }

    private void ConfigureControls()
    {
        textBox1.ReadOnly = true;
        comboBox1.DropDownStyle = ComboBoxStyle.DropDown;
        comboBox1.Items.Clear();

        dataGridView1.AutoGenerateColumns = false;
        dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridView1.MultiSelect = false;
        dataGridView1.ReadOnly = true;
        Column1.DataPropertyName = "Id";
        Column2.DataPropertyName = "ClassCode";
        Column3.DataPropertyName = "ClassName";
        Column4.DataPropertyName = "Notes";
    }

    private void WireEvents()
    {
        button1.Click += AddClassroom_Click;
        button2.Click += EditClassroom_Click;
        button3.Click += DeleteClassroom_Click;
        button4.Click += RefreshClassrooms_Click;
        button5.Click += SearchClassrooms_Click;
        dataGridView1.CellClick += ClassroomsGrid_CellClick;
    }

    private void LoadClassrooms(string keyword = "")
    {
        dataGridView1.DataSource = Database.Query(
            """
            SELECT Id, ClassCode, ClassName, Notes
            FROM dbo.Classrooms
            WHERE @Keyword = N''
               OR CONVERT(NVARCHAR(20), Id) LIKE N'%' + @Keyword + N'%'
               OR ClassCode LIKE N'%' + @Keyword + N'%'
               OR ClassName LIKE N'%' + @Keyword + N'%'
            ORDER BY Id DESC;
            """,
            new SqlParameter("@Keyword", SqlDbType.NVarChar, 255) { Value = keyword });
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(textBox2.Text))
        {
            MessageBox.Show("Vui lòng nhập mã lớp.");
            textBox2.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(comboBox1.Text))
        {
            MessageBox.Show("Vui lòng nhập tên lớp.");
            comboBox1.Focus();
            return false;
        }

        return true;
    }

    private SqlParameter[] ClassroomParameters()
    {
        return
        [
            new("@ClassCode", SqlDbType.NVarChar, 50) { Value = textBox2.Text.Trim() },
            new("@ClassName", SqlDbType.NVarChar, 255) { Value = comboBox1.Text.Trim() },
            new("@Notes", SqlDbType.NVarChar, -1) { Value = textBox4.Text.Trim() }
        ];
    }

    private void AddClassroom_Click(object? sender, EventArgs e)
    {
        if (!ValidateInputs())
        {
            return;
        }

        if (ClassCodeExists(textBox2.Text.Trim()))
        {
            MessageBox.Show("Mã lớp đã tồn tại. Vui lòng nhập mã khác.");
            textBox2.Focus();
            return;
        }

        try
        {
            Database.Execute(
                """
                INSERT INTO dbo.Classrooms (ClassCode, ClassName, Notes)
                VALUES (@ClassCode, @ClassName, @Notes);
                """,
                ClassroomParameters());
            MessageBox.Show("Thêm mới lớp học thành công.");
            LoadClassrooms();
            ClearInputs();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể thêm lớp học: " + ex.Message);
        }
    }

    private void EditClassroom_Click(object? sender, EventArgs e)
    {
        if (selectedClassroomId is null)
        {
            MessageBox.Show("Vui lòng chọn lớp học cần sửa.");
            return;
        }

        if (!ValidateInputs())
        {
            return;
        }

        if (ClassCodeExists(textBox2.Text.Trim(), selectedClassroomId.Value))
        {
            MessageBox.Show("Mã lớp đã tồn tại. Vui lòng nhập mã khác.");
            textBox2.Focus();
            return;
        }

        try
        {
            SqlParameter[] parameters =
            [
                .. ClassroomParameters(),
                new("@Id", SqlDbType.Int) { Value = selectedClassroomId.Value }
            ];
            int affectedRows = Database.Execute(
                """
                UPDATE dbo.Classrooms
                SET ClassCode = @ClassCode, ClassName = @ClassName, Notes = @Notes
                WHERE Id = @Id;
                """,
                parameters);

            if (affectedRows == 0)
            {
                MessageBox.Show("Lớp học đã chọn không còn tồn tại trong cơ sở dữ liệu.");
                LoadClassrooms();
                ClearInputs();
                return;
            }

            MessageBox.Show("Cập nhật lớp học thành công.");
            LoadClassrooms();
            ClearInputs();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể cập nhật lớp học: " + ex.Message);
        }
    }

    private static bool ClassCodeExists(string classCode, int? currentClassroomId = null)
    {
        DataTable result = Database.Query(
            """
            SELECT COUNT(1) AS Total
            FROM dbo.Classrooms
            WHERE ClassCode = @ClassCode
              AND (@Id IS NULL OR Id <> @Id);
            """,
            new SqlParameter("@ClassCode", SqlDbType.NVarChar, 50) { Value = classCode },
            new SqlParameter("@Id", SqlDbType.Int) { Value = currentClassroomId ?? (object)DBNull.Value });

        return Convert.ToInt32(result.Rows[0]["Total"]) > 0;
    }

    private void DeleteClassroom_Click(object? sender, EventArgs e)
    {
        if (selectedClassroomId is null)
        {
            MessageBox.Show("Vui lòng chọn lớp học cần xóa.");
            return;
        }

        if (MessageBox.Show("Bạn có chắc chắn muốn xóa lớp học đã chọn?", "Xác nhận",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            int affectedRows = Database.Execute(
                "DELETE FROM dbo.Classrooms WHERE Id = @Id;",
                new SqlParameter("@Id", SqlDbType.Int) { Value = selectedClassroomId.Value });

            if (affectedRows == 0)
            {
                MessageBox.Show("Lớp học đã chọn không còn tồn tại trong cơ sở dữ liệu.");
                LoadClassrooms();
                ClearInputs();
                return;
            }

            MessageBox.Show("Xóa lớp học thành công.");
            LoadClassrooms();
            ClearInputs();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể xóa lớp đang có sinh viên. Chi tiết: " + ex.Message);
        }
    }

    private void RefreshClassrooms_Click(object? sender, EventArgs e)
    {
        textBox3.Clear();
        LoadClassrooms();
        ClearInputs();
    }

    private void SearchClassrooms_Click(object? sender, EventArgs e)
    {
        LoadClassrooms(textBox3.Text.Trim());
    }

    private void ClassroomsGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
        selectedClassroomId = Convert.ToInt32(row.Cells[Column1.Name].Value);
        textBox1.Text = selectedClassroomId.Value.ToString();
        textBox2.Text = row.Cells[Column2.Name].Value?.ToString();
        comboBox1.Text = row.Cells[Column3.Name].Value?.ToString();
        textBox4.Text = row.Cells[Column4.Name].Value?.ToString();
    }

    private void ClearInputs()
    {
        selectedClassroomId = null;
        textBox1.Clear();
        textBox2.Clear();
        comboBox1.Text = "";
        textBox4.Clear();
        textBox2.Focus();
    }

}
