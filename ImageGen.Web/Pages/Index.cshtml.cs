using ImageGen.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ImageGen.Web.Pages;

public class IndexModel(ImageGenOptions options, ILogger<IndexModel> logger) : BasePageModel(options)
{
    private readonly ILogger<IndexModel> _logger = logger;

    public void OnGet()
    {

    }
}
