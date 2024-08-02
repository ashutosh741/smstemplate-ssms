// Controllers/msgController.cs
public class CreateUserRequest
{
    public string? UserName { get; internal set; }
    public object? Password { get; internal set; }
    public string? LastName { get; internal set; }
    public string? FirstName { get; internal set; }
}