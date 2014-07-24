using System;
using System.Collections.Generic;

namespace DocLite
{
    /// <summary>
    /// A Session enables the retrieval, addition, and removal of documents from a backing store (persistent, or in memory)
    /// </summary>
    public interface ISession : IDisposable
    {
        /// <summary>
        /// Gets a document by Type and Id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T Get<T>(object id = null);

        /// <summary>
        /// Gets documents matching Type and Ids
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids"></param>
        /// <returns></returns>
        IEnumerable<T> Get<T>(object[] ids);

        /// <summary>
        /// Returns a paged enumerable for documents of a Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        IEnumerable<T> Get<T>(int skip, int take);
        
        /// <summary>
        /// Gets all documents for a Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<T> GetAll<T>();

        /// <summary>
        /// Adds a document to the session
        /// </summary>
        /// <param name="document"></param>
        void Add(object document);
        
        /// <summary>
        /// Removes a document from the session
        /// </summary>
        /// <param name="document"></param>
        void Remove(object document);

        /// <summary>
        /// Returns the first document of a Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T First<T>();

        /// <summary>
        /// Returns the last document of a Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Last<T>();

        /// <summary>
        /// Returns a single document of a Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Single<T>();

        /// <summary>
        /// Returns the first document of a Type, null if none found
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T FirstOrDefault<T>();

        /// <summary>
        /// Returns the last document of a Type, null if none found
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T LastOrDefault<T>();

        /// <summary>
        /// Returns a single document of a Type, null if none found
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T SingleOrDefault<T>();

        /// <summary>
        /// Returns true if documents exist of the specified Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool Any<T>();
    }
}