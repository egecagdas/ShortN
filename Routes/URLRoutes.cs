using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ShortN.Services;
using Microsoft.AspNetCore.Mvc;
using ShortN.Models;
using FluentValidation;

namespace ShortN.Routes;

public static class URLRoutes
{
    public static void MapURLRoutes(this WebApplication app)
    {
        app.MapGet("/{shortCode}", async (string shortCode, IUrlShortenerService urlShortenerService) =>
        {
            var urlEntry = await urlShortenerService.GetUrlEntryByShortCodeAsync(shortCode);
            if (urlEntry is null)
            {
                return Results.NotFound();
            }
            else if (urlEntry.ExpiresAt is not null && DateTime.Compare(DateTime.Now, urlEntry.ExpiresAt.Value) > 0){
                return Results.NotFound("This short link has expired.");
            }

            return Results.Redirect(urlEntry.LongUrl, false, true);
        })
        .WithName("GetUrl")
        .WithTags("URL Shortener")
        .Produces(StatusCodes.Status307TemporaryRedirect)
        .Produces(StatusCodes.Status404NotFound);

        app.MapDelete("/{shortCode}", async (string shortCode, IUrlShortenerService urlShortenerService) =>
        {
            var urlEntry = await urlShortenerService.GetUrlEntryByShortCodeAsync(shortCode);
            if (urlEntry is null)
            {
                return Results.NotFound();
            }

            if(await urlShortenerService.DeleteUrlEntry(urlEntry)){
                return Results.NoContent();
            }
            else {
                return Results.InternalServerError("Could not delete URL Entry. Please try again.");
            }
        })
        .WithName("DeleteUrl")
        .WithTags("URL Shortener")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/new", async (IUrlShortenerService urlShortenerService, IValidator<UrlRequest> validator, [FromBody] UrlRequest request) =>
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }

            if (request.CustomCode is not null)
            {
                var existingEntry = await urlShortenerService.GetUrlEntryByShortCodeAsync(request.CustomCode);
                if (existingEntry is not null)
                {
                    // Check if the existing entry is expired
                    if (existingEntry.ExpiresAt is not null && DateTime.Compare(DateTime.Now, existingEntry.ExpiresAt.Value) > 0)
                    {
                        // Delete the expired entry
                        await urlShortenerService.DeleteUrlEntry(existingEntry);
                    }
                    else
                    {
                        return Results.Conflict($"The requested custom code is already in use.");
                    }
                }
            }

            var urlEntry = await urlShortenerService.CreateUrlEntryAsync(request.LongUrl, request.CustomCode, request.Ttl);
            return Results.Created($"/{urlEntry.ShortCode}", urlEntry);
        })
        .WithName("CreateUrl")
        .WithTags("URL Shortener")
        .Produces<UrlEntry>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("/urls", async (IUrlShortenerService urlShortenerService) =>
        {
            var entries = await urlShortenerService.GetUrlEntries();
            return Results.Ok(entries);
        })
        .WithName("GetAllUrls")
        .WithTags("URL Shortener")
        .Produces<List<UrlEntry>>(StatusCodes.Status200OK);
    }
}