﻿// <copyright file="ItemConsumeAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.ItemConsumeActions;

using System.ComponentModel;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Views.Inventory;
using MUnique.OpenMU.Persistence;

/// <summary>
/// Action to consume an item.
/// </summary>
public class ItemConsumeAction
{
    private IDictionary<ItemDefinition, IItemConsumeHandler>? _consumeHandlers;

    /// <summary>
    /// Handles the consume request.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="inventorySlot">The inventory slot.</param>
    /// <param name="inventoryTargetSlot">The inventory target slot.</param>
    /// <param name="fruitUsage">The fruit usage.</param>
    public async ValueTask HandleConsumeRequestAsync(Player player, byte inventorySlot, byte inventoryTargetSlot, FruitUsage fruitUsage)
    {
        var item = player.Inventory?.GetItem(inventorySlot);
        if (item?.Definition is null)
        {
            await player.InvokeViewPlugInAsync<IRequestedItemConsumptionFailedPlugIn>(p => p.RequestedItemConsumptionFailedAsync()).ConfigureAwait(false);
            return;
        }

        this.InitializeConsumeHandlersIfRequired(player.GameContext);
        if (!this._consumeHandlers!.TryGetValue(item.Definition, out var consumeHandler))
        {
            await player.InvokeViewPlugInAsync<IRequestedItemConsumptionFailedPlugIn>(p => p.RequestedItemConsumptionFailedAsync()).ConfigureAwait(false);
            return;
        }

        var targetItem = player.Inventory!.GetItem(inventoryTargetSlot);

        if (player.GameContext.PlugInManager.GetPlugInPoint<IItemConsumingPlugIn>() is { } plugInPoint)
        {
            var eventArgs = new CancelEventArgs();
            plugInPoint.ItemConsuming(player, item, targetItem, eventArgs);
            if (eventArgs.Cancel)
            {
                return;
            }
        }

        if (!await consumeHandler.ConsumeItemAsync(player, item, targetItem, fruitUsage).ConfigureAwait(false))
        {
            await player.InvokeViewPlugInAsync<IRequestedItemConsumptionFailedPlugIn>(p => p.RequestedItemConsumptionFailedAsync()).ConfigureAwait(false);
            return;
        }

        if (item.Durability == 0)
        {
            await player.DestroyInventoryItemAsync(item).ConfigureAwait(false);
        }
        else
        {
            await player.InvokeViewPlugInAsync<IItemDurabilityChangedPlugIn>(p => p.ItemDurabilityChangedAsync(item, true)).ConfigureAwait(false);
        }

        player.GameContext.PlugInManager.GetPlugInPoint<IItemConsumedPlugIn>()?.ItemConsumed(player, item, targetItem);
    }

    private void InitializeConsumeHandlersIfRequired(IGameContext gameContext)
    {
        if (this._consumeHandlers != null)
        {
            return;
        }

        this._consumeHandlers = new Dictionary<ItemDefinition, IItemConsumeHandler>();

        // find all items with configured item consume handler
        var items = gameContext.Configuration.Items.Where(def => !string.IsNullOrEmpty(def.ConsumeHandlerClass));
        foreach (var item in items)
        {
            var consumeHandler = this.CreateConsumeHandler(gameContext, item.ConsumeHandlerClass!);
            this._consumeHandlers.Add(item, consumeHandler);
        }

        IItemConsumeHandler effectHandler = new ApplyMagicEffectConsumeHandler();
        var effectItems = gameContext.Configuration.Items.Where(def => def.ConsumeEffect is not null);
        foreach (var item in effectItems)
        {
            this._consumeHandlers.Add(item, effectHandler);
        }
    }

    private IItemConsumeHandler CreateConsumeHandler(IGameContext gameContext, string handlerTypeName)
    {
        var handlerType = Type.GetType(handlerTypeName);
        if (handlerType is null)
        {
            throw new ArgumentException($"The consume handler {handlerTypeName} wasn't found.", nameof(handlerTypeName));
        }

        var constructors = handlerType.GetConstructors();
        foreach (var ctor in constructors)
        {
            var parameters = ctor.GetParameters();
            if (!parameters.Any())
            {
                return ctor.Invoke(Array.Empty<object>()) as IItemConsumeHandler
                       ?? throw new ArgumentException($"The consume handler {handlerTypeName} isn't implementing {nameof(IItemConsumeHandler)}.", nameof(handlerTypeName));
            }

            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IPersistenceContextProvider))
            {
                return ctor.Invoke(new object[] { gameContext.PersistenceContextProvider }) as IItemConsumeHandler
                       ?? throw new ArgumentException($"The consume handler {handlerTypeName} isn't implementing {nameof(IItemConsumeHandler)}.", nameof(handlerTypeName));
            }
        }

        throw new ArgumentException($"The consume handler {handlerTypeName} has no suitable constructor. One of '{handlerTypeName}()' or '{handlerTypeName}({nameof(IPersistenceContextProvider)}) is required.", nameof(handlerTypeName));
    }
}