﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Disposables;

namespace Bonsai
{
    public interface ILoadable
    {
        IDisposable Load();
    }
}
