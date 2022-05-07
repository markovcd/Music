namespace Presentation.Utility;

public interface IValidationRule<T>
{
  ValidationResult Validate(IBindable<T> property);
}