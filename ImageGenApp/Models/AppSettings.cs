using ImageGen.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ImageGenApp.Models;

public class AppSettings
{
    [Key]
    public int Id { get; set; } = 1; // Always use ID 1 for singleton

    public string? ApiKey { get; set; }

    public ImageQuality DefaultQuality { get; set; } = ImageQuality.High;

    public ImageFormat DefaultFormat { get; set; } = ImageFormat.Jpeg;

    public InputFidelity DefaultFidelity { get; set; } = InputFidelity.High;

    public string Theme { get; set; } = "Default"; // Default, Light, Dark

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
