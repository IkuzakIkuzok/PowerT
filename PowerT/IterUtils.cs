﻿
// (c) 2024 Kazuki Kohzuki

namespace PowerT;

/// <summary>
/// Utility methods for iteration.
/// </summary>
internal static class IterUtils
{
    /// <summary>
    /// Enumerates the elements of the source.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>The enumeration of the elements with their index.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
    internal static IEnumerable<(int index, T element)> Enumerate<T>(this IEnumerable<T> source, int offset = 0)
    {
#pragma warning disable Ex0105  // Exception handler does not work well in iterators.
        ArgumentNullException.ThrowIfNull(source);
#pragma warning restore

        var index = offset;
        foreach (var element in source)
            yield return (index++, element);
    } // internal static IEnumerable<(int index, T element)> Enumerate<T> (this IEnumerable<T>, [int])
} // internal static class IterUtils
