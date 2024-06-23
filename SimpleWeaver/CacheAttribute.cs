using System;

/// <summary>
/// Namespace to use.
/// </summary>
[AttributeUsage(AttributeTargets.Method )]
public class CacheAttribute : Attribute
{
    /// <summary>
    /// Initialize a new instance of <see cref="CacheAttribute"/>
    /// </summary>
    public CacheAttribute()
    {
    }
}