using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MahTweets.Core.Interfaces
{
    public interface IStorage
    {
        void Store<T>(T store, string filename, IEnumerable<Type> KnownTypes);
        Task<T> Load<T>(string filename);

        bool Exists(string filename);
        string CombineFullPath(string filename);
        string GetFullPath();
        string CombineDocumentsFullPath(string filename);
        string GetDocumentsFullPath();
        string GetApplicationFullPath();
        string GetDecodeTokenisedPath(string applicationScripts);
    }
}