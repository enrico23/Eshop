# Azure Blob Storage – How We Use It

## Quick start (local)
- Run the distributed app host (`EShop.AppHost/AppHost.cs`) to bring up Azurite and create the `Blobs` connection string automatically.
- The blob container `product-images` is created on startup by `EShop.BlobStorageInitializer` (no public access).
- The API wires up blob services in `EShop.API/Program.cs` and is ready to accept requests once the initializer has finished.

## Key pieces
- Config: connection string key `Blobs` (in environment/AppHost; not in `appsettings`).
- DI: singleton `BlobServiceClient`, singleton `BlobContainerClient` for `product-images`, and `IBlobFileStorageService`.
- Core logic: `EShop.API/Services/BlobFileStorageService.cs`.
- Download endpoint: `GET /api/images/{imageId}` (`EShop.API/Controllers/ImagesController.cs`).

## Blob service behaviors (developer view)
- Upload (`UploadImageAsync(IFormFile)`):
  - Validates file present, >0 bytes, size <= 5 MB, and `ContentType` starts with `image/`.
  - Names blob as `Guid + original extension` to avoid collisions and keep type hints.
  - Uploads stream with `BlobHttpHeaders.ContentType`; returns the blob name (used as image id/URL).
- Download (`GetImageAsync(string imageId)`):
  - Validates id, checks existence, then `DownloadAsync`.
  - Returns `(Stream, ContentType)`; throws `FileNotFoundException` if missing.
- Delete (`DeleteImageAsync(string imageId)`):
  - Validates id; calls `DeleteIfExistsAsync` with `DeleteSnapshotsOption.IncludeSnapshots`; returns whether a blob was removed.

## How it flows through products
- Create product: if an image file is posted, we upload it and store the returned blob name on the product.
- Update product: if a new image is posted, all existing product images are deleted from Blob Storage, the new one is uploaded, and the product’s image list is replaced.
- Delete product: deletes associated blobs before removing the product record.

## Ops and tips
- Blobs are private: clients never get SAS URLs; images are streamed via the API.
- Production: supply a real `Blobs` connection string; the initializer will create the container if missing.
- Troubleshooting: if image requests 404, check the container exists and the `Blobs` connection string matches your target storage/emulator.

## Local test
1. Start the AppHost so Azurite and the initializer run.
2. Upload an image via the Products API.
3. Fetch it back with `GET /api/images/{imageId}` and verify the content type.


## TODO / Improvements

- Add content-type validation using file signatures (magic numbers) to block spoofed MIME types.
- Add image resizing/thumbnails and store derivatives under prefixes; set cache headers when serving.
- Add CDN/front-door caching and ETag support on GET /api/images/{id} to reduce blob round trips.
- Enable soft delete or versioning (or use a staging container) to avoid race conditions during updates.
- Add antivirus/malware scanning before making uploads available.
- Instrument metrics/logging around upload/delete failures and blob size growth; add alerts.
- If client-side uploads are needed later, add SAS generation with per-blob, short-lived tokens instead of direct keys.
