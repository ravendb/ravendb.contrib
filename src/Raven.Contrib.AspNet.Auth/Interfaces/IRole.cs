using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.Contrib.AspNet.Auth.Interfaces
{
    public interface IRole<T>
    {
        /// <summary>
        /// The role name.
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The role users.
        /// </summary>
        ICollection<T> Users
        {
            get;
            set;
        }
    }
}