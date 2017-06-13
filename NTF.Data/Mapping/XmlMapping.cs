using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NTF.Data.Mapping
{
    public class XmlMapping : AttributeMapping
    {
        Dictionary<string, XElement> entities;
        private static readonly XName Entity = XName.Get("Entity");
        private static readonly XName Id = XName.Get("Id");
        
        public XmlMapping(XElement root)
            : base(null)
        {
            this.entities = root.Elements().Where(e => e.Name == Entity).ToDictionary(e => (string)e.Attribute(Id));
        }

        public static XmlMapping FromXml(string xml)
        {
            return new XmlMapping(XElement.Parse(xml));
        }

        protected override IEnumerable<MappingAttribute> GetMappingAttributes(string rootEntityId)       
        {
            XElement root;
            if (this.entities.TryGetValue(rootEntityId, out root))
            {
                foreach (var elem in root.Elements())
                {
                    if (elem != null)
                    {
                        yield return this.GetMappingAttribute(elem);
                    }
                }
            }
        }

        private MappingAttribute GetMappingAttribute(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "Table":
                    return this.GetMappingAttribute(typeof(TableAttribute), element);
                case "ExtensionTable":
                    return this.GetMappingAttribute(typeof(ExtensionTableAttribute), element);
                case "Column":
                    return this.GetMappingAttribute(typeof(ColumnAttribute), element);
                case "Association":
                    return this.GetMappingAttribute(typeof(AssociationAttribute), element);
                default:
                    return null;
            }
        }

        private MappingAttribute GetMappingAttribute(Type attrType, XElement element)
        {
            var ma = (MappingAttribute)Activator.CreateInstance(attrType);
            foreach (var prop in attrType.GetProperties())
            {
                var xa = element.Attribute(prop.Name);
                if (xa != null)
                {
                    if (prop.PropertyType == typeof(Type))
                    {
                        prop.SetValue(ma, this.FindType(xa.Value), null);
                    }
                    else
                    {
                        prop.SetValue(ma, Convert.ChangeType(xa.Value, prop.PropertyType), null);
                    }
                }
            }
            return ma;
        }

        private Type FindType(string name)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(name);
                if (type != null)
                    return type;
            }
            return null;
        }
    }
}