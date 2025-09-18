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

        // Backgrounds & Scenes
        var backgrounds = new PromptCategory { Name = "Backgrounds & Scenes" };
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

        // Professional & Industry
        var business = new PromptCategory { Name = "Professional & Industry" };
        business.Templates.Add(new PromptTemplate
        {
            Title = "LinkedIn Profile",
            Category = "Professional",
            Description = "Professional social media headshot",
            Prompt = "Create a professional LinkedIn-style profile photo with business attire, confident expression, and clean corporate background"
        });
        business.Templates.Add(new PromptTemplate
        {
            Title = "Corporate Headshot",
            Category = "Professional",
            Description = "Formal business portrait",
            Prompt = "Transform into a formal corporate headshot with professional lighting, business attire, and executive-style composition"
        });
        business.Templates.Add(new PromptTemplate
        {
            Title = "Team Photo Style",
            Category = "Professional",
            Description = "Consistent team photo look",
            Prompt = "Standardize for team photo consistency with uniform lighting, background, and professional presentation style"
        });

        // Product & Ecommerce
        var productPhotography = new PromptCategory { Name = "Product & Ecommerce" };
        productPhotography.Templates.Add(new PromptTemplate
        {
            Title = "Clean Product Shot",
            Category = "Product",
            Description = "Remove distractions, focus on product",
            Prompt = "Transform into a clean product photograph with the main subject prominently featured, distracting elements removed, and professional product photography lighting"
        });
        productPhotography.Templates.Add(new PromptTemplate
        {
            Title = "White Background Product",
            Category = "Product",
            Description = "Professional ecommerce white background",
            Prompt = "Place product on a pure white background with professional studio lighting, perfect for ecommerce listings and product catalogs"
        });
        productPhotography.Templates.Add(new PromptTemplate
        {
            Title = "Lifestyle Product",
            Category = "Product",
            Description = "Product in real-world usage context",
            Prompt = "Show the product in a natural lifestyle setting being used in real-world context with authentic lighting and environment"
        });
        productPhotography.Templates.Add(new PromptTemplate
        {
            Title = "Hero Product Image",
            Category = "Product",
            Description = "Eye-catching main product showcase",
            Prompt = "Create a stunning hero product image with dramatic lighting, perfect composition, and premium presentation suitable for main product showcase"
        });
        productPhotography.Templates.Add(new PromptTemplate
        {
            Title = "Product with Props",
            Category = "Product",
            Description = "Add complementary styling props",
            Prompt = "Style the product with complementary props and accessories that enhance its appeal without overwhelming the main subject"
        });
        productPhotography.Templates.Add(new PromptTemplate
        {
            Title = "Minimalist Product",
            Category = "Product",
            Description = "Clean, modern minimalist presentation",
            Prompt = "Present the product in a clean, minimalist style with plenty of negative space, subtle shadows, and modern aesthetic"
        });
        productPhotography.Templates.Add(new PromptTemplate
        {
            Title = "Premium Luxury Look",
            Category = "Product",
            Description = "High-end, sophisticated product styling",
            Prompt = "Transform into a luxury product photograph with premium materials, sophisticated lighting, and high-end presentation style"
        });

        // Add Ecommerce templates to the existing product category
        productPhotography.Templates.Add(new PromptTemplate
        {
            Title = "Marketplace Ready",
            Category = "Product",
            Description = "Amazon/eBay optimized format",
            Prompt = "Optimize for marketplace listings with clear product visibility, appropriate sizing, and format suitable for Amazon, eBay, or similar platforms"
        });
        productPhotography.Templates.Add(new PromptTemplate
        {
            Title = "Size Chart Friendly",
            Category = "Product",
            Description = "Clear scale and proportion emphasis",
            Prompt = "Present the product with clear scale references and proportions that help customers understand size and dimensions"
        });
        productPhotography.Templates.Add(new PromptTemplate
        {
            Title = "Feature Highlight",
            Category = "Product",
            Description = "Emphasize key product features",
            Prompt = "Highlight the most important product features with strategic lighting and composition that draws attention to key selling points"
        });
        productPhotography.Templates.Add(new PromptTemplate
        {
            Title = "Before/After Comparison",
            Category = "Product",
            Description = "Show product benefits/transformation",
            Prompt = "Create a compelling before/after style image that demonstrates the product's benefits or transformation capabilities"
        });
        productPhotography.Templates.Add(new PromptTemplate
        {
            Title = "Multi-Angle Composite",
            Category = "Product",
            Description = "Professional product catalog style",
            Prompt = "Present the product in a professional catalog style showing multiple angles or views in a single, well-composed image"
        });
        productPhotography.Templates.Add(new PromptTemplate
        {
            Title = "Zoom-Friendly Detail",
            Category = "Product",
            Description = "High detail for product inspection",
            Prompt = "Enhance image for detailed inspection with crystal clear focus, high resolution details, and optimal lighting for close examination"
        });
        productPhotography.Templates.Add(new PromptTemplate
        {
            Title = "Mobile Optimized",
            Category = "Product",
            Description = "Perfect for mobile shopping",
            Prompt = "Optimize the image for mobile viewing with clear visibility at small sizes, high contrast, and mobile-friendly composition"
        });

        // Digital Marketing
        var socialMedia = new PromptCategory { Name = "Digital Marketing" };
        socialMedia.Templates.Add(new PromptTemplate
        {
            Title = "Instagram Product Post",
            Category = "Digital Marketing",
            Description = "Square format, social-ready styling",
            Prompt = "Style for Instagram with trendy aesthetics, perfect square composition, and social media friendly presentation that encourages engagement"
        });
        socialMedia.Templates.Add(new PromptTemplate
        {
            Title = "Story Template",
            Category = "Digital Marketing",
            Description = "Vertical format for Instagram/TikTok stories",
            Prompt = "Format for vertical social media stories with engaging composition, leaving space for text overlays and story elements"
        });
        socialMedia.Templates.Add(new PromptTemplate
        {
            Title = "Pinterest Pin Style",
            Category = "Digital Marketing",
            Description = "Tall format with text overlay space",
            Prompt = "Create a Pinterest-optimized image with tall aspect ratio, eye-catching composition, and space for text overlays"
        });
        socialMedia.Templates.Add(new PromptTemplate
        {
            Title = "Facebook Ad Ready",
            Category = "Digital Marketing",
            Description = "Engaging social commerce format",
            Prompt = "Design for Facebook advertising with attention-grabbing composition, clear product focus, and format optimized for social commerce"
        });
        socialMedia.Templates.Add(new PromptTemplate
        {
            Title = "Influencer Style",
            Category = "Digital Marketing",
            Description = "Trendy, lifestyle-focused presentation",
            Prompt = "Style like an influencer post with trendy aesthetics, lifestyle integration, and authentic social media presentation"
        });
        socialMedia.Templates.Add(new PromptTemplate
        {
            Title = "User-Generated Content",
            Category = "Digital Marketing",
            Description = "Authentic, customer-style photo",
            Prompt = "Make it look like authentic user-generated content with natural, unposed styling and genuine customer perspective"
        });
        socialMedia.Templates.Add(new PromptTemplate
        {
            Title = "Brand Story Visual",
            Category = "Digital Marketing",
            Description = "Consistent brand aesthetic",
            Prompt = "Align with brand storytelling aesthetic using consistent colors, mood, and styling that reinforces brand identity"
        });

        // Add Website templates to the existing digital marketing category
        socialMedia.Templates.Add(new PromptTemplate
        {
            Title = "Hero Banner Style",
            Category = "Digital Marketing",
            Description = "Large website header image format",
            Prompt = "Create a stunning hero banner image with wide format composition, impactful presentation, and space for website header text"
        });
        socialMedia.Templates.Add(new PromptTemplate
        {
            Title = "Category Page Tile",
            Category = "Digital Marketing",
            Description = "Consistent grid-friendly format",
            Prompt = "Format for website category grids with consistent sizing, clean presentation, and uniform styling across product collections"
        });
        socialMedia.Templates.Add(new PromptTemplate
        {
            Title = "Product Card Image",
            Category = "Digital Marketing",
            Description = "Perfect for product listing cards",
            Prompt = "Optimize for product cards with clean background, centered product, and format perfect for website product listings and grids"
        });
        socialMedia.Templates.Add(new PromptTemplate
        {
            Title = "Blog Post Featured",
            Category = "Digital Marketing",
            Description = "Editorial-style product integration",
            Prompt = "Style for blog post integration with editorial aesthetics, storytelling composition, and format suitable for content marketing"
        });
        socialMedia.Templates.Add(new PromptTemplate
        {
            Title = "Landing Page Hero",
            Category = "Digital Marketing",
            Description = "Conversion-focused presentation",
            Prompt = "Create a conversion-optimized hero image with compelling product presentation designed to drive sales and engagement"
        });
        socialMedia.Templates.Add(new PromptTemplate
        {
            Title = "Comparison Chart Ready",
            Category = "Digital Marketing",
            Description = "Side-by-side comparison format",
            Prompt = "Format for product comparisons with consistent styling, clear visibility, and presentation suitable for comparison charts"
        });
        socialMedia.Templates.Add(new PromptTemplate
        {
            Title = "Newsletter Featured",
            Category = "Digital Marketing",
            Description = "Email marketing optimized",
            Prompt = "Optimize for email newsletters with eye-catching presentation, appropriate sizing, and format perfect for email marketing campaigns"
        });

        // Add Background Themes to the existing backgrounds category
        backgrounds.Templates.Add(new PromptTemplate
        {
            Title = "Kitchen/Cooking Scene",
            Category = "Backgrounds",
            Description = "For food and kitchen products",
            Prompt = "Place in a beautiful kitchen environment with cooking elements, warm lighting, and culinary atmosphere perfect for food and kitchen products"
        });
        backgrounds.Templates.Add(new PromptTemplate
        {
            Title = "Home Office Setup",
            Category = "Backgrounds",
            Description = "For tech and office products",
            Prompt = "Set in a modern home office environment with desk setup, professional lighting, and workspace atmosphere ideal for tech and office products"
        });
        backgrounds.Templates.Add(new PromptTemplate
        {
            Title = "Outdoor Adventure",
            Category = "Backgrounds",
            Description = "For sports and outdoor gear",
            Prompt = "Place in an outdoor adventure setting with natural elements, dynamic lighting, and active atmosphere perfect for sports and outdoor products"
        });
        backgrounds.Templates.Add(new PromptTemplate
        {
            Title = "Beauty Vanity",
            Category = "Backgrounds",
            Description = "For cosmetics and beauty products",
            Prompt = "Style with elegant vanity setting, soft glamorous lighting, and luxurious atmosphere ideal for cosmetics and beauty products"
        });
        backgrounds.Templates.Add(new PromptTemplate
        {
            Title = "Workshop/Garage",
            Category = "Backgrounds",
            Description = "For tools and hardware",
            Prompt = "Set in a workshop or garage environment with industrial elements, practical lighting, and work atmosphere perfect for tools and hardware"
        });
        backgrounds.Templates.Add(new PromptTemplate
        {
            Title = "Cozy Living Room",
            Category = "Backgrounds",
            Description = "For home and lifestyle products",
            Prompt = "Place in a cozy living room setting with comfortable furniture, warm lighting, and homey atmosphere ideal for lifestyle products"
        });
        backgrounds.Templates.Add(new PromptTemplate
        {
            Title = "Modern Studio",
            Category = "Backgrounds",
            Description = "Clean, contemporary backdrop",
            Prompt = "Set against a modern studio backdrop with clean lines, contemporary design, and professional lighting for versatile product presentation"
        });

        // Seasonal & Trending
        var seasonal = new PromptCategory { Name = "Seasonal & Trending" };
        seasonal.Templates.Add(new PromptTemplate
        {
            Title = "Holiday Themed",
            Category = "Seasonal",
            Description = "Seasonal product presentation",
            Prompt = "Add holiday theming with seasonal decorations, festive colors, and celebration atmosphere appropriate for holiday marketing"
        });
        seasonal.Templates.Add(new PromptTemplate
        {
            Title = "Summer Vibes",
            Category = "Seasonal",
            Description = "Bright, energetic summer styling",
            Prompt = "Create summer atmosphere with bright lighting, warm colors, and energetic summer vibes perfect for seasonal product promotion"
        });
        seasonal.Templates.Add(new PromptTemplate
        {
            Title = "Cozy Winter",
            Category = "Seasonal",
            Description = "Warm, inviting winter atmosphere",
            Prompt = "Add cozy winter elements with warm lighting, winter textures, and inviting atmosphere ideal for cold season marketing"
        });
        seasonal.Templates.Add(new PromptTemplate
        {
            Title = "Spring Fresh",
            Category = "Seasonal",
            Description = "Clean, renewal-themed styling",
            Prompt = "Create fresh spring atmosphere with clean aesthetics, renewal themes, and fresh colors perfect for spring product launches"
        });
        seasonal.Templates.Add(new PromptTemplate
        {
            Title = "Back to School",
            Category = "Seasonal",
            Description = "Educational/productivity focus",
            Prompt = "Style for back-to-school season with educational elements, productivity focus, and academic atmosphere"
        });
        seasonal.Templates.Add(new PromptTemplate
        {
            Title = "Black Friday Style",
            Category = "Seasonal",
            Description = "Sale/promotion optimized",
            Prompt = "Design for sales and promotions with attention-grabbing presentation, deal-focused styling, and promotional atmosphere"
        });
        seasonal.Templates.Add(new PromptTemplate
        {
            Title = "Gift Guide Ready",
            Category = "Seasonal",
            Description = "Present/gifting context",
            Prompt = "Present as gift guide material with gifting context, elegant presentation, and styling that suggests the perfect present"
        });

        // Add Industry Specific templates to the existing professional category
        business.Templates.Add(new PromptTemplate
        {
            Title = "Fashion Lookbook",
            Category = "Professional",
            Description = "Clothing and accessory styling",
            Prompt = "Style as fashion lookbook with trendy presentation, style-focused composition, and fashion industry aesthetics"
        });
        business.Templates.Add(new PromptTemplate
        {
            Title = "Tech Product Demo",
            Category = "Professional",
            Description = "Clean tech product presentation",
            Prompt = "Present as tech product with clean, modern aesthetics, innovative presentation, and technology-focused styling"
        });
        business.Templates.Add(new PromptTemplate
        {
            Title = "Food & Beverage",
            Category = "Professional",
            Description = "Appetizing culinary presentation",
            Prompt = "Make food and beverages look appetizing with mouth-watering presentation, perfect lighting, and culinary styling"
        });
        business.Templates.Add(new PromptTemplate
        {
            Title = "Beauty & Cosmetics",
            Category = "Professional",
            Description = "Glamorous beauty product styling",
            Prompt = "Style beauty products with glamorous presentation, luxurious aesthetics, and beauty industry standard presentation"
        });
        business.Templates.Add(new PromptTemplate
        {
            Title = "Home & Garden",
            Category = "Professional",
            Description = "Lifestyle home improvement context",
            Prompt = "Present home and garden products in lifestyle context with practical application, home improvement focus, and domestic atmosphere"
        });
        business.Templates.Add(new PromptTemplate
        {
            Title = "Fitness & Wellness",
            Category = "Professional",
            Description = "Active, healthy lifestyle context",
            Prompt = "Style with fitness and wellness focus, active lifestyle presentation, and health-conscious atmosphere"
        });
        business.Templates.Add(new PromptTemplate
        {
            Title = "Baby & Kids",
            Category = "Professional",
            Description = "Safe, playful, family-friendly styling",
            Prompt = "Present with family-friendly aesthetics, safe and playful presentation, and child-appropriate styling and colors"
        });

        _categories.Add(photography);
        _categories.Add(artStyles);
        _categories.Add(backgrounds);
        _categories.Add(enhancement);
        _categories.Add(productPhotography);
        _categories.Add(socialMedia);
        _categories.Add(business);
        _categories.Add(seasonal);
    }
}
