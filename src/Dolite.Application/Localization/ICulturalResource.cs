﻿using System.Reflection;
using Microsoft.Extensions.Localization;

namespace Dolite.Application.Localization;

public interface ICulturalResource
{
    static string[] GetAvailableCultures()
    {
        var assembly = Assembly.GetEntryAssembly()!;
        return assembly.GetTypes()
            .Where(type => type.IsAssignableTo(typeof(ICulturalResource)))
            .SelectMany(rootType => assembly.GetTypes()
                .Select(type => type.Name)
                .Where(name => name.StartsWith($"{rootType.Name}_"))
                .Where(name => name != rootType.Name)
                .Select(name => name.Split("_", 2).Last()))
            .Distinct()
            .ToArray();
    }
}

public interface ICulturalResource<TResource> : ICulturalResource
{
    IStringLocalizer<TResource> Localizer { get; init; }
}