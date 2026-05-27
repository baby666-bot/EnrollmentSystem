using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EnrollmentSystem.Data;
using EnrollmentSystem.Models;

namespace EnrollmentSystem.Forms
{
    public class EnrollmentForm : Form
    {
        private DataGridView dgv;
        private TextBox txtSearch;
        private ComboBox cmbFilterStatus;
        private Button btnEnroll, btnEdit, btnDelete;
        private Label lblCount;

        public EnrollmentForm()
        {
            InitializeComponent();
            LoadGrid();
        }

        private void InitializeComponent()
        {
            this.Text = "Enrollment Management";
            this.Size = new Size(1060, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9f);

            var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(142, 68, 173) };
            pnlTitle.Controls.Add(new Label
            {
                Text = "📋  Enrollment Management",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(18, 13)
            });
            this.Controls.Add(dgv);
            this.Controls.Add(pnlTitle);

            var pnlBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White, Padding = new Padding(10, 9, 10, 9) };
            pnlBar.Controls.Add(new Label { Text = "Search:", AutoSize = true, Location = new Point(10, 17) });
            txtSearch = new TextBox { Location = new Point(66, 13), Width = 200 };
            txtSearch.TextChanged += (s, e) => ApplyFilter();

            pnlBar.Controls.Add(new Label { Text = "Status:", AutoSize = true, Location = new Point(282, 17) });
            cmbFilterStatus = new ComboBox { Location = new Point(332, 13), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbFilterStatus.Items.AddRange(new[] { "All", "Enrolled", "Dropped", "Completed", "Pending" });
            cmbFilterStatus.SelectedIndex = 0;
            cmbFilterStatus.SelectedIndexChanged += (s, e) => ApplyFilter();

            btnEnroll = Btn("📋 Enroll Student", Color.FromArgb(142, 68, 173), 478, 9, 145);
            btnEdit   = Btn("✏️ Edit Status",    Color.FromArgb(41, 128, 185),  632, 9, 112);
            btnDelete = Btn("🗑️ Delete",         Color.FromArgb(192, 57, 43),   753, 9, 88);

            btnEnroll.Click += BtnEnroll_Click;
            btnEdit.Click   += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;

            pnlBar.Controls.AddRange(new Control[] { txtSearch, cmbFilterStatus, btnEnroll, btnEdit, btnDelete });
            this.Controls.Add(pnlBar);

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                RowHeadersVisible = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9f), GridColor = Color.FromArgb(228, 232, 240),
                ColumnHeadersHeight = 36, RowTemplate = { Height = 30 }
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(142, 68, 173);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 248, 254);
            dgv.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex != 5 || e.RowIndex < 0) return;
                e.CellStyle.ForeColor = e.Value?.ToString() switch
                {
                    "Enrolled"  => Color.FromArgb(39, 174, 96),
                    "Dropped"   => Color.FromArgb(192, 57, 43),
                    "Completed" => Color.FromArgb(41, 128, 185),
                    "Pending"   => Color.FromArgb(230, 126, 34),
                    _           => Color.Gray
                };
                e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            };

            var pnlStatus = new Panel { Dock = DockStyle.Bottom, Height = 28, BackColor = Color.FromArgb(240, 244, 248) };
            lblCount = new Label { AutoSize = true, Location = new Point(10, 7), ForeColor = Color.SlateGray, Font = new Font("Segoe UI", 8.5f) };
            pnlStatus.Controls.Add(lblCount);
            this.Controls.Add(pnlStatus);
        }

        private Button Btn(string text, Color color, int x, int y, int w)
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

        private void LoadGrid() => BindGrid(DataManager.Enrollments);

        private void ApplyFilter()
        {
            string search = txtSearch.Text.Trim();
            string status = cmbFilterStatus.SelectedIndex > 0 ? cmbFilterStatus.SelectedItem?.ToString() : null;
            var list = (string.IsNullOrEmpty(search) ? DataManager.Enrollments.AsEnumerable() : DataManager.SearchEnrollments(search));
            if (!string.IsNullOrEmpty(status)) list = list.Where(e => e.Status == status);
            BindGrid(list.ToList());
        }

        private void BindGrid(System.Collections.Generic.List<Enrollment> list)
        {
            dgv.Rows.Clear();
            dgv.Columns.Clear();
            foreach (var h in new[] { "Enrollment ID", "Student ID", "Student Name", "Course", "Date", "Status" })
                dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = h, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

            foreach (var e in list)
            {
                var student = DataManager.GetStudent(e.StudentID);
                var course  = DataManager.GetCourse(e.CourseID);
                dgv.Rows.Add(e.EnrollmentID, e.StudentID, student?.FullName ?? e.StudentID, course?.CourseName ?? e.CourseID, e.EnrollmentDate, e.Status);
            }
            lblCount.Text = $"Showing {list.Count} of {DataManager.Enrollments.Count} enrollment(s)";
        }

        private string SelectedID() => dgv.SelectedRows.Count == 0 ? null : dgv.SelectedRows[0].Cells[0].Value?.ToString();

        private void BtnEnroll_Click(object sender, EventArgs e)
        {
            if (!DataManager.Students.Any()) { MessageBox.Show("Add students first.", "No Students", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!DataManager.Courses.Any())  { MessageBox.Show("Add courses first.", "No Courses",  MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            using var dlg = new EnrollmentDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                if (!DataManager.AddEnrollment(dlg.Enrollment))
                    MessageBox.Show("Enrollment ID exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else { MessageBox.Show("Student enrolled successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); ApplyFilter(); }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var id = SelectedID();
            if (id == null) { MessageBox.Show("Select an enrollment to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            var enr = DataManager.Enrollments.FirstOrDefault(x => x.EnrollmentID == id);
            if (enr == null) return;
            using var dlg = new EnrollmentDialog(enr);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                DataManager.UpdateEnrollment(dlg.Enrollment);
                MessageBox.Show("Enrollment updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ApplyFilter();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var id = SelectedID();
            if (id == null) { MessageBox.Show("Select an enrollment to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show($"Delete enrollment '{id}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DataManager.DeleteEnrollment(id);
                MessageBox.Show("Enrollment deleted.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ApplyFilter();
            }
        }
    }

   

    public class EnrollmentDialog : Form
    {
        public Enrollment Enrollment { get; private set; }
        private TextBox txtID, txtDate;
        private ComboBox cmbStudent, cmbCourse, cmbStatus;

        public EnrollmentDialog(Enrollment existing = null)
        {
            Enrollment = existing;
            InitializeComponent();
            if (existing != null) Populate(existing);
        }

        private void InitializeComponent()
        {
            bool isEdit = Enrollment != null;
            this.Text = isEdit ? "Edit Enrollment" : "Enroll Student";
            this.Size = new Size(460, 370);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);

            var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(142, 68, 173) };
            pnlTitle.Controls.Add(new Label
            {
                Text = isEdit ? "✏️  Edit Enrollment" : "📋  Enroll Student",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.White,
                AutoSize = true, Location = new Point(16, 14)
            });
            this.Controls.Add(pnlTitle);

            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 16, 24, 16) };
            int y = 16;

            void Row(string label, Control ctrl)
            {
                pnl.Controls.Add(new Label { Text = label, AutoSize = true, Location = new Point(0, y + 3) });
                ctrl.Location = new Point(155, y);
                if (ctrl is TextBox tb) tb.Width = 256;
                else if (ctrl is ComboBox cb) { cb.Width = 256; cb.DropDownStyle = ComboBoxStyle.DropDownList; }
                pnl.Controls.Add(ctrl);
                y += 40;
            }

            txtID = new TextBox { Text = DataManager.GenerateEnrollmentID() };
            if (isEdit) { txtID.ReadOnly = true; txtID.BackColor = Color.FromArgb(242, 244, 248); }
            Row("Enrollment ID *", txtID);

            cmbStudent = new ComboBox();
            foreach (var s in DataManager.Students.OrderBy(s => s.LastName))
                cmbStudent.Items.Add($"{s.StudentID} – {s.FullName}");
            if (isEdit) cmbStudent.Enabled = false;
            Row("Student *", cmbStudent);

            cmbCourse = new ComboBox();
            foreach (var c in DataManager.Courses.OrderBy(c => c.CourseName))
                cmbCourse.Items.Add($"{c.CourseID} – {c.CourseName}");
            if (isEdit) cmbCourse.Enabled = false;
            Row("Course *", cmbCourse);

            txtDate = new TextBox { Text = DateTime.Now.ToString("MM/dd/yyyy") };
            Row("Enrollment Date *", txtDate);

            cmbStatus = new ComboBox();
            cmbStatus.Items.AddRange(new[] { "Enrolled", "Pending", "Dropped", "Completed" });
            cmbStatus.SelectedIndex = 0;
            Row("Status *", cmbStatus);

            var btnSave = new Button
            {
                Text = isEdit ? "💾 Update" : "💾 Enroll",
                Location = new Point(155, y + 8), Size = new Size(120, 34),
                BackColor = Color.FromArgb(142, 68, 173), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += Save;

            var btnCancel = new Button
            {
                Text = "Cancel", Location = new Point(285, y + 8), Size = new Size(90, 34),
                BackColor = Color.FromArgb(149, 165, 166), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            pnl.Controls.Add(btnSave); pnl.Controls.Add(btnCancel);
            this.Controls.Add(pnl);
        }

        private void Populate(Enrollment e)
        {
            txtID.Text = e.EnrollmentID;
            for (int i = 0; i < cmbStudent.Items.Count; i++)
                if (cmbStudent.Items[i].ToString().StartsWith(e.StudentID)) { cmbStudent.SelectedIndex = i; break; }
            for (int i = 0; i < cmbCourse.Items.Count; i++)
                if (cmbCourse.Items[i].ToString().StartsWith(e.CourseID)) { cmbCourse.SelectedIndex = i; break; }
            txtDate.Text = e.EnrollmentDate;
            cmbStatus.SelectedItem = e.Status;
        }

        private void Save(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text) || cmbStudent.SelectedIndex < 0 ||
                cmbCourse.SelectedIndex < 0 || string.IsNullOrWhiteSpace(txtDate.Text) || cmbStatus.SelectedIndex < 0)
            { MessageBox.Show("Please fill in all required fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            string studentID = cmbStudent.SelectedItem.ToString().Split('–')[0].Trim();
            string courseID  = cmbCourse.SelectedItem.ToString().Split('–')[0].Trim();
            Enrollment = new Enrollment
            {
                EnrollmentID   = txtID.Text.Trim(),
                StudentID      = studentID,
                CourseID       = courseID,
                EnrollmentDate = txtDate.Text.Trim(),
                Status         = cmbStatus.SelectedItem.ToString()
            };
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
