﻿using System.Collections.Generic;

namespace Essential.Diagnostics
{
    public interface IStructuredData
    {
        IDictionary<string, object> Properties { get; }
        string MessageTemplate { get; }
    }
}
