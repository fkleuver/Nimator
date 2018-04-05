namespace Nimator.Util
{
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            Guard.AgainstNegative(nameof(maxLength), maxLength);
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
