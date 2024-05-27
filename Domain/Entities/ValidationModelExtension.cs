using System.ComponentModel.DataAnnotations;


namespace AmxBookstore.Domain.Entities
{
    public static class ValidationModelExtension
    {
        public static IList<ValidationResult> ValidateModel(this object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }
    }
}
