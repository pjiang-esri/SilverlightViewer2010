using System;
using System.Windows;
using System.Collections.Generic;

namespace ESRI.SilverlightViewer.Utility
{
    public class SymbolTypeGroup
    {
        public SymbolTypeGroup()
        {
            SymbolTypes = new List<SymbolType>();
        }

        public string LayerID { get; set; }
        public string LayerName { get; set; }
        public bool LayerVisibility { get; set; }
        public IList<SymbolType> SymbolTypes { get; set; }
    }
}
