using System;
using System.Drawing;
using System.Windows.Forms;
using EnrollmentSystem.Data;

namespace EnrollmentSystem.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtID, txtPassword;
        private Label lblError;
        private CheckBox chkShow;
        private Button btnLogin;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text            = "Enrollment System — Login";
            this.Size            = new Size(460, 560);
            this.MinimumSize     = new Size(460, 560);
            this.MaximumSize     = new Size(460, 560);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.BackColor       = Color.White;
            this.Font            = new Font("Segoe UI", 9.5f);

            
            var pnlBanner = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(460, 150),
                BackColor = Color.FromArgb(22, 44, 76)
            };

            
            pnlBanner.Controls.Add(new Label
            {
                Text      = "📘",
                Font      = new Font("Segoe UI", 32f),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(195, 14)
            });

            
            pnlBanner.Controls.Add(new Label
            {
                Text      = "Enrollment System",
                Font      = new Font("Segoe UI", 17f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(108, 78)
            });

            
            pnlBanner.Controls.Add(new Label
            {
                Text      = "Please sign in to continue",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(170, 205, 240),
                AutoSize  = true,
                Location  = new Point(142, 116)
            });

            this.Controls.Add(pnlBanner);

            
            int lx = 50;   
            int fw = 360;  
            int y  = 168;  

            
            this.Controls.Add(new Label
            {
                Text      = "User ID",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 65, 90),
                AutoSize  = true,
                Location  = new Point(lx, y)
            });
            y += 22;

            
            txtID = new TextBox
            {
                Location    = new Point(lx, y),
                Size        = new Size(fw, 32),
                Font        = new Font("Segoe UI", 11f),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtID.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; txtPassword.Focus(); } };
            this.Controls.Add(txtID);
            y += 44;

            
            this.Controls.Add(new Label
            {
                Text      = "Password",
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 65, 90),
                AutoSize  = true,
                Location  = new Point(lx, y)
            });
            y += 22;

            
            txtPassword = new TextBox
            {
                Location              = new Point(lx, y),
                Size                  = new Size(fw, 32),
                Font                  = new Font("Segoe UI", 11f),
                BorderStyle           = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };
            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BtnLogin_Click(null, null); } };
            this.Controls.Add(txtPassword);
            y += 40;

            
            chkShow = new CheckBox
            {
                Text      = "Show password",
                Location  = new Point(lx, y),
                AutoSize  = true,
                ForeColor = Color.FromArgb(110, 120, 140),
                Font      = new Font("Segoe UI", 8.5f)
            };
            chkShow.CheckedChanged += (s, e) =>
                txtPassword.UseSystemPasswordChar = !chkShow.Checked;
            this.Controls.Add(chkShow);
            y += 36;

            
            lblError = new Label
            {
                Text      = "",
                Location  = new Point(lx, y),
                Size      = new Size(fw, 22),
                ForeColor = Color.FromArgb(192, 57, 43),
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            this.Controls.Add(lblError);
            y += 26;

            
            btnLogin = new Button
            {
                Text      = "Sign In",
                Location  = new Point(lx, y),
                Size      = new Size(fw, 44),
                BackColor = Color.FromArgb(22, 44, 76),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);
            y += 54;

            
            var divider = new Panel
            {
                Location  = new Point(lx, y),
                Size      = new Size(fw, 1),
                BackColor = Color.FromArgb(220, 225, 235)
            };
            this.Controls.Add(divider);
            y += 14;

            
            var pnlHint = new Panel
            {
                Location  = new Point(lx, y),
                Size      = new Size(fw, 64),
                BackColor = Color.FromArgb(245, 248, 252)
            };
            pnlHint.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(210, 220, 235));
                e.Graphics.DrawRectangle(pen, 0, 0, pnlHint.Width - 1, pnlHint.Height - 1);
            };
            pnlHint.Controls.Add(new Label
            {
                Text      = "ℹ  Default Login Credentials",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize  = true,
                Location  = new Point(10, 8)
            });
            pnlHint.Controls.Add(new Label
            {
                Text      = "Admin: ADMIN  /  admin123\nStudent or Teacher: use your ID as both username and password",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(90, 100, 120),
                AutoSize  = true,
                Location  = new Point(10, 28)
            });
            this.Controls.Add(pnlHint);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Text = "";

            string id  = txtID.Text.Trim();
            string pwd = txtPassword.Text;

            if (string.IsNullOrEmpty(id))
            {
                lblError.Text = "⚠  Please enter your User ID.";
                txtID.Focus();
                return;
            }
            if (string.IsNullOrEmpty(pwd))
            {
                lblError.Text = "⚠  Please enter your Password.";
                txtPassword.Focus();
                return;
            }

            var user = DataManager.Login(id, pwd);
            if (user == null)
            {
                lblError.Text = "⚠  Invalid User ID or Password.";
                txtPassword.Clear();
                txtPassword.Focus();
                return;
            }

            DataManager.CurrentUser = user;
            this.Hide();

            Form dashboard = user.Role switch
            {
                "Admin"   => (Form)new MainForm(),
                "Teacher" => new TeacherDashboard(),
                "Student" => new StudentDashboard(),
                _         => new MainForm()
            };

            dashboard.FormClosed += (s2, e2) => this.Close();
            dashboard.Show();
        }
    }
}
