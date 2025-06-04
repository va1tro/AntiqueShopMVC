using System;
using System.Collections.Generic;

namespace AntiqueShopMVC.Models;

public partial class OriginCountry
{
    public int IdOriginCountry { get; set; }

    public string NameCountry { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
