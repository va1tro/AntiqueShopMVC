using System;
using System.Collections.Generic;

namespace AntiqueShopMVC.Models;

public partial class Item
{
    public int IdItem { get; set; }

    public string NameItem { get; set; } = null!;

    public int? Year { get; set; }

    public string? Condition { get; set; }

    public string? Authenticity { get; set; }

    public decimal? PurchasePrice { get; set; }

    public decimal? SellingPrice { get; set; }

    public DateOnly? ArrivalDate { get; set; }

    public string? Image { get; set; }

    public int? IdCategory { get; set; }

    public int? IdMaterial { get; set; }

    public int? IdSupplier { get; set; }

    public int? IdStatus { get; set; }

    public int? IdOriginCountry { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual Category? IdCategoryNavigation { get; set; }

    public virtual Material? IdMaterialNavigation { get; set; }

    public virtual OriginCountry? IdOriginCountryNavigation { get; set; }

    public virtual Status? IdStatusNavigation { get; set; }

    public virtual Supplier? IdSupplierNavigation { get; set; }

    public virtual ICollection<ItemLog> ItemLogs { get; set; } = new List<ItemLog>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
