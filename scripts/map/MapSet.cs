using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

public class MapSet
{
    public string Directory { get; set; }

    public List<Map> Maps { get; set; }

    public string Artist => Maps.First().Artist;

    public string Title => Maps.First().Title;

    //public string Mappers => Maps.First().Mappers;
}
