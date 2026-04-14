using UnityEngine;

namespace GameTweaks.Tweaks;

public abstract class AbstractGameTweak
{
    /// <summary>
    /// Name of the tweak.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Color of the tweak.
    /// </summary>
    public abstract Color Color { get; }

    /// <summary>
    /// ID of the tweak, used in networking.
    /// </summary>
    public ushort Id { get; internal set; }

    /// <summary>
    /// Is the tweak enabled and should be added on game start.
    /// </summary>
    /// <returns>Enabled.</returns>
    public abstract bool IsEnabled();

    /// <summary>
    /// Update method that runs under manager's FixedUpdate.
    /// </summary>
    public virtual void Update() {}

    /// <summary>
    /// Called when the tweak is added to <see cref="GameTweaksManager.ActiveTweaks"/>.
    /// </summary>
    public virtual void OnAdd() {}

    /// <summary>
    /// Called after all tweaks are added to the list and ready.
    /// </summary>
    public virtual void Start() {}

    /// <summary>
    /// Called when the round starts.
    /// </summary>
    public virtual void OnRoundStart(bool gameStart) {}
}