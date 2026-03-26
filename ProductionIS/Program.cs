using System;
using System.Windows.Forms;
using Npgsql;
using ProductionIS.Config;
using ProductionIS.Forms;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.ThreadException += (_, e) => ShowFatalError(e.Exception);

        if (!CheckDatabaseConnection()) return;

        Application.Run(new LoginForm());
    }

    private static bool CheckDatabaseConnection()
    {
        try
        {
            using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT 1", conn);
            cmd.ExecuteScalar();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось подключиться к базе данных.\n\nПроверьте параметры подключения и доступность сервера.\n\nПодробности: {ex.Message}",
                "Ошибка подключения",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }
    }

    private static void ShowFatalError(Exception ex)
    {
        MessageBox.Show(
            $"Произошла непредвиденная ошибка.\n\n{ex.Message}",
            "Критическая ошибка",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
