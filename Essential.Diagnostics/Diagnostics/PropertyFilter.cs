using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Essential.Diagnostics
{
    public class PropertyFilter : TraceFilter
    {
        private string propertyTemplate;
        private string comparisonValue;

        public PropertyFilter(string propertyComparison)
        {
            var comparatorPosition = propertyComparison.IndexOf("==");
            if (comparatorPosition < 1 || comparatorPosition > propertyComparison.Length - 2)
            {
                throw new ArgumentException("Property comparison must contain a comparison ('==') with a property and value, e.g. initializeData=\"id == 1\".", "propertyComparison");
            }

            propertyTemplate = "{" + propertyComparison.Substring(0, comparatorPosition).Trim() + "}";
            comparisonValue = propertyComparison.Substring(comparatorPosition + 2).Trim();

            if (comparisonValue.Length >= 2)
            {
                if ((comparisonValue.StartsWith("\"") && comparisonValue.EndsWith("\""))
                    || (comparisonValue.StartsWith("'") && comparisonValue.EndsWith("'")))
                {
                    comparisonValue = comparisonValue.Substring(1, comparisonValue.Length - 2);
                }
            }
        }

        public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1, object[] data)
        {
            if (data == null)
            {
                data = new object[] { data1 };
            }
            string propertyValue = TraceTemplate.Format(propertyTemplate, cache, source, eventType, id, null,
                                                        null, data);
            return propertyValue.Equals(comparisonValue);
        }
    }
}
