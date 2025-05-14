using tugasmodul7.Models;

public class UserWithDetail
{
    public decimal Saldo { get; set; }
    public decimal Hutang { get; set; }
}

    public class UserDetail: User
{
    public UserWithDetail? UserWithDetail { get; set; }
}
