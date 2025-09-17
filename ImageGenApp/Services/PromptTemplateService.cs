using ImageGenApp.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace ImageGenApp.Services;

public class PromptTemplateService
{
    private readonly ObservableCollection<PromptCategory> _categories = [];

    public PromptTemplateService()
    {
        InitializeTemplates();
    }

    public ObservableCollection<PromptCategory> GetCategories() => _categories;

    public ObservableCollection<PromptTemplate> SearchTemplates(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new ObservableCollection<PromptTemplate>(_categories.SelectMany(c => c.Templates));
        }

        var results = _categories
            .SelectMany(c => c.Templates)
            .Where(t => t.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                       t.Prompt.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                       t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                       t.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return new ObservableCollection<PromptTemplate>(results);
    }

    private void InitializeTemplates()
    {
        // Photography & Style
        var photography = new PromptCategory { Name = "Photography & Style" };
        photography.Templates.Add(new PromptTemplate
        {
            Title = "Professional Portrait",
            Category = "Photography",
            Description = "Convert to professional headshot style",
            Prompt = "Transform this into a professional portrait photograph with studio lighting, clean background, and sharp focus on the subject's face"
        });
        photography.Templates.Add(new PromptTemplate
        {
            Title = "Vintage Film Look",
            Category = "Photography",
            Description = "Apply vintage film photography aesthetic",
            Prompt = "Convert to vintage film photography style with warm tones, slight grain, and classic color grading reminiscent of 1970s photography"
        });
        photography.Templates.Add(new PromptTemplate
        {
            Title = "Black & White Artistic",
            Category = "Photography",
            Description = "Dramatic black and white conversion",
            Prompt = "Transform into a dramatic black and white photograph with high contrast, deep shadows, and artistic lighting"
        });
        photography.Templates.Add(new PromptTemplate
        {
            Title = "Golden Hour Glow",
            Category = "Photography",
            Description = "Warm golden hour lighting effect",
            Prompt = "Add warm golden hour lighting with soft, glowing sunlight, enhanced warm tones, and beautiful lens flare effects"
        });

        // Art Styles
        var artStyles = new PromptCategory { Name = "Art Styles" };
        artStyles.Templates.Add(new PromptTemplate
        {
            Title = "Oil Painting",
            Category = "Art",
            Description = "Classic oil painting style",
            Prompt = "Transform into a classical oil painting with visible brush strokes, rich textures, and traditional artistic composition"
        });
        artStyles.Templates.Add(new PromptTemplate
        {
            Title = "Watercolor Art",
            Category = "Art",
            Description = "Soft watercolor painting effect",
            Prompt = "Convert to a delicate watercolor painting with soft edges, transparent colors, and artistic paper texture"
        });
        artStyles.Templates.Add(new PromptTemplate
        {
            Title = "Digital Art",
            Category = "Art",
            Description = "Modern digital illustration style",
            Prompt = "Transform into modern digital art with clean lines, vibrant colors, and contemporary illustration techniques"
        });
        artStyles.Templates.Add(new PromptTemplate
        {
            Title = "Anime Style",
            Category = "Art",
            Description = "Japanese anime/manga art style",
            Prompt = "Convert to anime/manga art style with large expressive eyes, clean cel-shading, and vibrant anime colors"
        });

        // Backgrounds & Environments
        var backgrounds = new PromptCategory { Name = "Backgrounds & Environments" };
        backgrounds.Templates.Add(new PromptTemplate
        {
            Title = "Remove Background",
            Category = "Background",
            Description = "Clean background removal",
            Prompt = "Remove the background completely, leaving only the main subject on a transparent or white background"
        });
        backgrounds.Templates.Add(new PromptTemplate
        {
            Title = "Studio Background",
            Category = "Background",
            Description = "Professional studio backdrop",
            Prompt = "Replace background with a clean, professional studio backdrop with gradient lighting and smooth texture"
        });
        backgrounds.Templates.Add(new PromptTemplate
        {
            Title = "Nature Scene",
            Category = "Background",
            Description = "Beautiful natural environment",
            Prompt = "Place the subject in a beautiful natural environment with lush greenery, soft natural lighting, and scenic landscape"
        });
        backgrounds.Templates.Add(new PromptTemplate
        {
            Title = "City Skyline",
            Category = "Background",
            Description = "Modern urban cityscape",
            Prompt = "Set against a modern city skyline backdrop with tall buildings, urban lighting, and metropolitan atmosphere"
        });

        // Enhancement & Effects
        var enhancement = new PromptCategory { Name = "Enhancement & Effects" };
        enhancement.Templates.Add(new PromptTemplate
        {
            Title = "Enhance Quality",
            Category = "Enhancement",
            Description = "Overall image quality improvement",
            Prompt = "Enhance overall image quality with improved sharpness, better contrast, reduced noise, and optimized colors"
        });
        enhancement.Templates.Add(new PromptTemplate
        {
            Title = "Color Pop",
            Category = "Enhancement",
            Description = "Vibrant color enhancement",
            Prompt = "Enhance colors to be more vibrant and saturated while maintaining natural skin tones and realistic appearance"
        });
        enhancement.Templates.Add(new PromptTemplate
        {
            Title = "Dramatic Lighting",
            Category = "Enhancement",
            Description = "Add dramatic lighting effects",
            Prompt = "Add dramatic lighting with strong contrast, directional light sources, and enhanced shadows and highlights"
        });
        enhancement.Templates.Add(new PromptTemplate
        {
            Title = "Soft Glow Effect",
            Category = "Enhancement",
            Description = "Gentle glow and softening",
            Prompt = "Add a soft, ethereal glow effect with gentle lighting and slightly softened features for a dreamy appearance"
        });

        // Business & Professional
        var business = new PromptCategory { Name = "Business & Professional" };
        business.Templates.Add(new PromptTemplate
        {
            Title = "LinkedIn Profile",
            Category = "Business",
            Description = "Professional social media headshot",
            Prompt = "Create a professional LinkedIn-style profile photo with business attire, confident expression, and clean corporate background"
        });
        business.Templates.Add(new PromptTemplate
        {
            Title = "Corporate Headshot",
            Category = "Business",
            Description = "Formal business portrait",
            Prompt = "Transform into a formal corporate headshot with professional lighting, business attire, and executive-style composition"
        });
        business.Templates.Add(new PromptTemplate
        {
            Title = "Team Photo Style",
            Category = "Business",
            Description = "Consistent team photo look",
            Prompt = "Standardize for team photo consistency with uniform lighting, background, and professional presentation style"
        });

        _categories.Add(photography);
        _categories.Add(artStyles);
        _categories.Add(backgrounds);
        _categories.Add(enhancement);
        _categories.Add(business);
    }
}
