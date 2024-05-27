using System.ComponentModel.DataAnnotations.Schema;


namespace AmxBookstore.Domain.Entities
{
    public class ExtendedBaseModel
    {
        [NotMapped]
        public IList<string> ErrorList { get; protected set; } = new List<string>();

        [NotMapped]
        public bool IsInvalid { get; private set; }

        public void AddError(string message)
        {
            if (!ErrorList.Any())
                ErrorList.Add(message);
            else if (ErrorList.Any(x => x != message))
                ErrorList.Add(message);

            IsInvalid = ErrorList.Count > 0;
        }

        public void AddError(IList<string> messages)
        {
            foreach (var message in messages)
                AddError(message);

            IsInvalid = ErrorList.Count > 0;
        }
        public virtual void Validate()
        {
            var validationResult = this.ValidateModel();

            if (validationResult.Any())
                foreach (var item in validationResult) AddError(item.ErrorMessage);
        }
    }
}
