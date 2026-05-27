using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLySinhVien.Views;

public partial class UCQLSinhVien : UserControl
{
    private readonly Label notesLabel = new();
    private readonly TextBox notesTextBox = new();
    private int? selectedStudentId;

    public UCQLSinhVien()
    {
        InitializeComponent();
        ConfigureAdditionalFields();
        ConfigureGrid();
        WireEvents();
        LoadClassrooms();
        LoadStudents();
        ClearInputs();
    }

    private void ConfigureAdditionalFields()
    {
        groupBox1.Height = 700;

        notesLabel.AutoSize = true;
        notesLabel.Font = label6.Font;
        notesLabel.Location = new Point(25, 610);
        notesLabel.Text = "Ghi chú:";

        notesTextBox.Location = new Point(29, 640);
        notesTextBox.Size = new Size(751, 35);

        groupBox1.Controls.Add(notesLabel);
        groupBox1.Controls.Add(notesTextBox);
    }

    private void ConfigureGrid()
    {
        dataGridView1.AutoGenerateColumns = false;
        dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridView1.MultiSelect = false;
        dataGridView1.ReadOnly = true;

        Column1.DataPropertyName = "StudentCode";
        Column2.DataPropertyName = "FullName";
        Column3.DataPropertyName = "Gender";
        Column4.DataPropertyName = "BirthDate";
        Column4.DefaultCellStyle.Format = "dd/MM/yyyy";
        Column5.DataPropertyName = "ClassCode";

        dataGridView1.Columns.Insert(0, new DataGridViewTextBoxColumn
        {
            Name = "IdColumn",
            DataPropertyName = "Id",
            Visible = false
        });
        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "NotesColumn",
            DataPropertyName = "Notes",
            HeaderText = "Ghi chú",
            Width = 180
        });
    }

    private void WireEvents()
    {
        button1.Click += AddStudent_Click;
        button2.Click += EditStudent_Click;
        button3.Click += DeleteStudent_Click;
        button4.Click += RefreshStudents_Click;
        button5.Click += SearchStudents_Click;
        dataGridView1.CellClick += StudentsGrid_CellClick;
    }

    private void LoadClassrooms()
    {
        DataTable classrooms = Database.Query(
            "SELECT Id, ClassCode, ClassName FROM dbo.Classrooms ORDER BY ClassCode;");
        comboBox1.DataSource = classrooms;
        comboBox1.DisplayMember = "ClassCode";
        comboBox1.ValueMember = "Id";
    }

    private void LoadStudents(string keyword = "")
    {
        string sql = """
            SELECT s.Id, s.StudentCode, s.FullName, s.Gender, s.BirthDate,
                   c.ClassCode, s.Notes
            FROM dbo.Students AS s
            INNER JOIN dbo.Classrooms AS c ON c.Id = s.ClassId
            WHERE @Keyword = N''
               OR s.StudentCode LIKE N'%' + @Keyword + N'%'
               OR s.FullName LIKE N'%' + @Keyword + N'%'
               OR c.ClassCode LIKE N'%' + @Keyword + N'%'
            ORDER BY s.Id DESC;
            """;
        dataGridView1.DataSource = Database.Query(
            sql,
            new SqlParameter("@Keyword", SqlDbType.NVarChar, 255) { Value = keyword });
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(textBox1.Text))
        {
            MessageBox.Show("Vui lòng nhập mã sinh viên.");
            textBox1.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(textBox2.Text))
        {
            MessageBox.Show("Vui lòng nhập họ và tên.");
            textBox2.Focus();
            return false;
        }

        if (comboBox1.SelectedValue is null)
        {
            MessageBox.Show("Vui lòng tạo và chọn một lớp học trước.");
            return false;
        }

        return true;
    }

    private SqlParameter[] StudentParameters()
    {
        return
        [
            new("@StudentCode", SqlDbType.NVarChar, 50) { Value = textBox1.Text.Trim() },
            new("@FullName", SqlDbType.NVarChar, 255) { Value = textBox2.Text.Trim() },
            new("@BirthDate", SqlDbType.Date) { Value = dateTimePicker2.Value.Date },
            new("@Gender", SqlDbType.NVarChar, 10) { Value = comboBox2.Text },
            new("@ClassId", SqlDbType.Int) { Value = Convert.ToInt32(comboBox1.SelectedValue) },
            new("@Notes", SqlDbType.NVarChar, -1) { Value = notesTextBox.Text.Trim() }
        ];
    }

    private void AddStudent_Click(object? sender, EventArgs e)
    {
        if (!ValidateInputs())
        {
            return;
        }

        try
        {
            Database.Execute(
                """
                INSERT INTO dbo.Students (StudentCode, FullName, BirthDate, Gender, ClassId, Notes)
                VALUES (@StudentCode, @FullName, @BirthDate, @Gender, @ClassId, @Notes);
                """,
                StudentParameters());
            MessageBox.Show("Thêm mới sinh viên thành công.");
            LoadStudents();
            ClearInputs();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể thêm sinh viên: " + ex.Message);
        }
    }

    private void EditStudent_Click(object? sender, EventArgs e)
    {
        if (selectedStudentId is null)
        {
            MessageBox.Show("Vui lòng chọn sinh viên cần sửa.");
            return;
        }

        if (!ValidateInputs())
        {
            return;
        }

        try
        {
            SqlParameter[] parameters =
            [
                .. StudentParameters(),
                new("@Id", SqlDbType.Int) { Value = selectedStudentId.Value }
            ];
            Database.Execute(
                """
                UPDATE dbo.Students
                SET StudentCode = @StudentCode, FullName = @FullName, BirthDate = @BirthDate,
                    Gender = @Gender, ClassId = @ClassId, Notes = @Notes
                WHERE Id = @Id;
                """,
                parameters);
            MessageBox.Show("Cập nhật sinh viên thành công.");
            LoadStudents();
            ClearInputs();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể cập nhật sinh viên: " + ex.Message);
        }
    }

    private void DeleteStudent_Click(object? sender, EventArgs e)
    {
        if (selectedStudentId is null)
        {
            MessageBox.Show("Vui lòng chọn sinh viên cần xóa.");
            return;
        }

        if (MessageBox.Show("Bạn có chắc chắn muốn xóa sinh viên đã chọn?", "Xác nhận",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            Database.Execute(
                "DELETE FROM dbo.Students WHERE Id = @Id;",
                new SqlParameter("@Id", SqlDbType.Int) { Value = selectedStudentId.Value });
            MessageBox.Show("Xóa sinh viên thành công.");
            LoadStudents();
            ClearInputs();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể xóa sinh viên: " + ex.Message);
        }
    }

    private void RefreshStudents_Click(object? sender, EventArgs e)
    {
        textBox3.Clear();
        LoadClassrooms();
        LoadStudents();
        ClearInputs();
    }

    private void SearchStudents_Click(object? sender, EventArgs e)
    {
        LoadStudents(textBox3.Text.Trim());
    }

    private void StudentsGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
        selectedStudentId = Convert.ToInt32(row.Cells["IdColumn"].Value);
        textBox1.Text = row.Cells[Column1.Name].Value?.ToString();
        textBox2.Text = row.Cells[Column2.Name].Value?.ToString();
        comboBox2.Text = row.Cells[Column3.Name].Value?.ToString();
        if (row.Cells[Column4.Name].Value is DateTime birthDate)
        {
            dateTimePicker2.Value = birthDate;
        }
        comboBox1.Text = row.Cells[Column5.Name].Value?.ToString();
        notesTextBox.Text = row.Cells["NotesColumn"].Value?.ToString();
    }

    private void ClearInputs()
    {
        selectedStudentId = null;
        textBox1.Clear();
        textBox2.Clear();
        notesTextBox.Clear();
        dateTimePicker2.Value = DateTime.Today;
        comboBox2.SelectedIndex = comboBox2.Items.Count > 0 ? 0 : -1;
        comboBox1.SelectedIndex = comboBox1.Items.Count > 0 ? 0 : -1;
        textBox1.Focus();
    }

}
