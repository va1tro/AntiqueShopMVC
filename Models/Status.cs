using System;
using System.Collections.Generic;

namespace AntiqueShopMVC.Models;

public partial class Status
{
    public int IdStatus { get; set; }

    public string NameStatus { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
