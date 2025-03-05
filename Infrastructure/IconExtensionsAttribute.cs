using System.ComponentModel.DataAnnotations;

namespace Scribe.Infrastructure
{
    public class IconExtensionAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            //var _context = (CmsShoppingCartContext)validationContext.GetService(typeof(CmsShoppingCartContext));    

            var file = value as IFormFile;
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);

                string[] extensions =
                {
                    "svg", "ico"
                };
                bool result = extension.Any(x => extension.EndsWith(x));

                if (!result)
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }
            return ValidationResult.Success;
        }

        private string GetErrorMessage()
        {
            return "Allowed extensions are svg and ico.";
        }
    }
}
