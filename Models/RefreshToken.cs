namespace CourseProject.Models;

public class RefreshToken
{
    public long Id { get; set; }

    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public string Token { get; set; } = "";
    public DateTime ExpiresAt { get; set; }

    public bool Revoked { get; set; } = false;
}
