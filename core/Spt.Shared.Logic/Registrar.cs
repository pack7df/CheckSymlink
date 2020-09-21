using System;
using System.Collections.Generic;

namespace Spt.Shared.Logic
{
    public static class Registrar
    {
        public static readonly Dictionary<string, Type> QueryHandlers = new Dictionary<string, Type>();
    }
}
