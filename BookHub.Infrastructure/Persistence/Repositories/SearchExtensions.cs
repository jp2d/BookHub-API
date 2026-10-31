namespace BookHub.Infrastructure.Persistence.Repositories
{
    internal static class SearchExtensions
    {
        public const string EscapeCharacter = "\\";

        public static string ToLikePattern(string search)
        {
            var escaped = search
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");

            return $"%{escaped}%";
        }
    }
}
