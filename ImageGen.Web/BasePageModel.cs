using ImageGen.Configuration;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ImageGen.Web;

/// <summary>
/// Base page model with common functionality for all pages.
/// </summary>
public class BasePageModel(ImageGenOptions options) : PageModel
{
    /// <summary>
    /// Whether the API key is configured and valid.
    /// </summary>
    public bool IsApiKeyConfigured => !string.IsNullOrWhiteSpace(options.ApiKey);

    public override void OnPageHandlerExecuting(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context)
    {
        ViewData["IsApiKeyConfigured"] = IsApiKeyConfigured;
        base.OnPageHandlerExecuting(context);
    }
}
