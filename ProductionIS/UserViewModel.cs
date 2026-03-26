namespace ProductionIS.ViewModels
{
    // ViewModel для привязки к DataGridView
    public class UserViewModel
    {
        public int    Id             { get; set; }
        public string Login          { get; set; } = "";
        public string Role           { get; set; } = "";
        public bool   IsBlocked      { get; set; }
        public int    FailedAttempts { get; set; }
    }
}
