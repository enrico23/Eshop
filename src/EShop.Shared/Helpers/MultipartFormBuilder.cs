namespace EShop.Shared.Helpers;

public sealed record FilePart(Stream Content, string Name, string? ContentType);

/// <summary>
/// Build Multipart Data Content
/// </summary>
public static class MultipartFormBuilder
{
    public static MultipartFormDataContent Build<T>(
        T model,
        FilePart? file,
        Action<MultipartFormDataContent, T> addFields)
    {
        var form = new MultipartFormDataContent();
        addFields(form, model);

        if (file is not null)
        {
            var fileContent = new StreamContent(file.Content);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");

            form.Add(fileContent, "file", file.Name);
        }

        return form;
    }
}
