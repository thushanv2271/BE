using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Application.Files.DeleteFile;
/// <summary>
/// Validates the DeleteFileCommand to ensure required fields are provided.
/// </summary>
internal sealed class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("File ID is required");

        RuleFor(x => x.DeletedBy)
            .NotEmpty()
            .WithMessage("DeletedBy user ID is required");
    }
}
