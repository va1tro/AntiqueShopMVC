using System;
using System.Collections.Generic;

namespace AntiqueShopMVC.Models;

public partial class Supplier
{
    public int IdSupplier { get; set; }

    public string? NameSupplier { get; set; }

    public string? Specialization { get; set; }

    public string? Authenticity { get; set; }

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
