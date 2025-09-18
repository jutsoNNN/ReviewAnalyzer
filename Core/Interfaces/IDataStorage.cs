using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewAnalyzer.Core.Interfaces
{
    public interface IDataStorage
    {
        string Save<T>(string fileName, T data);
        T? Load<T>(string filePath);
    }
}