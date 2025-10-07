using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Files.DeleteFile;
/// <summary>
/// Handles the deletion of uploaded files, removing both the physical file and database metadata.
/// </summary>
/// <remarks>
/// This handler performs two critical operations:
/// 1. Deletes the physical file from the file system
/// 2. Removes the file metadata from the database
/// 
/// If the physical file deletion fails, the operation continues to remove the database record
/// to prevent orphaned metadata.
/// </remarks>
internal sealed class DeleteFileCommandHandler(
    IApplicationDbContext dbContext,
    ILogger<DeleteFileCommandHandler> logger,
    IDateTimeProvider dateTimeProvider
) : ICommandHandler<DeleteFileCommand, DeleteFileResponse>
{
    public async Task<Result<DeleteFileResponse>> Handle(
        DeleteFileCommand command,
        CancellationToken cancellationToken)
    {
        if (command is null)
        {
            return Result.Failure<DeleteFileResponse>(Error.NullValue);
        }

        // Fetch the file metadata from database
        UploadedFile? uploadedFile = await dbContext.UploadedFiles
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (uploadedFile is null)
        {
            return Result.Failure<DeleteFileResponse>(Error.NotFound(
                "File.NotFound",
                $"The file with ID '{command.Id}' was not found."));
        }

        // Store details for response before deletion
        var response = new DeleteFileResponse(
            uploadedFile.Id,
            uploadedFile.OriginalFileName,
            uploadedFile.StoredFileName,
            uploadedFile.Size,
            uploadedFile.PhysicalPath,
            dateTimeProvider.UtcNow,
            command.DeletedBy
        );

        // Delete physical file from disk
        bool physicalFileDeleted = false;
        try
        {
            if (File.Exists(uploadedFile.PhysicalPath))
            {
                File.Delete(uploadedFile.PhysicalPath);
                physicalFileDeleted = true;
                logger.LogInformation(
                    "Physical file deleted at '{PhysicalPath}' by user {UserId}",
                    uploadedFile.PhysicalPath,
                    command.DeletedBy);
            }
            else
            {
                logger.LogWarning(
                    "Physical file not found at '{PhysicalPath}'. Proceeding to delete metadata.",
                    uploadedFile.PhysicalPath);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to delete physical file at '{PhysicalPath}'. Proceeding to delete metadata.",
                uploadedFile.PhysicalPath);
        }

        // Remove metadata from database
        dbContext.UploadedFiles.Remove(uploadedFile);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "File '{FileName}' (ID: {FileId}) deleted by user {UserId}. Physical file deleted: {PhysicalDeleted}",
            uploadedFile.OriginalFileName,
            uploadedFile.Id,
            command.DeletedBy,
            physicalFileDeleted);

        return Result.Success(response);
    }
}
