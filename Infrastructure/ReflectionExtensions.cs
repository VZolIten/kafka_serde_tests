namespace KafkaTest.Infrastructure;

public static class ReflectionExtensions
{
    /// <summary>
    /// Get value of the property
    /// </summary>
    /// <param name="type">Type to inspect</param>
    /// <param name="propName">The property name</param>
    /// <param name="context">Object to use as this. Null means <b>static</b> property</param>
    /// <typeparam name="TReturn">Result type to cast to</typeparam>
    /// <returns>Property of type TReturn</returns>
    /// <exception cref="ArgumentException">There is no property with requested name</exception>
    /// <exception cref="InvalidCastException">Property value is not convertable to the <b>TReturn</b> type</exception>
    public static TReturn? GetPropertyValue<TReturn>(this Type type, string propName, object? context = null)
        where TReturn : class
    {
        ArgumentException.ThrowIfNullOrEmpty(propName);

        var property = type.GetProperty(propName)
                       ?? throw new ArgumentException($"Unable to find property {propName} in {type.Name}");

        return (TReturn?) property.GetValue(context);
    }
}