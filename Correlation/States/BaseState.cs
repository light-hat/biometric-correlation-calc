using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Correlation.States
{
    public abstract class BaseState
    {
        /// <summary>
        /// Имя конкретного состояния.
        /// </summary>
        public abstract String StateName { get; }
    }
}
