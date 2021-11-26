using System.ComponentModel.DataAnnotations;

namespace ModelMinimalValidator
{
    public interface IMinimalValidator
    {
        ValidationResult Validate<T>(T model);
    }
}