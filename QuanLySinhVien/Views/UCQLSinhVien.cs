using System.Data;
using Microsoft.Data.SqlClient;

namespace QuanLySinhVien.Views;

public partial class UCQLSinhVien : UserControl
{
    private const int PageSize = 10;

    private readonly Label notesLabel = new();
    private readonly TextBox notesTextBox = new();
    private readonly Label pageInfoLabel = new();
    private int currentPage = 1;
    private int? selectedStudentId;
    private string currentKeyword = string.Empty;
    private int totalPages = 1;

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

        pageInfoLabel.AutoSize = false;
        pageInfoLabel.Font = label6.Font;
        pageInfoLabel.Location = new Point(945, 755);
        pageInfoLabel.Size = new Size(550, 47);
        pageInfoLabel.TextAlign = ContentAlignment.MiddleCenter;

        groupBox1.Controls.Add(notesLabel);
        groupBox1.Controls.Add(notesTextBox);
        Controls.Add(pageInfoLabel);
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
        button6.Click += FirstPage_Click;
        button7.Click += PreviousPage_Click;
        button8.Click += NextPage_Click;
        button9.Click += LastPage_Click;
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

    private void LoadStudents(string keyword = "", bool resetPage = true)
    {
        currentKeyword = keyword;
        if (resetPage)
        {
            currentPage = 1;
        }

        DataTable countTable = Database.Query(
            """
            SELECT COUNT(1) AS Total
            FROM dbo.Students AS s
            INNER JOIN dbo.Classrooms AS c ON c.Id = s.ClassId
            WHERE @Keyword = N''
               OR s.StudentCode LIKE N'%' + @Keyword + N'%'
               OR s.FullName LIKE N'%' + @Keyword + N'%'
               OR c.ClassCode LIKE N'%' + @Keyword + N'%';
            """,
            new SqlParameter("@Keyword", SqlDbType.NVarChar, 255) { Value = currentKeyword });

        int totalRecords = Convert.ToInt32(countTable.Rows[0]["Total"]);
        totalPages = Math.Max(1, (int)Math.Ceiling(totalRecords / (double)PageSize));
        currentPage = Math.Min(Math.Max(1, currentPage), totalPages);

        string sql = """
            SELECT s.Id, s.StudentCode, s.FullName, s.Gender, s.BirthDate,
                   c.ClassCode, s.Notes
            FROM dbo.Students AS s
            INNER JOIN dbo.Classrooms AS c ON c.Id = s.ClassId
            WHERE @Keyword = N''
               OR s.StudentCode LIKE N'%' + @Keyword + N'%'
               OR s.FullName LIKE N'%' + @Keyword + N'%'
               OR c.ClassCode LIKE N'%' + @Keyword + N'%'
            ORDER BY s.Id DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;
        dataGridView1.DataSource = Database.Query(
            sql,
            new SqlParameter("@Keyword", SqlDbType.NVarChar, 255) { Value = currentKeyword },
            new SqlParameter("@Offset", SqlDbType.Int) { Value = (currentPage - 1) * PageSize },
            new SqlParameter("@PageSize", SqlDbType.Int) { Value = PageSize });

        selectedStudentId = null;
        UpdatePagingState(totalRecords);
    }

    private void UpdatePagingState(int totalRecords)
    {
        pageInfoLabel.Text = $"Trang {currentPage} / {totalPages} - Tổng {totalRecords} sinh viên";
        button6.Enabled = currentPage > 1;
        button7.Enabled = currentPage > 1;
        button8.Enabled = currentPage < totalPages;
        button9.Enabled = currentPage < totalPages;
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
            LoadStudents(currentKeyword);
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

        if (StudentCodeExistsForOtherStudent(textBox1.Text.Trim(), selectedStudentId.Value))
        {
            MessageBox.Show("Mã sinh viên đã tồn tại. Vui lòng nhập mã khác.");
            textBox1.Focus();
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
            LoadStudents(currentKeyword, resetPage: false);
            ClearInputs();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không thể cập nhật sinh viên: " + ex.Message);
        }
    }

    private static bool StudentCodeExistsForOtherStudent(string studentCode, int currentStudentId)
    {
        DataTable result = Database.Query(
            """
            SELECT COUNT(1) AS Total
            FROM dbo.Students
            WHERE StudentCode = @StudentCode AND Id <> @Id;
            """,
            new SqlParameter("@StudentCode", SqlDbType.NVarChar, 50) { Value = studentCode },
            new SqlParameter("@Id", SqlDbType.Int) { Value = currentStudentId });

        return Convert.ToInt32(result.Rows[0]["Total"]) > 0;
    }

    private void DeleteStudent_Click(object? sender, EventArgs e)
    {
        if (selectedStudentId is null)
        {
            MessageBox.Show("Vui lòng chọn sinh viên cần xóa.");
            return;
        }

        string selectedStudent = $"{textBox1.Text.Trim()} - {textBox2.Text.Trim()}".Trim(' ', '-');
        if (MessageBox.Show($"Bạn có chắc chắn muốn xóa sinh viên {selectedStudent}?", "Xác nhận",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            int affectedRows = Database.Execute(
                "DELETE FROM dbo.Students WHERE Id = @Id;",
                new SqlParameter("@Id", SqlDbType.Int) { Value = selectedStudentId.Value });

            if (affectedRows == 0)
            {
                MessageBox.Show("Sinh viên đã chọn không còn tồn tại trong cơ sở dữ liệu.");
                LoadStudents(currentKeyword, resetPage: false);
                ClearInputs();
                return;
            }

            MessageBox.Show("Xóa sinh viên thành công.");
            LoadStudents(currentKeyword, resetPage: false);
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

    private void FirstPage_Click(object? sender, EventArgs e)
    {
        currentPage = 1;
        LoadStudents(currentKeyword, resetPage: false);
    }

    private void PreviousPage_Click(object? sender, EventArgs e)
    {
        currentPage--;
        LoadStudents(currentKeyword, resetPage: false);
    }

    private void NextPage_Click(object? sender, EventArgs e)
    {
        currentPage++;
        LoadStudents(currentKeyword, resetPage: false);
    }

    private void LastPage_Click(object? sender, EventArgs e)
    {
        currentPage = totalPages;
        LoadStudents(currentKeyword, resetPage: false);
    }

    private void StudentsGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
        if (row.Cells["IdColumn"].Value is null or DBNull)
        {
            return;
        }

        dataGridView1.ClearSelection();
        row.Selected = true;

        selectedStudentId = Convert.ToInt32(row.Cells["IdColumn"].Value);
        textBox1.Text = GetCellText(row, Column1.Name);
        textBox2.Text = GetCellText(row, Column2.Name);
        comboBox2.Text = GetCellText(row, Column3.Name);

        object? birthDateValue = row.Cells[Column4.Name].Value;
        if (birthDateValue is DateTime birthDate)
        {
            dateTimePicker2.Value = birthDate;
        }

        comboBox1.Text = GetCellText(row, Column5.Name);
        notesTextBox.Text = GetCellText(row, "NotesColumn");
    }

    private static string GetCellText(DataGridViewRow row, string columnName)
    {
        object? value = row.Cells[columnName].Value;
        return value is null or DBNull ? string.Empty : value.ToString() ?? string.Empty;
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
