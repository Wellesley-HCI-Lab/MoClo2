using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SurfaceApplication1
{
    public class fusionSite
    {
        private string _name2;
        private bool _canDrop2;

        #region Properties
        public string Name2
        {
            get { return _name2; }
        }

        public bool CanDrop2
        {
            get { return _canDrop2; }
        }
        #endregion
       
        public fusionSite(string name, bool canDrop)
        {
            _name2 = name;
            _canDrop2 = canDrop;
        }

    }
}
