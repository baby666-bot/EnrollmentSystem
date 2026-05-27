using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EnrollmentSystem.Data;
using EnrollmentSystem.Models;

namespace EnrollmentSystem.Forms
{
    public class CourseForm : Form
    {
        private DataGridView dgv;
        private Button btnAdd, btnEdit, btnDelete;
        private Label lblCount;

        public CourseForm()
        {
            InitializeComponent();
            LoadGrid();
        }

        private void InitializeComponent()
        {
            this.Text = "Course Management";
            this.Size = new Size(820, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9f);

            var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(39, 174, 96) };
            pnlTitle.Controls.Add(new Label
            {
                Text = "📚  Course Management",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(18, 13)
            });
            this.Controls.Add(dgv);
            this.Controls.Add(pnlTitle);

            var pnlBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White, Padding = new Padding(10, 9, 10, 9) };
            btnAdd    = Btn("➕ Add Course",  Color.FromArgb(39, 174, 96),  10, 9, 120);
            btnEdit   = Btn("✏️ Edit",        Color.FromArgb(41, 128, 185), 140, 9, 80);
            btnDelete = Btn("🗑️ Delete",      Color.FromArgb(192, 57, 43),  228, 9, 90);
            btnAdd.Click += BtnAdd_Click; btnEdit.Click += BtnEdit_Click; btnDelete.Click += BtnDelete_Click;
            pnlBar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });
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
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(39, 174, 96);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 252, 249);
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };

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

        private void LoadGrid()
        {
            dgv.Rows.Clear();
            dgv.Columns.Clear();
            foreach (var h in new[] { "Course ID", "Course Name", "Department", "Enrolled" })
                dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = h, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

            foreach (var c in DataManager.Courses.OrderBy(c => c.CourseID))
            {
                int enrolled = DataManager.Enrollments.Count(e => e.CourseID == c.CourseID && e.Status == "Enrolled");
                dgv.Rows.Add(c.CourseID, c.CourseName, c.Department, enrolled);
            }
            lblCount.Text = $"{DataManager.Courses.Count} course(s) on file";
        }

        private Course SelectedCourse()
        {
            if (dgv.SelectedRows.Count == 0) return null;
            return DataManager.GetCourse(dgv.SelectedRows[0].Cells[0].Value?.ToString());
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using var dlg = new CourseDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                if (!DataManager.AddCourse(dlg.Course))
                    MessageBox.Show("Course ID already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else { MessageBox.Show("Course added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); LoadGrid(); }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var c = SelectedCourse();
            if (c == null) { MessageBox.Show("Select a course to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            using var dlg = new CourseDialog(c);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                DataManager.UpdateCourse(dlg.Course);
                MessageBox.Show("Course updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadGrid();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var c = SelectedCourse();
            if (c == null) { MessageBox.Show("Select a course to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show($"Delete '{c.CourseName}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (!DataManager.DeleteCourse(c.CourseID))
                    MessageBox.Show("Cannot delete — course has existing enrollment records.", "Cannot Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else { MessageBox.Show("Course deleted.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information); LoadGrid(); }
            }
        }
    }

    

    public class CourseDialog : Form
    {
        public Course Course { get; private set; }
        private TextBox txtID, txtName, txtDept;

        public CourseDialog(Course existing = null)
        {
            Course = existing;
            InitializeComponent();
            if (existing != null) Populate(existing);
        }

        private void InitializeComponent()
        {
            bool isEdit = Course != null;
            this.Text = isEdit ? "Edit Course" : "Add New Course";
            this.Size = new Size(420, 290);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);

            var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(39, 174, 96) };
            pnlTitle.Controls.Add(new Label
            {
                Text = isEdit ? "✏️  Edit Course" : "➕  Add New Course",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.White,
                AutoSize = true, Location = new Point(16, 14)
            });
            this.Controls.Add(pnlTitle);

            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 16, 24, 16) };
            int y = 16;
            void Row(string label, TextBox tb)
            {
                pnl.Controls.Add(new Label { Text = label, AutoSize = true, Location = new Point(0, y + 3) });
                tb.Location = new Point(130, y); tb.Width = 244;
                pnl.Controls.Add(tb); y += 40;
            }

            txtID   = new TextBox { Text = isEdit ? "" : DataManager.GenerateCourseID() };
            if (isEdit) { txtID.ReadOnly = true; txtID.BackColor = Color.FromArgb(242, 244, 248); }
            txtName = new TextBox();
            txtDept = new TextBox();
            Row("Course ID *",   txtID);
            Row("Course Name *", txtName);
            Row("Department *",  txtDept);

            var btnSave = new Button
            {
                Text = isEdit ? "💾 Update" : "💾 Save",
                Location = new Point(130, y + 8), Size = new Size(115, 34),
                BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtID.Text) || string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtDept.Text))
                { MessageBox.Show("All fields are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                Course = new Course { CourseID = txtID.Text.Trim(), CourseName = txtName.Text.Trim(), Department = txtDept.Text.Trim() };
                this.DialogResult = DialogResult.OK; this.Close();
            };

            var btnCancel = new Button
            {
                Text = "Cancel", Location = new Point(255, y + 8), Size = new Size(90, 34),
                BackColor = Color.FromArgb(149, 165, 166), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            pnl.Controls.Add(btnSave); pnl.Controls.Add(btnCancel);
            this.Controls.Add(pnl);
        }

        private void Populate(Course c) { txtID.Text = c.CourseID; txtName.Text = c.CourseName; txtDept.Text = c.Department; }
    }
}
