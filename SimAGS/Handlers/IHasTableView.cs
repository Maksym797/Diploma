﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimAGS.Handlers
{
    public interface IHasTableView
    {
        string[] AsArrayForRow();
        string[] GetHeaders();
    }
}
