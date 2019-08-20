using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    /// <summary>
    /// Simple IEntity interface which works as a filter to prevent committing no POCO entities into IMailStorage.
    /// </summary>
    public interface IEntity
    {
    }
}
