using System;

namespace Application.Files.UploadFile;

public sealed record UploadFileResponse(
    Uri Url,
    string StoredFileName,
    long Size
);
