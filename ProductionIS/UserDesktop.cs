using System.Drawing;
using System.Windows.Forms;

namespace ProductionIS.Forms
{
    public class UserDesktop : Form
    {
        public UserDesktop(string login)
        {
            Text          = $"ПроизводствоИС — Рабочий стол ({login})";
            Size          = new Size(800, 600);
            MinimumSize   = new Size(640, 480);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = Color.White;

            var lblWelcome = new Label
            {
                Text      = $"Добро пожаловать, {login}!",
                Left      = 30,
                Top       = 30,
                AutoSize  = true,
                Font      = new Font("Segoe UI", 14, FontStyle.Bold),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left
            };

            Controls.Add(lblWelcome);
        }
    }
}
