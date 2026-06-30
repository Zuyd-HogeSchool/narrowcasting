namespace Narrowcasting.Services
{
    /// <summary>
    /// Extension methods for common service operations to reduce code duplication.
    /// Improves cohesion by providing reusable utilities across services.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Validate if an enumerable is empty and throw if required.
        /// </summary>
        public static IEnumerable<T> ValidateNotEmpty<T>(
            this IEnumerable<T> items,
            string itemName)
            where T : class
        {
            if (!items.Any())
                throw new InvalidOperationException($"No {itemName} found.");
            return items;
        }

        /// <summary>
        /// Check if entity exists and throw if not.
        /// </summary>
        public static T ValidateExists<T>(
            this T? item,
            string itemName,
            int id)
            where T : class
        {
            if (item is null)
                throw new InvalidOperationException($"{itemName} with ID {id} not found.");
            return item;
        }

        /// <summary>
        /// Safely get enumerable or empty collection.
        /// </summary>
        public static IEnumerable<T> OrEmpty<T>(
            this IEnumerable<T>? items)
            where T : class
        {
            return items ?? Enumerable.Empty<T>();
        }
    }
}
