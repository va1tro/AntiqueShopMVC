using System;
using System.Collections.Generic;

namespace AntiqueShopMVC.Models;

public partial class Material
{
    public int IdMaterial { get; set; }

    public string NameMaterial { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
