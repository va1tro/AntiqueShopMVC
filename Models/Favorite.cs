using System;
using System.Collections.Generic;

namespace AntiqueShopMVC.Models;

public partial class Favorite
{
    public int IdFavorite { get; set; }

    public int? IdUser { get; set; }

    public int? IdItem { get; set; }

    public virtual Item? IdItemNavigation { get; set; }

    public virtual User? IdUserNavigation { get; set; }
}
