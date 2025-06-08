using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ShortN.Services;
using Microsoft.AspNetCore.Mvc;
using ShortN.Models;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ShortN.Routes;

public static class URLRoutes
{
    public static void MapURLRoutes(this WebApplication app)
    {
        app.MapGet("/{shortCode}", async (string shortCode, IUrlShortenerService urlShortenerService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Attempting to redirect short code: {ShortCode}", shortCode);
            var urlEntry = await urlShortenerService.GetUrlEntryByShortCodeAsync(shortCode);
            if (urlEntry is null)
            {
                logger.LogWarning("Short code not found: {ShortCode}", shortCode);
                return Results.NotFound();
            }
            else if (urlEntry.ExpiresAt is not null && DateTime.Compare(DateTime.Now, urlEntry.ExpiresAt.Value) > 0){
                logger.LogWarning("Short code expired: {ShortCode}", shortCode);
                return Results.NotFound("This short link has expired.");
            }

            logger.LogInformation("Redirecting {ShortCode} to {LongUrl}", shortCode, urlEntry.LongUrl);
            return Results.Redirect(urlEntry.LongUrl, false, true);
        })
        .WithName("GetUrl")
        .WithTags("URL Shortener")
        .Produces(StatusCodes.Status307TemporaryRedirect)
        .Produces(StatusCodes.Status404NotFound);

        app.MapDelete("/{shortCode}", async (string shortCode, IUrlShortenerService urlShortenerService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Attempting to delete short code: {ShortCode}", shortCode);
            var urlEntry = await urlShortenerService.GetUrlEntryByShortCodeAsync(shortCode);
            if (urlEntry is null)
            {
                logger.LogWarning("Short code not found for deletion: {ShortCode}", shortCode);
                return Results.NotFound();
            }

            if(await urlShortenerService.DeleteUrlEntry(urlEntry)){
                logger.LogInformation("Successfully deleted short code: {ShortCode}", shortCode);
                return Results.NoContent();
            }
            else {
                logger.LogError("Failed to delete short code: {ShortCode}", shortCode);
                return Results.InternalServerError("Could not delete URL Entry. Please try again.");
            }
        })
        .WithName("DeleteUrl")
        .WithTags("URL Shortener")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/new", async (IUrlShortenerService urlShortenerService, IValidator<UrlRequest> validator, [FromBody] UrlRequest request, ILogger<Program> logger) =>
        {
            logger.LogInformation("Creating new URL entry for: {LongUrl}", request.LongUrl);
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                logger.LogWarning("Invalid URL request: {Errors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return Results.BadRequest(validationResult.Errors);
            }

            if (request.CustomCode is not null)
            {
                logger.LogInformation("Checking custom code availability: {CustomCode}", request.CustomCode);
                var existingEntry = await urlShortenerService.GetUrlEntryByShortCodeAsync(request.CustomCode);
                if (existingEntry is not null)
                {
                    if (existingEntry.ExpiresAt is not null && DateTime.Compare(DateTime.Now, existingEntry.ExpiresAt.Value) > 0)
                    {
                        logger.LogInformation("Deleting expired entry with custom code: {CustomCode}", request.CustomCode);
                        await urlShortenerService.DeleteUrlEntry(existingEntry);
                    }
                    else
                    {
                        logger.LogWarning("Custom code already in use: {CustomCode}", request.CustomCode);
                        return Results.Conflict($"The requested custom code is already in use.");
                    }
                }
            }

            var urlEntry = await urlShortenerService.CreateUrlEntryAsync(request.LongUrl, request.CustomCode, request.Ttl);
            logger.LogInformation("Successfully created URL entry: {ShortCode} -> {LongUrl}", urlEntry.ShortCode, urlEntry.LongUrl);
            return Results.Created($"/{urlEntry.ShortCode}", urlEntry);
        })
        .WithName("CreateUrl")
        .WithTags("URL Shortener")
        .Produces<UrlEntry>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("/urls", async (IUrlShortenerService urlShortenerService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Retrieving all URL entries");
            var entries = await urlShortenerService.GetUrlEntries();
            logger.LogInformation("Retrieved {Count} URL entries", entries.Count);
            return Results.Ok(entries);
        })
        .WithName("GetAllUrls")
        .WithTags("URL Shortener")
        .Produces<List<UrlEntry>>(StatusCodes.Status200OK);
    }
}