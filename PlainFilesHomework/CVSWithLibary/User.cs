namespace CVSWithLibary;

public class User
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool IsActive { get; set; }

    public override string ToString()
    {
        return $"{Username},{Password},{IsActive}";
    }
}
