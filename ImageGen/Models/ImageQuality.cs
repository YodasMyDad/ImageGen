namespace ImageGen.Models;

/// <summary>
/// Specifies the quality level for generated images.
/// Higher quality typically produces more detailed images but may take longer to generate.
/// </summary>
public enum ImageQuality
{
    /// <summary>
    /// Standard quality - good balance between quality and generation speed.
    /// </summary>
    Standard = 0,

    /// <summary>
    /// High quality - produces more detailed and refined images.
    /// May take longer to generate than standard quality.
    /// </summary>
    High = 1
}
