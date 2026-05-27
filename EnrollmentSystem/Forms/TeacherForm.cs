using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EnrollmentSystem.Data;
using EnrollmentSystem.Models;

namespace EnrollmentSystem.Forms
{
    public class TeacherForm : Form
    {
        private DataGridView dgv;
        private Button btnAdd, btnEdit, btnDelete;
        private Label lblCount;

        public TeacherForm()
        {
            InitializeComponent();
            LoadGrid();
        }

        private void InitializeComponent()
        {
            this.Text = "Teacher Management";
            this.Size = new Size(860, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9f);

            var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(211, 84, 0) };
            pnlTitle.Controls.Add(new Label
            {
                Text = "👨‍🏫  Teacher Management",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White, AutoSize = true, Location = new Point(18, 13)
            });
            this.Controls.Add(dgv);
            this.Controls.Add(pnlTitle);

            var pnlBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White, Padding = new Padding(10, 9, 10, 9) };
            btnAdd    = Btn("➕ Add Teacher",  Color.FromArgb(211, 84, 0),    10, 9, 120);
            btnEdit   = Btn("✏️ Edit",         Color.FromArgb(41, 128, 185), 140, 9, 80);
            btnDelete = Btn("🗑️ Delete",       Color.FromArgb(192, 57, 43),  228, 9, 90);
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
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(211, 84, 0);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(255, 250, 247);
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnEdit_Click(s, e); };

            var pnlStatus = new Panel { Dock = DockStyle.Bottom, Height = 28, BackColor = Color.FromArgb(240, 244, 248) };
            lblCount = new Label { AutoSize = true, Location = new Point(10, 7), ForeColor = Color.SlateGray, Font = new Font("Segoe UI", 8.5f) };
            var lblHint = new Label
            {
                Text = "ℹ  Default password for new teachers = their Teacher ID",
                AutoSize = true, Location = new Point(300, 7), ForeColor = Color.CadetBlue, Font = new Font("Segoe UI", 8f)
            };
            pnlStatus.Controls.Add(lblCount);
            pnlStatus.Controls.Add(lblHint);
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
            foreach (var h in new[] { "Teacher ID", "First Name", "Last Name", "Department", "Contact" })
                dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = h, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            foreach (var t in DataManager.Teachers.OrderBy(t => t.LastName))
                dgv.Rows.Add(t.TeacherID, t.FirstName, t.LastName, t.Department, t.ContactNumber);
            lblCount.Text = $"{DataManager.Teachers.Count} teacher(s) on file";
        }

        private Teacher SelectedTeacher()
        {
            if (dgv.SelectedRows.Count == 0) return null;
            return DataManager.GetTeacher(dgv.SelectedRows[0].Cells[0].Value?.ToString());
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using var dlg = new TeacherDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                if (!DataManager.AddTeacher(dlg.Teacher))
                    MessageBox.Show("Teacher ID already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else { MessageBox.Show("Teacher added!\n\nDefault login password = Teacher ID.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); LoadGrid(); }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var t = SelectedTeacher();
            if (t == null) { MessageBox.Show("Select a teacher to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            using var dlg = new TeacherDialog(t);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                DataManager.UpdateTeacher(dlg.Teacher);
                MessageBox.Show("Teacher updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadGrid();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var t = SelectedTeacher();
            if (t == null) { MessageBox.Show("Select a teacher to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            if (MessageBox.Show($"Delete teacher '{t.FullName}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                DataManager.DeleteTeacher(t.TeacherID);
                MessageBox.Show("Teacher deleted.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadGrid();
            }
        }
    }

    

    public class TeacherDialog : Form
    {
        public Teacher Teacher { get; private set; }
        private TextBox txtID, txtFirst, txtLast, txtDept, txtContact;

        public TeacherDialog(Teacher existing = null)
        {
            Teacher = existing;
            InitializeComponent();
            if (existing != null) Populate(existing);
        }

        private void InitializeComponent()
        {
            bool isEdit = Teacher != null;
            this.Text = isEdit ? "Edit Teacher" : "Add New Teacher";
            this.Size = new Size(430, 360);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);

            var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(211, 84, 0) };
            pnlTitle.Controls.Add(new Label
            {
                Text = isEdit ? "✏️  Edit Teacher" : "➕  Add New Teacher",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.White,
                AutoSize = true, Location = new Point(16, 14)
            });
            this.Controls.Add(pnlTitle);

            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24, 16, 24, 16) };
            int y = 16;
            void Row(string label, TextBox tb)
            {
                pnl.Controls.Add(new Label { Text = label, AutoSize = true, Location = new Point(0, y + 3) });
                tb.Location = new Point(140, y); tb.Width = 244;
                pnl.Controls.Add(tb); y += 38;
            }

            txtID      = new TextBox { Text = isEdit ? "" : DataManager.GenerateTeacherID() };
            if (isEdit) { txtID.ReadOnly = true; txtID.BackColor = Color.FromArgb(242, 244, 248); }
            txtFirst   = new TextBox();
            txtLast    = new TextBox();
            txtDept    = new TextBox();
            txtContact = new TextBox();

            Row("Teacher ID *",  txtID);
            Row("First Name *",  txtFirst);
            Row("Last Name *",   txtLast);
            Row("Department *",  txtDept);
            Row("Contact",       txtContact);

            var btnSave = new Button
            {
                Text = isEdit ? "💾 Update" : "💾 Save",
                Location = new Point(140, y + 8), Size = new Size(115, 34),
                BackColor = Color.FromArgb(211, 84, 0), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtID.Text) || string.IsNullOrWhiteSpace(txtFirst.Text) ||
                    string.IsNullOrWhiteSpace(txtLast.Text) || string.IsNullOrWhiteSpace(txtDept.Text))
                { MessageBox.Show("Please fill required fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                Teacher = new Teacher { TeacherID = txtID.Text.Trim(), FirstName = txtFirst.Text.Trim(), LastName = txtLast.Text.Trim(), Department = txtDept.Text.Trim(), ContactNumber = txtContact.Text.Trim() };
                this.DialogResult = DialogResult.OK; this.Close();
            };

            var btnCancel = new Button
            {
                Text = "Cancel", Location = new Point(265, y + 8), Size = new Size(90, 34),
                BackColor = Color.FromArgb(149, 165, 166), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            pnl.Controls.Add(btnSave); pnl.Controls.Add(btnCancel);
            this.Controls.Add(pnl);
        }

        private void Populate(Teacher t)
        {
            txtID.Text = t.TeacherID; txtFirst.Text = t.FirstName; txtLast.Text = t.LastName;
            txtDept.Text = t.Department; txtContact.Text = t.ContactNumber;
        }
    }
}
