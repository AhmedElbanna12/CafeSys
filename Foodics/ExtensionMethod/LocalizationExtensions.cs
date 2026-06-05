namespace Foodics.ExtensionMethod
{
    public static class LocalizationExtensions
    {
        public static string Localize(
            string? ar,
            string? en,
            string lang)
        {
            if (lang == "ar")
                return string.IsNullOrWhiteSpace(ar)
                    ? en ?? string.Empty
                    : ar;

            return string.IsNullOrWhiteSpace(en)
                ? ar ?? string.Empty
                : en;
        }
    }
}
