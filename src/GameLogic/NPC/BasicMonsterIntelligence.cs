﻿// <copyright file="BasicMonsterIntelligence.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.NPC;

using System.Diagnostics;
using System.Threading;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// A basic monster AI which is pretty basic.
/// </summary>
public class BasicMonsterIntelligence : INpcIntelligence, IDisposable
{
    private Timer? _aiTimer;
    private Monster? _monster;

    /// <summary>
    /// Finalizes an instance of the <see cref="BasicMonsterIntelligence"/> class.
    /// </summary>
    ~BasicMonsterIntelligence()
    {
        this.Dispose();
    }

    /// <inheritdoc/>
    public NonPlayerCharacter Npc
    {
        get => this.Monster;
        set => this.Monster = (Monster)value;
    }

    /// <summary>
    /// Gets or sets the monster.
    /// </summary>
    public Monster Monster
    {
        get => this._monster ?? throw new InvalidOperationException("Instance is not initialized with a Monster yet");
        set => this._monster = value;
    }

    /// <summary>
    /// Gets the current target.
    /// </summary>
    protected IAttackable? CurrentTarget { get; private set; }

    private async ValueTask<bool> IsObservedByAttackerAsync()
    {
        using var readerLock = await this.Monster.ObserverLock.ReaderLockAsync();
        return this.Monster.Observers.OfType<IAttacker>().Any();
    }

    /// <inheritdoc/>
    public void Start()
    {
        // TODO: Optimize this: start timer when first observer is added. stop timer when last observer is removed.
        this._aiTimer = new Timer(_ => this.SafeTick(), null, this.Npc.Definition.AttackDelay, this.Npc.Definition.AttackDelay);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public virtual void RegisterHit(IAttacker attacker)
    {
        if (this.CurrentTarget is null && attacker is IAttackable attackable)
        {
            this.CurrentTarget = attackable;
        }
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="managed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool managed)
    {
        this._aiTimer?.Dispose();
        this._aiTimer = null;
    }

    /// <summary>
    /// Searches the next target in view range.
    /// </summary>
    /// <returns>The next target.</returns>
    protected virtual async ValueTask<IAttackable?> SearchNextTargetAsync()
    {
        List<IWorldObserver> tempObservers;
        using (await this.Npc.ObserverLock.ReaderLockAsync())
        {
            if (this.Npc.Observers.Count == 0)
            {
                return null;
            }

            tempObservers = new List<IWorldObserver>(this.Npc.Observers);
        }

        var possibleTargets = tempObservers.OfType<IAttackable>()
            .Where(a => a.IsActive() && !a.IsAtSafezone())
            .ToList();
        var summons = possibleTargets.OfType<Player>()
            .Select(p => p.Summon?.Item1)
            .Where(s => s is not null)
            .Cast<IAttackable>()
            .WhereActive()
            .ToList();
        possibleTargets.AddRange(summons);
        var closestTarget = possibleTargets.MinBy(a => a.GetDistanceTo(this.Npc));

        return closestTarget;

        // todo: check the walk distance
    }

    /// <summary>
    /// Determines whether this instance can attack.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if this instance can attack; otherwise, <c>false</c>.
    /// </returns>
    protected virtual ValueTask<bool> CanAttackAsync() => ValueTask.FromResult(true);

    private async ValueTask<bool> IsTargetInObserversAsync(IAttackable target)
    {
        using (await this.Npc.ObserverLock.ReaderLockAsync())
        {
            return target is IWorldObserver worldObserver && this.Npc.Observers.Contains(worldObserver);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "Catching all Exceptions.")]
    private async void SafeTick()
    {
        try
        {
            await this.TickAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // can be ignored.
        }
        catch (Exception ex)
        {
            Debug.Fail(ex.Message, ex.StackTrace);
        }
    }

    private async ValueTask TickAsync()
    {
        if (!this.Monster.IsAlive)
        {
            return;
        }

        if (this.Monster.IsWalking)
        {
            return;
        }

        if (this.Monster.Attributes[Stats.IsStunned] > 0)
        {
            return;
        }

        if (!await this.CanAttackAsync().ConfigureAwait(false))
        {
            return;
        }

        var target = this.CurrentTarget;
        if (target != null)
        {
            // Old Target out of Range?
            if (!target.IsAlive
                || target.IsTeleporting
                || target.IsAtSafezone()
                || !target.IsInRange(this.Monster.Position, this.Npc.Definition.ViewRange)
                || (target is IWorldObserver && !await this.IsTargetInObserversAsync(target).ConfigureAwait(false)))
            {
                target = this.CurrentTarget = await this.SearchNextTargetAsync().ConfigureAwait(false);
            }
        }
        else
        {
            target = this.CurrentTarget = await this.SearchNextTargetAsync().ConfigureAwait(false);
        }

        // no target?
        if (target is null)
        {
            if (this.Monster.Attributes[Stats.IsFrozen] > 0)
            {
                return;
            }

            // we move around randomly, so the monster does not look dead when watched from distance.
            if (await this.IsObservedByAttackerAsync().ConfigureAwait(false))
            {
                await this.Monster.RandomMoveAsync().ConfigureAwait(false);
            }

            return;
        }

        // Target in Attack Range?
        if (target.IsInRange(this.Monster.Position, this.Monster.Definition.AttackRange + 1) && !this.Monster.IsAtSafezone())
        {
            await this.Monster.AttackAsync(target).ConfigureAwait(false);  // yes, attack
        }

        if (this.Monster.Attributes[Stats.IsFrozen] > 0)
        {
            return;
        }

        // Target in View Range?
        else if (target.IsInRange(this.Monster.Position, this.Monster.Definition.ViewRange + 1))
        {
            // no, walk to the target
            var walkTarget = this.Monster.CurrentMap!.Terrain.GetRandomCoordinate(target.Position, this.Monster.Definition.AttackRange);
            await this.Monster.WalkToAsync(walkTarget).ConfigureAwait(false);
        }
        else
        {
            // we move around randomly, so the monster does not look dead when watched from distance.
            await this.Monster.RandomMoveAsync().ConfigureAwait(false);
        }
    }
}