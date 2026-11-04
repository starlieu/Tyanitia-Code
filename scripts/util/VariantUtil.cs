using System.Collections;
using System.Collections.Generic;
using Godot;

public class VariantUtil
{
    public static T TryCast<T>(Variant variant)
    {
        if (variant is T type)
        {
            return type;
        }
        else
        {
            return default;
        }
    }

    public static IList<Variant> ToList<[MustBeVariant] T>(IEnumerable<T> values)
    {
        var list = new List<Variant>();

        foreach (var v in values)
        {
            list.Add(Variant.From(v));
        }

        return list;
    }
}
