using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedKernel;

namespace Domain.Branches;
public static class BranchErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "Branch.NotFound",
        $"The branch with ID '{id}' was not found");

    public static Error NotFoundByCode(string code) => Error.NotFound(
        "Branch.NotFoundByCode",
        $"The branch with code '{code}' was not found");

    public static Error CodeNotUnique => Error.Conflict(
        "Branch.CodeNotUnique",
        "The branch code must be unique");

    public static Error EmailNotUnique => Error.Conflict(
        "Branch.EmailNotUnique",
        "The branch email must be unique");

    public static Error OrganizationNotFound => Error.NotFound(
        "Branch.OrganizationNotFound",
        "The specified organization does not exist");
}
