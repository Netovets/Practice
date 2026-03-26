using System;
using System.Drawing;
using System.Windows.Forms;
using ProductionIS.Data;

namespace ProductionIS.Forms
{
    public class LoginForm : Form
    {
        private readonly Label    lblLoginCaption;
        private readonly TextBox  txtLogin;
        private readonly Label    lblPasswordCaption;
        private readonly TextBox  txtPassword;
        private readonly Button   btnLogin;
        private readonly Label    lblStatus;

        public LoginForm()
        {
            Text            = "ПроизводствоИС — Вход в систему";
            Size            = new Size(420, 280);
            MinimumSize     = new Size(420, 280);
            MaximumSize     = new Size(420, 280);  
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;

            lblLoginCaption = new Label
            {
                Text     = "Логин:",
                Left     = 24,
                Top      = 24,
                AutoSize = true
            };

            txtLogin = new TextBox
            {
                Left      = 24,
                Top       = 44,
                Width     = 356,
                MaxLength = 50,
                TabIndex  = 0
            };

            lblPasswordCaption = new Label
            {
                Text     = "Пароль:",
                Left     = 24,
                Top      = 80,
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Left         = 24,
                Top          = 100,
                Width        = 356,
                PasswordChar = '●',
                MaxLength    = 100,
                TabIndex     = 1
            };

            btnLogin = new Button
            {
                Text     = "Войти",
                Left     = 24,
                Top      = 148,
                Width    = 356,
                Height   = 40,
                TabIndex = 2
            };
            btnLogin.Click += BtnLogin_Click;

            lblStatus = new Label
            {
                Left      = 24,
                Top       = 200,
                Width     = 356,
                Height    = 40,
                ForeColor = Color.Crimson
            };

            txtPassword.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter) BtnLogin_Click(null, EventArgs.Empty);
            };

            Controls.AddRange(new Control[]
            {
                lblLoginCaption, txtLogin,
                lblPasswordCaption, txtPassword,
                btnLogin, lblStatus
            });
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            lblStatus.Text = "";
            string login    = txtLogin.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                lblStatus.Text = "Введите логин и пароль.";
                return;
            }

            try
            {
                var user = UserRepository.FindByLogin(login);

                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    lblStatus.Text = "Неверный логин или пароль.";
                    if (user != null)
                    {
                        UserRepository.IncrementFailedAttempts(user.Id);

                        if (user.FailedAttempts + 1 >= 3)
                        {
                            UserRepository.BlockUser(user.Id);
                            lblStatus.Text = "Превышено число попыток. Учётная запись заблокирована.\nОбратитесь к администратору.";
                        }
                    }
                    return;
                }

                if (user.IsBlocked)
                {
                    lblStatus.Text = "Учётная запись заблокирована.\nОбратитесь к администратору.";
                    return;
                }

                UserRepository.ResetFailedAttempts(user.Id);
                Hide();

                if (user.Role == "admin")
                {
                    var desktop = new AdminDesktop(user.Login);
                    desktop.FormClosed += (_, _) => Application.Exit();
                    desktop.Show();
                }
                else
                {
                    var captchaForm = new UserCaptchaForm(user);
                    captchaForm.FormClosed += (_, _) => Application.Exit();
                    captchaForm.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при попытке входа:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
