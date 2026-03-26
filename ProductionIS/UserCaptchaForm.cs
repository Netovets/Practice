using System;
using System.Drawing;
using System.Windows.Forms;
using ProductionIS.Controls;
using ProductionIS.Data;
using ProductionIS.Entities;

namespace ProductionIS.Forms
{
    public class UserCaptchaForm : Form
    {
        private readonly PuzzleCaptcha puzzleCaptcha;
        private readonly Button        btnContinue;
        private readonly User          currentUser;
        private          int           failedCaptchaCount;

        public UserCaptchaForm(User user)
        {
            currentUser = user;
            Text            = $"ПроизводствоИС — Проверка (капча)";
            Size            = new Size(460, 520);
            MinimumSize     = new Size(460, 520);
            MaximumSize     = new Size(460, 520);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;

            var lblInstruction = new Label
            {
                Text      = "Для входа расставьте фрагменты изображения\nв правильном порядке:",
                Left      = 20,
                Top       = 16,
                Width     = 400,
                Height    = 40,
                AutoSize  = false
            };

            puzzleCaptcha = new PuzzleCaptcha
            {
                Left = 20,
                Top  = 64
            };

            btnContinue = new Button
            {
                Text     = "Продолжить",
                Left     = 20,
                Top      = puzzleCaptcha.Bottom + 16,
                Width    = puzzleCaptcha.Width,
                Height   = 40,
                TabIndex = 0
            };
            btnContinue.Click += BtnContinue_Click;

            Controls.AddRange(new Control[] { lblInstruction, puzzleCaptcha, btnContinue });

            ClientSize = new Size(460, btnContinue.Bottom + 20);
            MinimumSize = Size;
            MaximumSize = Size;
        }

        private void BtnContinue_Click(object? sender, EventArgs e)
        {
            if (!puzzleCaptcha.IsSolved)
            {
                failedCaptchaCount++;

                if (failedCaptchaCount >= 3)
                {
                    try { UserRepository.BlockUser(currentUser.Id); } catch { }
                    MessageBox.Show(
                        "Превышено число попыток прохождения капчи.\nУчётная запись заблокирована. Обратитесь к администратору.",
                        "Учётная запись заблокирована",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Close();
                    return;
                }

                int remaining = 3 - failedCaptchaCount;
                MessageBox.Show(
                    $"Изображение собрано неверно.\nОсталось попыток: {remaining}.",
                    "Неверно",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                puzzleCaptcha.Reset();
                return;
            }

            try
            {
                UserRepository.ResetFailedAttempts(currentUser.Id);
                Hide();
                var desktop = new UserDesktop(currentUser.Login);
                desktop.FormClosed += (_, _) => Application.Exit();
                desktop.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при открытии рабочего стола:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
