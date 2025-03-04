// <copyright file="Item.Generated.cs" company="MUnique">
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
/// A plain implementation of <see cref="Item"/>.
/// </summary>
public partial class Item : MUnique.OpenMU.DataModel.Entities.Item, IIdentifiable, IConvertibleTo<Item>
{
    
    /// <summary>
    /// Gets or sets the identifier of this instance.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets the raw collection of <see cref="ItemOptions" />.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("itemOptions")]
    [System.Text.Json.Serialization.JsonPropertyName("itemOptions")]
    public ICollection<ItemOptionLink> RawItemOptions { get; } = new List<ItemOptionLink>();
    
    /// <inheritdoc/>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public override ICollection<MUnique.OpenMU.DataModel.Entities.ItemOptionLink> ItemOptions
    {
        get => base.ItemOptions ??= new CollectionAdapter<MUnique.OpenMU.DataModel.Entities.ItemOptionLink, ItemOptionLink>(this.RawItemOptions);
        protected set
        {
            this.ItemOptions.Clear();
            foreach (var item in value)
            {
                this.ItemOptions.Add(item);
            }
        }
    }

    /// <summary>
    /// Gets the raw collection of <see cref="ItemSetGroups" />.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("itemSetGroups")]
    [System.Text.Json.Serialization.JsonPropertyName("itemSetGroups")]
    public ICollection<ItemOfItemSet> RawItemSetGroups { get; } = new List<ItemOfItemSet>();
    
    /// <inheritdoc/>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public override ICollection<MUnique.OpenMU.DataModel.Configuration.Items.ItemOfItemSet> ItemSetGroups
    {
        get => base.ItemSetGroups ??= new CollectionAdapter<MUnique.OpenMU.DataModel.Configuration.Items.ItemOfItemSet, ItemOfItemSet>(this.RawItemSetGroups);
        protected set
        {
            this.ItemSetGroups.Clear();
            foreach (var item in value)
            {
                this.ItemSetGroups.Add(item);
            }
        }
    }

    /// <summary>
    /// Gets the raw object of <see cref="Definition" />.
    /// </summary>
    [Newtonsoft.Json.JsonProperty("definition")]
    [System.Text.Json.Serialization.JsonPropertyName("definition")]
    public ItemDefinition RawDefinition
    {
        get => base.Definition as ItemDefinition;
        set => base.Definition = value;
    }

    /// <inheritdoc/>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public override MUnique.OpenMU.DataModel.Configuration.Items.ItemDefinition Definition
    {
        get => base.Definition;
        set => base.Definition = value;
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
    public Item Convert() => this;
}
