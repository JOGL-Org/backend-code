using AutoMapper.Internal;
using Jogl.Server.API.Model;
using Jogl.Server.Data;
using System.ComponentModel.DataAnnotations;

namespace Jogl.Server.API.Validators
{
    public class DocumentValidationAttribute : ValidationAttribute
    {
        private DocumentType? _type;

        public DocumentValidationAttribute()
        {
        }

        public DocumentValidationAttribute(DocumentType type)
        {
            _type = type;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is DocumentInsertModel)
            {
                var doc = value as DocumentInsertModel;
                return ValidateDocument(doc);
            }

            var docs = value as List<DocumentInsertModel>;
            if (docs == null)
                return ValidationResult.Success;

            foreach (var doc in docs)
            {
                var res = ValidateDocument(doc);
                if (res != ValidationResult.Success)
                    return res;
            }

            return ValidationResult.Success;
        }

        private ValidationResult ValidateDocument(DocumentInsertModel doc)
        {
            if (_type.HasValue && doc.Type != _type)
                return new ValidationResult($"Only documents of type {doc.Type} are expected");

            switch (doc.Type)
            {
                case DocumentType.Document:
                    if (string.IsNullOrEmpty(doc.Data))
                        return new ValidationResult($"Documents of type {DocumentType.Document} must have data");
                    break;

                case DocumentType.Link:
                    if (string.IsNullOrEmpty(doc.URL))
                        return new ValidationResult($"Documents of type {DocumentType.Link} must have a URL");
                    break;
            }

            return ValidationResult.Success;
        }
    }
}
