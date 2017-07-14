using Essential.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Holds structured data for logging as key-value properties, along with a templated message.
    /// </summary>
    public class StructuredData : IStructuredData
    {
        OrderedDictionary<string, object> _allProperties;
        OrderedDictionary<string, object> _baseProperties;
        Exception _exception;
        string _message;
        string _messageTemplate;
        IList<string> _messageTemplateKeys;
        List<object> _templateValues;

        /// <summary>
        /// Constructor. Creates structured data with the specified properties.
        /// </summary>
        /// <param name="properties">The key-value properties to trace</param>
        public StructuredData(IEnumerable<KeyValuePair<string, object>> properties)
            : this(properties, null, null, null)
        {
        }

        /// <summary>
        /// Constructor. Creates structured data with the specified properties, message template, and (optional) override template values.
        /// </summary>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Optional values, assigned in sequence, to keys in the template</param>
        public StructuredData(IEnumerable<KeyValuePair<string, object>> properties, string messageTemplate, params object[] templateValues)
            : this(properties, null, messageTemplate, templateValues)
        {
        }

        /// <summary>
        /// Constructor. Creates structured data with the specified message template, and template values.
        /// </summary>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public StructuredData(string messageTemplate, params object[] templateValues)
            : this(null, null, messageTemplate, templateValues)
        {
        }

        /// <summary>
        /// Constructor. Creates structured data with the specified exception, message template, and template values.
        /// </summary>
        /// <param name="exception">The Exception to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Values, assigned in sequence, to keys in the template</param>
        public StructuredData(Exception exception, string messageTemplate, params object[] templateValues)
            : this(null, exception, messageTemplate, templateValues)
        {
        }

        /// <summary>
        /// Constructor. Creates structured data with the specified properties, exception, message template, and (optional) override template values.
        /// </summary>
        /// <param name="properties">The key-value properties to trace</param>
        /// <param name="exception">The Exception to trace</param>
        /// <param name="messageTemplate">Message template to insert properties into, containing keys</param>
        /// <param name="templateValues">Optional values, assigned in sequence, to keys in the template</param>
        /// <remarks>
        /// <para>
        /// Note that the relationship between messageTemplate and templateValues is flexible.
        /// </para>
        /// <para>
        /// The IStructuredData.Properties dictionary is built from the properties parameter, if present, 
        /// with the exception parameter, if present, overriding the "Exception" property.
        /// </para>
        /// <para>
        /// Items in templateValues, if any, are then added to the properties using the matching key in sequence from
        /// messageTemplate, overriding any existing value in properties. Note that templateValues are not needed, and 
        /// there is no error if there are less values than messageTemplate keys. If there are more values than keys, 
        /// then they are added as "Extra1", "Extra2", etc.
        /// </para>
        /// </remarks>
        public StructuredData(IEnumerable<KeyValuePair<string, object>> properties, Exception exception, string messageTemplate, params object[] templateValues)
        {
            if (properties == null)
            {
                _baseProperties = new OrderedDictionary<string, object>();
            }
            else
            {
                _baseProperties = new OrderedDictionary<string, object>(properties);
            }
            _exception = exception;
            _messageTemplate = messageTemplate;
            if (templateValues == null)
            {
                _templateValues = new List<object>();
            }
            else
            {
                _templateValues = new List<object>(templateValues);
            }
        }

        public IEnumerable<KeyValuePair<string, object>> BaseProperties { get { return _baseProperties; } }

        public Exception Exception { get { return _exception; } }

        public string MessageTemplate { get { return _messageTemplate; } }

        public IEnumerable<object> TemplateValues { get { return _templateValues; } }

        IDictionary<string, object> IStructuredData.Properties
        {
            get
            {
                BuildAllProperties();
                return _allProperties;
            }
        }

        public override string ToString()
        {
            BuildMessage();
            return _message;
        }

        private void BuildAllProperties()
        {
            if (_allProperties == null)
            {
                var allProperties = new OrderedDictionary<string, object>();
                foreach (var kvp in _baseProperties)
                {
                    allProperties.Add(kvp.Key, kvp.Value);
                }
                if (_exception != null)
                {
                    ((IDictionary<string,object>)allProperties)["Exception"] = _exception;
                }
                if (_messageTemplate != null)
                {
                    var keyExtractor = new MessageTemplateKeyExtractor(_messageTemplate);
                    _messageTemplateKeys = keyExtractor.GetKeys();
                }
                else
                {
                    _messageTemplateKeys = new List<string>();
                }
                if (_templateValues != null && _templateValues.Count > 0)
                {
                    var extraCount = 0;
                    for (var index = 0; index < _templateValues.Count; index++)
                    {
                        if (index < _messageTemplateKeys.Count)
                        {
                            ((IDictionary<string,object>)allProperties)[_messageTemplateKeys[index]] = _templateValues[index];
                        }
                        else
                        {
                            extraCount++;
                            ((IDictionary<string, object>)allProperties)[string.Format("Extra{0}", extraCount)] = _templateValues[index];
                        }
                    }
                }
                _allProperties = allProperties;
            }
        }

        private void BuildMessage()
        {
            if (_message == null)
            {
                BuildAllProperties();
                var writer = new StringWriter();
                var delimiter = "";
                if (_messageTemplate != null)
                {
                    var messageFromTemplate = StringTemplate.Format(_messageTemplate, GetValue);
                    writer.Write(messageFromTemplate);
                    delimiter = "; ";
                }
                StructuredPropertyFormatter.FormatProperties(_allProperties, _messageTemplateKeys, writer, ref delimiter);
                _message = writer.GetStringBuilder().ToString();
            }
        }

        private bool GetValue(string name, out object value)
        {
            if (!_allProperties.TryGetValue(name, out value))
            {
                value = null;
            }
            return true;
        }


        class MessageTemplateKeyExtractor
        {
            string _messageTemplate;
            IList<string> _keys;

            public MessageTemplateKeyExtractor(string messageTemplate)
            {
                _messageTemplate = messageTemplate;
            }

            public IList<string> GetKeys()
            {
                if (_keys == null)
                {
                    _keys = new List<string>();
                    var dummy = StringTemplate.Format(_messageTemplate, GetValue);
                }
                return _keys;
            }

            private bool GetValue(string name, out object value)
            {
                _keys.Add(name);
                value = null;
                return true;
            }
        }
    }
}
