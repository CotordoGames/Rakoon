using System;
using System.Collections.Generic;
using System.Text;

public static class ObjectRegistry
{
    //keywords/ object names to tell the engine to do something if a specific type is found
    static readonly Dictionary<string, Func<GameObject>> factories = new()
    {
        //plain solid rect
        ["solid_rect"] = () => new StaticSolid(),

        //dore
        ["door"] = () => new Door(),

        ["text"] = () => new HUDTest()
    };

    public static GameObject? Create(string name) =>
        factories.TryGetValue(name, out var factory) ? factory() : null;
}