using System;
using System.Windows;
using ESRI.ArcGIS.Client.Symbols;

namespace ESRI.SilverlightViewer.Utility
{
    public class SymbolType
    {
        public SymbolType() { }
        public SymbolType(string name, object typeID, Symbol symbol, string desc)
        {
            this.Name = name;
            this.TypeID = typeID;
            this.Symbol = symbol;
            this.Description = desc;
        }

        public string Name { get; set; }
        public object TypeID { get; set; }
        public Symbol Symbol { get; set; }
        public string Description { get; set; }
    }
}
