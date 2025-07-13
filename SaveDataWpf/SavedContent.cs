namespace SaveDataWpf
{
    internal sealed record SavedContent
    {
        public string Content { get; init; } = default!;
        public bool IsEncrypted { get; init; } = default!;

        public SavedContent(string content, bool isEncrypted)
        {
            Content = content;
            IsEncrypted = isEncrypted;
        }
    }
}
