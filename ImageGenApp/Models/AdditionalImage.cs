using Microsoft.UI.Xaml.Media.Imaging;

namespace ImageGenApp.Models;

public class AdditionalImage
{
    public string FilePath { get; set; } = string.Empty;
    public BitmapImage? ImageSource { get; set; }
    public Stream? ImageStream { get; set; }
}
