using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EnrollmentSystem.Data;
using EnrollmentSystem.Models;

namespace EnrollmentSystem.Forms
{
    public class StudentForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSearch;
        private ComboBox cmbFilterCourse, cmbFilterYear;
        private Button btnAdd, btnEdit, btnDelete;
        private Label lblCount;

        public StudentForm()
        {
            InitializeComponent();
            LoadGrid();
        }

        private void InitializeComponent()
        {
            this.Text = "Student Management";
            this.Size = new Size(1060, 620);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9f);

            
            var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(41, 128, 185) };
            pnlTitle.Controls.Add(new Label
            {
                Text = "👨‍🎓  Student Management",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(18, 13)
            });
            this.Controls.Add(dgv);
            this.Controls.Add(pnlTitle);

            
            var pnlBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White, Padding = new Padding(10, 9, 10, 9) };

            var lblS = new Label { Text = "Search:", AutoSize = true, Location = new Point(10, 17) };
            txtSearch = new TextBox { Location = new Point(65, 13), Width = 195, Font = new Font("Segoe UI", 9f) };
            txtSearch.TextChanged += (s, e) => ApplyFilter();

            var lblC = new Label { Text = "Course:", AutoSize = true, Location = new Point(275, 17) };
            cmbFilterCourse = new ComboBox { Location = new Point(328, 13), Width = 175, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilterCourse.SelectedIndexChanged += (s, e) => ApplyFilter();

            var lblY = new Label { Text = "Year:", AutoSize = true, Location = new Point(515, 17) };
            cmbFilterYear = new ComboBox { Location = new Point(552, 13), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilterYear.SelectedIndexChanged += (s, e) => ApplyFilter();

            btnAdd    = Btn("➕ Add",    Color.FromArgb(39, 174, 96),  688, 10, 88);
            btnEdit   = Btn("✏️ Edit",   Color.FromArgb(41, 128, 185), 784, 10, 78);
            btnDelete = Btn("🗑️ Delete", Color.FromArgb(192, 57, 43),  870, 10, 88);
            var btnRefresh = Btn("🔄", Color.FromArgb(100, 115, 140), 966, 10, 36);

            btnAdd.Click    += BtnAdd_Click;
            btnEdit.Click   += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => { DataManager.LoadAll(); PopulateFilters(); LoadGrid(); };

            pnlBar.Controls.AddRange(new Control[] { lblS, txtSearch, lblC, cmbFilterCourse, lblY, cmbFilterYear, btnAdd, btnEdit, btnDelete, btnRefresh });
            this.Controls.Add(pnlBar);

            
            dgv = BuildGrid();

            
            var pnlStatus = new Panel { Dock = DockStyle.Bottom, Height = 28, BackColor = Color.FromArgb(240, 244, 248) };
            lblCount = new Label { AutoSize = true, Location = new Point(10, 7), ForeColor = Color.SlateGray, Font = new Font("Segoe UI", 8.5f) };
            pnlStatus.Controls.Add(lblCount);
            this.Controls.Add(pnlStatus);

            PopulateFilters();
        }

        private Button Btn(string text, Color color, int x, int y, int w = 80)
        {
            var b = new Button
            {
                Text = text, Location = new Point(x, y), Size = new Size(w, 30),
                BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private DataGridView BuildGrid()
        {
            var dg = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                RowHeadersVisible = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9f), GridColor = Color.FromArgb(228, 232, 240),
                ColumnHeadersHeight = 36, RowTemplate = { Height = 30 }
            };
            dg.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            dg.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dg.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dg.EnableHeadersVisualStyles = false;
            dg.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 251, 255);
            dg.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 228, 250);
            dg.DefaultCellStyle.SelectionForeColor = Color.FromArgb(20, 50, 100);
            dg.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };
            return dg;
        }

        private void PopulateFilters()
        {
            cmbFilterCourse.Items.Clear();
            cmbFilterCourse.Items.Add("All Courses");
            foreach (var c in DataManager.Courses.Select(c => c.CourseName).Distinct().OrderBy(x => x))
                cmbFilterCourse.Items.Add(c);
            if (cmbFilterCourse.SelectedIndex < 0) cmbFilterCourse.SelectedIndex = 0;

            cmbFilterYear.Items.Clear();
            cmbFilterYear.Items.Add("All Years");
            foreach (var y in new[] { "1st Year", "2nd Year", "3rd Year", "4th Year", "5th Year" })
                cmbFilterYear.Items.Add(y);
            if (cmbFilterYear.SelectedIndex < 0) cmbFilterYear.SelectedIndex = 0;
        }

        private void LoadGrid() => BindGrid(DataManager.Students.OrderBy(s => s.LastName).ToList());

        private void ApplyFilter()
        {
            string search = txtSearch.Text.Trim();
            string course = cmbFilterCourse.SelectedIndex > 0 ? cmbFilterCourse.SelectedItem?.ToString() : null;
            string year   = cmbFilterYear.SelectedIndex   > 0 ? cmbFilterYear.SelectedItem?.ToString()   : null;

            var list = string.IsNullOrEmpty(search)
                ? DataManager.Students.AsEnumerable()
                : DataManager.SearchStudents(search);

            if (!string.IsNullOrEmpty(course)) list = list.Where(s => s.Course == course);
            if (!string.IsNullOrEmpty(year))   list = list.Where(s => s.YearLevel == year);

            BindGrid(list.OrderBy(s => s.LastName).ToList());
        }

        private void BindGrid(List<Student> list)
        {
            dgv.Rows.Clear();
            dgv.Columns.Clear();

            foreach (var (hdr, fill) in new[] {
                ("Student ID",false),("First Name",true),("Last Name",true),
                ("Gender",false),("Course",false),("Year Level",false),("Contact No.",false)
            })
                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = hdr,
                    AutoSizeMode = fill ? DataGridViewAutoSizeColumnMode.Fill : DataGridViewAutoSizeColumnMode.AllCells
                });

            foreach (var s in list)
                dgv.Rows.Add(s.StudentID, s.FirstName, s.LastName, s.Gender, s.Course, s.YearLevel, s.ContactNumber);

            lblCount.Text = $"Showing {list.Count} of {DataManager.Students.Count} student(s)";
        }

        private Student SelectedStudent()
        {
            if (dgv.SelectedRows.Count == 0) return null;
            return DataManager.GetStudent(dgv.SelectedRows[0].Cells[0].Value?.ToString());
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using var dlg = new StudentDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                if (!DataManager.AddStudent(dlg.Student))
                    MessageBox.Show($"Student ID '{dlg.Student.StudentID}' already exists.", "Duplicate ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                {
                    MessageBox.Show("Student added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    PopulateFilters();
                    ApplyFilter();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var s = SelectedStudent();
            if (s == null) { MessageBox.Show("Select a student to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            using var dlg = new StudentDialog(s);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                DataManager.UpdateStudent(dlg.Student);
                MessageBox.Show("Student updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ApplyFilter();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var s = SelectedStudent();
            if (s == null) { MessageBox.Show("Select a student to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show($"Delete '{s.FullName}' ({s.StudentID})?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DataManager.DeleteStudent(s.StudentID);
                MessageBox.Show("Student deleted.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ApplyFilter();
            }
        }
    }

    

    public class StudentDialog : Form
    {
        public Student Student { get; private set; }
        private TextBox txtID, txtFirst, txtLast, txtContact;
        private ComboBox cmbGender, cmbCourse, cmbYear;

        public StudentDialog(Student existing = null)
        {
            Student = existing;
            InitializeComponent();
            if (existing != null) Populate(existing);
        }

        private void InitializeComponent()
        {
            bool isEdit = Student != null;
            this.Text = isEdit ? "Edit Student" : "Add New Student";
            this.Size = new Size(440, 440);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);

            var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(41, 128, 185) };
            pnlTitle.Controls.Add(new Label
            {
                Text = isEdit ? "✏️  Edit Student Record" : "➕  Add New Student",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.White,
                AutoSize = true, Location = new Point(16, 14)
            });
            this.Controls.Add(pnlTitle);

            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 16, 24, 16) };
            int y = 16;

            void Row(string lbl, Control ctrl)
            {
                pnl.Controls.Add(new Label { Text = lbl, AutoSize = true, Location = new Point(0, y + 3), Font = new Font("Segoe UI", 9f) });
                ctrl.Location = new Point(138, y);
                ctrl.Width = 256;
                pnl.Controls.Add(ctrl);
                y += 40;
            }

            txtID = new TextBox { Text = isEdit ? "" : DataManager.GenerateStudentID() };
            if (isEdit) { txtID.ReadOnly = true; txtID.BackColor = Color.FromArgb(242, 244, 248); }
            Row("Student ID *", txtID);

            txtFirst = new TextBox(); Row("First Name *", txtFirst);
            txtLast  = new TextBox(); Row("Last Name *",  txtLast);

            cmbGender = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGender.Items.AddRange(new[] { "Male", "Female", "Other" });
            Row("Gender *", cmbGender);

            cmbCourse = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var c in DataManager.Courses) cmbCourse.Items.Add(c.CourseName);
            Row("Course *", cmbCourse);

            cmbYear = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cmbYear.Items.AddRange(new[] { "1st Year", "2nd Year", "3rd Year", "4th Year", "5th Year" });
            Row("Year Level *", cmbYear);

            txtContact = new TextBox(); Row("Contact Number", txtContact);

            var btnSave = new Button
            {
                Text = isEdit ? "💾 Update" : "💾 Save Student",
                Location = new Point(138, y + 8), Size = new Size(130, 34),
                BackColor = Color.FromArgb(41, 128, 185), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += Save;

            var btnCancel = new Button
            {
                Text = "Cancel", Location = new Point(278, y + 8), Size = new Size(90, 34),
                BackColor = Color.FromArgb(149, 165, 166), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            pnl.Controls.Add(btnSave);
            pnl.Controls.Add(btnCancel);
            this.Controls.Add(pnl);
        }

        private void Populate(Student s)
        {
            txtID.Text = s.StudentID;
            txtFirst.Text = s.FirstName;
            txtLast.Text  = s.LastName;
            cmbGender.SelectedItem = s.Gender;
            cmbCourse.SelectedItem = s.Course;
            cmbYear.SelectedItem   = s.YearLevel;
            txtContact.Text = s.ContactNumber;
        }

        private void Save(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text) || string.IsNullOrWhiteSpace(txtFirst.Text) ||
                string.IsNullOrWhiteSpace(txtLast.Text) || cmbGender.SelectedIndex < 0 ||
                cmbCourse.SelectedIndex < 0 || cmbYear.SelectedIndex < 0)
            {
                MessageBox.Show("Please fill in all required (*) fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Student = new Student
            {
                StudentID     = txtID.Text.Trim(),
                FirstName     = txtFirst.Text.Trim(),
                LastName      = txtLast.Text.Trim(),
                Gender        = cmbGender.SelectedItem.ToString(),
                Course        = cmbCourse.SelectedItem.ToString(),
                YearLevel     = cmbYear.SelectedItem.ToString(),
                ContactNumber = txtContact.Text.Trim()
            };
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
