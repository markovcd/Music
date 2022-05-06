namespace Presentation.Utility;

public readonly record struct ValidationResult
{
  public bool IsSuccess { get; }
  public string Message { get; }

  private ValidationResult(bool isSuccess, string message)
  {
    IsSuccess = isSuccess;
    Message = message;
  }

  public static ValidationResult Ok => new (true, string.Empty);
  
  public static ValidationResult Fail(string message)
  {
    return new ValidationResult(false, message);
  }
}