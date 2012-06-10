using System;
using System.IO;

namespace Tristram.Shared.Network
{
    public interface IPackable
    {
        bool Read(MemoryStream pStream);
        void Write(MemoryStream pStream);
        void Reset();
    }
}
