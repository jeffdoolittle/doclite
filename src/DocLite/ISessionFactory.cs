using System;

namespace DocLite
{
    /// <summary>
    /// A Session Factory acts as the factory for a doclite <see cref="ISession"/>
    /// </summary>
    public interface ISessionFactory : IDisposable
    {
        /// <summary>
        /// Opens a new <see cref="ISession"/>
        /// </summary>
        /// <returns>A new <see cref="ISession"/></returns>
        ISession OpenSession();
    }
}