using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    /// <summary> Will decide when to send a especific type of emails, or just chill /// </summary>/// <remarks>
    /// <code>0 No Changes: Value of the asset still the same or not low/high enough to send a email".
    /// <para>1 TimeToSell: Value of the asset is higher than the minimun defined to sell.
    /// </para>2 TimeToBuy: Value of the asset is lower than the maximun defined to buy.
    /// </code></remarks>
    public enum AssetsAction
    {
        NoChanges = 0,
        TimeToSell = 1,
        TimeToBuy = 2
    }
}
