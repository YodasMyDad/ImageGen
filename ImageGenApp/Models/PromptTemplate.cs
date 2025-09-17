using System.Collections.ObjectModel;

namespace ImageGenApp.Models;

public class PromptTemplate
{
    public string Title { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class PromptCategory
{
    public string Name { get; set; } = string.Empty;
    public ObservableCollection<PromptTemplate> Templates { get; set; } = [];
}
