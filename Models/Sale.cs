using System;
using System.Collections.Generic;

namespace AntiqueShopMVC.Models;

public partial class Sale
{
    public int IdSale { get; set; }

    public int? IdUser { get; set; }

    public int? IdItem { get; set; }

    public DateOnly? SaleDate { get; set; }

    public decimal? FinalPrice { get; set; }

    public virtual Item? IdItemNavigation { get; set; }

    public virtual User? IdUserNavigation { get; set; }
}
