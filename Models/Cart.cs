using System;
using System.Collections.Generic;

namespace AntiqueShopMVC.Models;

public partial class Cart
{
    public int IdCart { get; set; }

    public int? IdUser { get; set; }

    public int? IdItem { get; set; }

    public int Quantity { get; set; }

    public DateOnly? AddedDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual Item? IdItemNavigation { get; set; }

    public virtual User? IdUserNavigation { get; set; }
}
