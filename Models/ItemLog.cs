using System;
using System.Collections.Generic;

namespace AntiqueShopMVC.Models;

public partial class ItemLog
{
    public int IdLog { get; set; }

    public int? IdItem { get; set; }

    public string? ChangedField { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime? ChangeDate { get; set; }

    public virtual Item? IdItemNavigation { get; set; }
}
