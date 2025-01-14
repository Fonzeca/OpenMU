// <copyright file="ItemOfItemSet.Generated.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

//------------------------------------------------------------------------------
// <auto-generated>
//     This source code was auto-generated by a roslyn code generator.
// </auto-generated>
//------------------------------------------------------------------------------

// ReSharper disable All

namespace MUnique.OpenMU.Persistence.BasicModel;

using MUnique.OpenMU.Persistence.Json;

/// <summary>
/// A plain implementation of <see cref="ItemOfItemSet"/>.
/// </summary>
public partial class ItemOfItemSet : MUnique.OpenMU.DataModel.Configuration.Items.ItemOfItemSet, IIdentifiable, IConvertibleTo<ItemOfItemSet>
{
    
    /// <summary>
    /// Gets or sets the identifier of this instance.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets the raw object of <see cref="ItemSetGroup" />.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("itemSetGroup")]
    [System.Text.Json.Serialization.JsonPropertyName("itemSetGroup")]
    public ItemSetGroup RawItemSetGroup
    {
        get => base.ItemSetGroup as ItemSetGroup;
        set => base.ItemSetGroup = value;
    }

    /// <inheritdoc/>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public override MUnique.OpenMU.DataModel.Configuration.Items.ItemSetGroup ItemSetGroup
    {
        get => base.ItemSetGroup;
        set => base.ItemSetGroup = value;
    }

    /// <summary>
    /// Gets the raw object of <see cref="ItemDefinition" />.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("itemDefinition")]
    [System.Text.Json.Serialization.JsonPropertyName("itemDefinition")]
    public ItemDefinition RawItemDefinition
    {
        get => base.ItemDefinition as ItemDefinition;
        set => base.ItemDefinition = value;
    }

    /// <inheritdoc/>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public override MUnique.OpenMU.DataModel.Configuration.Items.ItemDefinition ItemDefinition
    {
        get => base.ItemDefinition;
        set => base.ItemDefinition = value;
    }

    /// <summary>
    /// Gets the raw object of <see cref="BonusOption" />.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("bonusOption")]
    [System.Text.Json.Serialization.JsonPropertyName("bonusOption")]
    public IncreasableItemOption RawBonusOption
    {
        get => base.BonusOption as IncreasableItemOption;
        set => base.BonusOption = value;
    }

    /// <inheritdoc/>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public override MUnique.OpenMU.DataModel.Configuration.Items.IncreasableItemOption BonusOption
    {
        get => base.BonusOption;
        set => base.BonusOption = value;
    }


    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        var baseObject = obj as IIdentifiable;
        if (baseObject != null)
        {
            return baseObject.Id == this.Id;
        }

        return base.Equals(obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }

    /// <inheritdoc/>
    public ItemOfItemSet Convert() => this;
}
