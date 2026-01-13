using AzureOpenAI.Models;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AzureOpenAI.Services.DocumentProcessing;

public class DocumentLoader
{
    public async Task<List<KnowledgeDocument>> LoadFromDirectoryAsync(string directoryPath)
    {
        var documents = new List<KnowledgeDocument>();

        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine($"⚠️  Directory not found: {directoryPath}");
            return documents;
        }

        var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);

        foreach (var filePath in files)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            try
            {
                KnowledgeDocument? doc = extension switch
                {
                    ".md" or ".txt" => await LoadTextFileAsync(filePath),
                    ".json" => await LoadJsonFileAsync(filePath),
                    _ => null
                };

                if (doc != null)
                {
                    documents.Add(doc);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Error loading {filePath}: {ex.Message}");
            }
        }

        return documents;
    }

    private async Task<KnowledgeDocument?> LoadTextFileAsync(string filePath)
    {
        var content = await File.ReadAllTextAsync(filePath);
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        // Check if it's a markdown file with YAML frontmatter
        if (extension == ".md" && content.StartsWith("---"))
        {
            return ParseMarkdownWithFrontmatter(content, filePath, fileName);
        }

        // Default text file handling
        return new KnowledgeDocument
        {
            Id = Guid.NewGuid().ToString(),
            Title = fileName,
            Content = content,
            Source = filePath,
            LastUpdated = File.GetLastWriteTime(filePath),
            DocumentType = InferDocumentType(fileName, content),
            Category = InferCategory(filePath),
            Tags = []
        };
    }

    private KnowledgeDocument? ParseMarkdownWithFrontmatter(string content, string filePath, string fileName)
    {
        try
        {
            // Split frontmatter and content
            var lines = content.Split('\n');
            var frontmatterEnd = -1;

            // Find the closing --- for frontmatter
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Trim() == "---")
                {
                    frontmatterEnd = i;
                    break;
                }
            }

            if (frontmatterEnd == -1)
            {
                // No valid frontmatter, treat as regular markdown
                return new KnowledgeDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = fileName,
                    Content = content,
                    Source = filePath,
                    LastUpdated = File.GetLastWriteTime(filePath),
                    DocumentType = InferDocumentType(fileName, content),
                    Category = InferCategory(filePath),
                    Tags = []
                };
            }

            // Extract frontmatter and content
            var frontmatterLines = lines[1..frontmatterEnd];
            var frontmatterYaml = string.Join('\n', frontmatterLines);
            var markdownContent = string.Join('\n', lines[(frontmatterEnd + 1)..]).Trim();

            // Parse YAML frontmatter
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var metadata = deserializer.Deserialize<Dictionary<string, object>>(frontmatterYaml);

            // Create KnowledgeDocument from metadata
            var doc = new KnowledgeDocument
            {
                Id = GetMetadataValue(metadata, "id") ?? Guid.NewGuid().ToString(),
                Title = GetMetadataValue(metadata, "title") ?? fileName,
                Content = markdownContent,
                Source = filePath,
                LastUpdated = File.GetLastWriteTime(filePath),
                DocumentType = ParseDocumentType(GetMetadataValue(metadata, "documentType") ?? "KnowledgeBaseArticle"),
                Category = GetMetadataValue(metadata, "category") ?? InferCategory(filePath),
                Author = GetMetadataValue(metadata, "author") ?? string.Empty,
                Tags = ParseTags(metadata)
            };

            return doc;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Error parsing frontmatter in {filePath}: {ex.Message}");
            return null;
        }
    }

    private string? GetMetadataValue(Dictionary<string, object> metadata, string key)
    {
        if (metadata.TryGetValue(key, out var value))
        {
            return value?.ToString();
        }
        return null;
    }

    private List<string> ParseTags(Dictionary<string, object> metadata)
    {
        if (metadata.TryGetValue("tags", out var tagsObj))
        {
            if (tagsObj is List<object> tagsList)
            {
                return tagsList.Select(t => t.ToString() ?? string.Empty).Where(t => !string.IsNullOrEmpty(t)).ToList();
            }
            else if (tagsObj is string tagsString)
            {
                return tagsString.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();
            }
        }
        return [];
    }

    private DocumentType ParseDocumentType(string typeString)
    {
        if (Enum.TryParse<DocumentType>(typeString, true, out var docType))
        {
            return docType;
        }
        return DocumentType.KnowledgeBaseArticle;
    }

    private async Task<KnowledgeDocument?> LoadJsonFileAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var doc = JsonSerializer.Deserialize<KnowledgeDocument>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (doc != null && string.IsNullOrEmpty(doc.Id))
        {
            doc.Id = Guid.NewGuid().ToString();
        }

        return doc;
    }

    private DocumentType InferDocumentType(string fileName, string content)
    {
        var lowerFileName = fileName.ToLowerInvariant();
        var lowerContent = content.ToLowerInvariant();

        if (lowerFileName.Contains("policy") || lowerFileName.Contains("policies"))
            return DocumentType.Policy;
        if (lowerFileName.Contains("procedure") || lowerFileName.Contains("sop"))
            return DocumentType.Procedure;
        if (lowerFileName.Contains("faq"))
            return DocumentType.FAQ;
        if (lowerFileName.Contains("troubleshoot") || lowerFileName.Contains("troubleshooting"))
            return DocumentType.Troubleshooting;
        if (lowerFileName.Contains("guide") || lowerFileName.Contains("manual"))
            return DocumentType.UserGuide;
        if (lowerFileName.Contains("technical") || lowerFileName.Contains("architecture"))
            return DocumentType.TechnicalDocumentation;

        return DocumentType.KnowledgeBaseArticle;
    }

    private string InferCategory(string filePath)
    {
        var pathLower = filePath.ToLowerInvariant();

        if (pathLower.Contains("network")) return "Network";
        if (pathLower.Contains("security")) return "Security";
        if (pathLower.Contains("email")) return "Email";
        if (pathLower.Contains("hardware")) return "Hardware";
        if (pathLower.Contains("software")) return "Software";
        if (pathLower.Contains("account")) return "Account";
        if (pathLower.Contains("access")) return "Access";
        if (pathLower.Contains("password")) return "Password";

        return "General";
    }

    public List<KnowledgeDocument> LoadFromMemory(List<KnowledgeDocument> documents)
    {
        return documents;
    }
}