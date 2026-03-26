using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ProductionIS.Data;
using ProductionIS.ViewModels;

namespace ProductionIS.Forms
{
    public class AdminDesktop : Form
    {
        private readonly DataGridView grid;
        private readonly TextBox      txtNewLogin;
        private readonly TextBox      txtNewPassword;
        private readonly ComboBox     cmbNewRole;
        private readonly Label        lblMessage;

        public AdminDesktop(string login)
        {
            Text          = $"ПроизводствоИС — Администратор ({login})";
            Size          = new Size(960, 660);
            MinimumSize   = new Size(760, 520);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = Color.White;

            var lblTitle = new Label
            {
                Text   = "Управление пользователями",
                Left   = 16,
                Top    = 14,
                AutoSize = true,
                Font   = new Font("Segoe UI", 13, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            grid = new DataGridView
            {
                Left                    = 16,
                Top                     = 48,
                Width                   = ClientSize.Width - 32,
                Height                  = ClientSize.Height - 230,
                ReadOnly                = false,
                SelectionMode           = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode     = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows      = false,
                AllowUserToDeleteRows   = false,
                RowHeadersVisible       = false,
                Anchor                  = AnchorStyles.Top | AnchorStyles.Bottom
                                        | AnchorStyles.Left | AnchorStyles.Right
            };

            var pnlButtons = new Panel
            {
                Left   = 16,
                Height = 44,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            grid.SizeChanged += (_, _) => pnlButtons.Top = grid.Bottom + 8;
            pnlButtons.Top = grid.Bottom + 8;
            pnlButtons.Width = grid.Width;

            var btnBlock   = new Button { Text = "Заблокировать",  Width = 150, Height = 36, Left = 0,   TabIndex = 0 };
            var btnUnblock = new Button { Text = "Разблокировать", Width = 150, Height = 36, Left = 158, TabIndex = 1 };
            var btnSave    = new Button { Text = "Сохранить изм.", Width = 150, Height = 36, Left = 316, TabIndex = 2 };

            btnBlock.Click   += BtnBlock_Click;
            btnUnblock.Click += BtnUnblock_Click;
            btnSave.Click    += BtnSave_Click;

            pnlButtons.Controls.AddRange(new Control[] { btnBlock, btnUnblock, btnSave });

            var grpAdd = new GroupBox
            {
                Text   = "Добавить нового пользователя",
                Left   = 16,
                Height = 110,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            grpAdd.SizeChanged += (_, _) => grpAdd.Top = ClientSize.Height - grpAdd.Height - 8;
            grpAdd.Top = ClientSize.Height - grpAdd.Height - 8;
            grpAdd.Width = grid.Width;

            grpAdd.Controls.Add(new Label { Text = "Логин:",   Left = 12, Top = 28, AutoSize = true });
            txtNewLogin = new TextBox { Left = 70, Top = 24, Width = 160, MaxLength = 50, TabIndex = 3 };
            grpAdd.Controls.Add(txtNewLogin);

            grpAdd.Controls.Add(new Label { Text = "Пароль:", Left = 244, Top = 28, AutoSize = true });
            txtNewPassword = new TextBox
            {
                Left                 = 304,
                Top                  = 24,
                Width                = 160,
                UseSystemPasswordChar = true,
                MaxLength            = 100,
                TabIndex             = 4
            };
            grpAdd.Controls.Add(txtNewPassword);

            grpAdd.Controls.Add(new Label { Text = "Роль:", Left = 478, Top = 28, AutoSize = true });
            cmbNewRole = new ComboBox
            {
                Left          = 518,
                Top           = 24,
                Width         = 100,
                DropDownStyle = ComboBoxStyle.DropDownList,
                TabIndex      = 5
            };
            cmbNewRole.Items.AddRange(new object[] { "user", "admin" });
            cmbNewRole.SelectedIndex = 0;
            grpAdd.Controls.Add(cmbNewRole);

            var btnAdd = new Button { Text = "Добавить", Left = 634, Top = 20, Width = 110, Height = 36, TabIndex = 6 };
            btnAdd.Click += BtnAdd_Click;
            grpAdd.Controls.Add(btnAdd);

            lblMessage = new Label
            {
                Left      = 12,
                Top       = 72,
                Width     = 740,
                AutoSize  = false,
                ForeColor = Color.Crimson
            };
            grpAdd.Controls.Add(lblMessage);

            Controls.AddRange(new Control[] { lblTitle, grid, pnlButtons, grpAdd });

            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                var users = UserRepository.GetAllUsers()
                    .Select(u => new UserViewModel
                    {
                        Id             = u.Id,
                        Login          = u.Login,
                        Role           = u.Role,
                        IsBlocked      = u.IsBlocked,
                        FailedAttempts = u.FailedAttempts
                    }).ToList();

                grid.DataSource = null;
                grid.DataSource = users;

                grid.Columns["Id"].ReadOnly             = true;
                grid.Columns["FailedAttempts"].ReadOnly = true;
                grid.Columns["IsBlocked"].ReadOnly      = true;

                int roleIndex = grid.Columns["Role"].Index;
                var roleColumn = new DataGridViewComboBoxColumn
                {
                    DataPropertyName = "Role",
                    HeaderText       = "Роль",
                    Name             = "Role",
                    DataSource       = new[] { "user", "admin" }
                };
                grid.Columns.RemoveAt(roleIndex);
                grid.Columns.Insert(roleIndex, roleColumn);

                grid.Columns["Id"].HeaderText             = "ID";
                grid.Columns["Login"].HeaderText          = "Логин";
                grid.Columns["IsBlocked"].HeaderText      = "Заблокирован";
                grid.Columns["FailedAttempts"].HeaderText = "Неудачных входов";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось загрузить список пользователей:\n{ex.Message}",
                    "Ошибка загрузки",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnBlock_Click(object? sender, EventArgs e)
        {
            if (!TryGetSelectedId(out int id)) return;
            try
            {
                UserRepository.BlockUser(id);
                ShowMessage("Пользователь заблокирован.", success: true);
                LoadUsers();
            }
            catch (Exception ex) { ShowDbError(ex); }
        }

        private void BtnUnblock_Click(object? sender, EventArgs e)
        {
            if (!TryGetSelectedId(out int id)) return;
            try
            {
                UserRepository.UnblockUser(id);
                ShowMessage("Пользователь разблокирован.", success: true);
                LoadUsers();
            }
            catch (Exception ex) { ShowDbError(ex); }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (grid.SelectedRows.Count == 0)
            {
                ShowMessage("Выберите строку для сохранения.");
                return;
            }

            var row   = grid.SelectedRows[0];
            int id    = (int)row.Cells["Id"].Value;
            string lg = row.Cells["Login"].Value?.ToString()?.Trim() ?? "";
            string rl = row.Cells["Role"].Value?.ToString() ?? "user";

            if (string.IsNullOrEmpty(lg))
            {
                ShowMessage("Логин не может быть пустым.");
                return;
            }

            try
            {
                UserRepository.UpdateUser(id, lg, rl);
                ShowMessage("Данные пользователя обновлены.", success: true);
                LoadUsers();
            }
            catch (Exception ex) { ShowDbError(ex); }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            string login    = txtNewLogin.Text.Trim();
            string password = txtNewPassword.Text;
            string role     = cmbNewRole.SelectedItem?.ToString() ?? "user";

            if (string.IsNullOrEmpty(login))
            {
                ShowMessage("Введите логин нового пользователя.");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowMessage("Введите пароль нового пользователя.");
                return;
            }

            if (login.Length < 3)
            {
                ShowMessage("Логин должен содержать не менее 3 символов.");
                return;
            }

            try
            {
                if (UserRepository.LoginExists(login))
                {
                    ShowMessage($"Пользователь с логином «{login}» уже существует.");
                    return;
                }

                UserRepository.AddUser(login, password, role);
                ShowMessage($"Пользователь «{login}» успешно добавлен.", success: true);
                txtNewLogin.Clear();
                txtNewPassword.Clear();
                LoadUsers();
            }
            catch (Exception ex) { ShowDbError(ex); }
        }

        private bool TryGetSelectedId(out int id)
        {
            id = 0;
            if (grid.SelectedRows.Count == 0)
            {
                ShowMessage("Выберите пользователя в таблице.");
                return false;
            }
            id = (int)grid.SelectedRows[0].Cells["Id"].Value;
            return true;
        }

        private void ShowMessage(string text, bool success = false)
        {
            lblMessage.ForeColor = success ? Color.Green : Color.Crimson;
            lblMessage.Text      = text;
        }

        private void ShowDbError(Exception ex)
        {
            MessageBox.Show(
                $"Ошибка при обращении к базе данных:\n{ex.Message}",
                "Ошибка БД",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
