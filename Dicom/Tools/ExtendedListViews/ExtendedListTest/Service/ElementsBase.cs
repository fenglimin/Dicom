using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedListTest.Service
{
    public interface IElementsBase
    {
	    ReceivedDicomElements GetReceivedDicomElements();
        string GetLastError();
    }
}
