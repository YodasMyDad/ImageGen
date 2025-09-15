namespace ImageGen.Models;

/// <summary>
/// Specifies the input fidelity level for image editing operations.
/// High fidelity preserves more details from the input image, especially faces and distinctive features.
/// </summary>
public enum InputFidelity
{
    /// <summary>
    /// Default fidelity level - balanced processing speed and quality.
    /// </summary>
    Default = 0,

    /// <summary>
    /// High fidelity level - preserves more details from input images,
    /// especially faces, logos, and distinctive features. May take longer to process.
    /// </summary>
    High = 1
}
