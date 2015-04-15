using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EPiServer.Localization.Models
{
    public interface IXmlImportable
    {
        void SetPageData(XmlNode node);
    }
}
