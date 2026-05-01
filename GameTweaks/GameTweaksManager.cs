using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameTweaks.Tweaks;
using HarmonyLib;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace GameTweaks;

[RegisterInIl2Cpp]
public class GameTweaksManager(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    public static GameTweaksManager? Instance { get; set; }
    public static Dictionary<ushort, Type> TweakTypes { get; private set; } = new();
    public static List<AbstractGameTweak> AllTweaks { get; private set; } = [];
    private static ushort nextId { get; set; } = 0;

    public readonly List<AbstractGameTweak> ActiveTweaks = [];
    public readonly Queue<Type> PendingTweaks = new();
    public readonly List<AbstractGameTweak> TweaksAddedLater = []; // this is basically just to add an indicator for anomaly tweaks

    void Start()
    {
        Instance = this;
        foreach (var (_, type) in TweakTypes)
        {
            var tweak = CreateTweak(type);
            if (!tweak.IsEnabled())
            {
                continue;
            }

            ActiveTweaks.Add(tweak);
            tweak.OnAdd();
        }

        ActiveTweaks.Do(x => x.Start());
        ProcessPending();
        Warning($"{ActiveTweaks.Count} tweaks active");
    }

    void FixedUpdate()
    {
        foreach (var tweak in ActiveTweaks)
        {
            tweak.Update();
        }

        ProcessPending();
    }

    private void ProcessPending()
    {
        while (PendingTweaks.Count > 0)
        {
            var type = PendingTweaks.Dequeue();
            var tweak = CreateTweak(type);
            ActiveTweaks.Add(tweak);
            TweaksAddedLater.Add(tweak);
            tweak.OnAdd();
            tweak.Start();
        }
    }

    internal static AbstractGameTweak CreateTweak(Type type)
    {
        var tweak = (Activator.CreateInstance(type) as AbstractGameTweak)!;
        var id = TweakTypes.FirstOrDefault(x => x.Value == type).Key;
        tweak.Id = id;
        return tweak;
    }

    public static void RegisterTweaks(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsSubclassOf(typeof(AbstractGameTweak)))
            {
                continue;
            }
            if (type.IsAbstract)
            {
                continue;
            }

            var tweak = Activator.CreateInstance(type) as AbstractGameTweak;
            var id = nextId++;
            tweak!.Id = id;
            AllTweaks.Add(tweak);
            TweakTypes.Add(id, type);
        }
    }

    public void AddTweak(Type tweak)
    {
        if (ActiveTweaks.Any(x => x.GetType() == tweak) || PendingTweaks.Any(x => x == tweak))
        {
            return;
        }

        PendingTweaks.Enqueue(tweak);
    }

    public Type? GetTweakType(ushort id)
    {
        return TweakTypes.GetValueOrDefault(id);
    }

    public bool IsActive<T>() where T : AbstractGameTweak
    {
        return ActiveTweaks.OfType<T>().Any();
    }
    public bool IsActive(ushort id)
    {
        return ActiveTweaks.Any(x => x.Id == id);
    }
}