using System;
using System.Collections.Generic;
using System.Text;

namespace Essential.Diagnostics
{
    public class StructuredData : IStructuredData
    {
        string _message;
        List<string> _templateKeys = new List<string>();
        List<object> _templateValues;

        public StructuredData(string messageTemplate, params object[] templateValues)
            : this(null, null, messageTemplate, templateValues)
        {
        }

        public StructuredData(Exception exception, string messageTemplate, params object[] templateValues)
            : this(exception, null, messageTemplate, templateValues)
        {
        }

        public StructuredData(IDictionary<string, object> properties, string messageTemplate, params object[] templateValues)
            : this(null, properties, messageTemplate, templateValues)
        {
        }

        public StructuredData(Exception exception, IDictionary<string, object> additionalProperties, string messageTemplate, params object[] templateValues)
        {
            if (additionalProperties == null)
            {
                AdditionalProperties = new Dictionary<string, object>();
            }
            else
            {
                AdditionalProperties = new Dictionary<string, object>(additionalProperties);
            }
            Exception = exception;
            MessageTemplate = messageTemplate;
            _templateValues = new List<object>(templateValues);
            // TODO: Move parsing the message to only when needed
            _message = StringTemplate.Format(MessageTemplate, GetValue);
        }

        public IDictionary<string, object> AdditionalProperties { get; }

        public Exception Exception { get; }

        public string MessageTemplate { get; }

        public IEnumerable<object> TemplateValues { get { return _templateValues; } }

        IDictionary<string, object> IStructuredData.Properties
        {
            get
            {
                // TODO: Generate once and cache
                // TODO: Return null to GetValue for quick grab of names
                var allProperties = new Dictionary<string, object>();
                for (var index = 0; index < _templateKeys.Count; index++)
                {
                    allProperties.Add(_templateKeys[index], _templateValues[index]);
                }
                if (Exception != null)
                {
                    allProperties.Add("Exception", Exception);
                }
                foreach (var kvp in AdditionalProperties)
                {
                    allProperties.Add(kvp.Key, kvp.Value);
                }
                return allProperties;
            }
        }

        public override string ToString()
        {
            return _message;
        }
        
        private bool GetValue(string name, out object value)
        {
            // TODO: Return false (which will throw error) if too many items requested
            // TODO: If lazy, then need to lock for multithread (only init once)
            _templateKeys.Add(name);
            value = _templateValues[_templateKeys.Count - 1];
            return true;
        }
    }
}
