using Essential.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;

namespace Essential.Diagnostics
{
    /// <summary>
    /// Holds structured data for logging as key-value properties, along with a templated message.
    /// </summary>
    public class StructuredData : IStructuredData
    {
        public const string ExceptionProperty = "Exception";
        public const string MessageTemplateProperty = "MessageTemplate";

        // Internally use ordered dictionary, to preserve the order passed in
        OrderedDictionary<string, object> _allProperties;
        OrderedDictionary<string, object> _baseProperties;
        Exception _exception;
        string _messageTemplate;
        IList<string> _messageTemplateKeys;
        List<object> _templateValues;
        string _toString;

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

        public IEnumerable<KeyValuePair<string, object>> BaseProperties
        {
            get { return _baseProperties; }
        }

        public int Count
        {
            get
            {
                EnsureAllProperties();
                return _allProperties.Count;
            }
        }

        public Exception Exception {
            get
            {
                if (_exception == null)
                {
                    // Could be from either base properties or template values
                    EnsureAllProperties();
                    object exceptionFromAllProperties;
                    if (_allProperties.TryGetValue(ExceptionProperty, out exceptionFromAllProperties))
                    {
                        if (exceptionFromAllProperties is Exception)
                        {
                            return (Exception)exceptionFromAllProperties;
                        }
                    }
                }
                return _exception;
            }
        }

        public string MessageTemplate {
            get
            {
                EnsureMessageTemplate();
                return _messageTemplate;
            }
        }

        public IEnumerable<object> TemplateValues { get { return _templateValues; } }

        public object this[string key]
        {
            get
            {
                EnsureAllProperties();
                return ((IDictionary<string, object>)_allProperties)[key];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public bool ContainsKey(string key)
        {
            EnsureAllProperties();
            return _allProperties.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            EnsureAllProperties();
            return _allProperties.GetEnumerator();
        }

        public override string ToString()
        {
            EnsureToString();
            return _toString;
        }

        public bool TryGetValue(string key, out object value)
        {
            EnsureAllProperties();
            return _allProperties.TryGetValue(key, out value);
        }

        #region IDictionary

        ICollection<string> IDictionary<string, object>.Keys
        {
            get
            {
                EnsureAllProperties();
                return _allProperties.Keys;
            }
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get
            {
                EnsureAllProperties();
                return _allProperties.Values;
            }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get { return true; }
        }


        void IDictionary<string, object>.Add(string key, object value)
        {
            throw new NotSupportedException();
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            EnsureAllProperties();
            return _allProperties.Contains(item);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            EnsureAllProperties();
            _allProperties.CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            EnsureAllProperties();
            return _allProperties.GetEnumerator();
        }

        #endregion

        private void EnsureAllProperties()
        {
            if (_allProperties == null)
            {
                var allProperties = new OrderedDictionary<string, object>();
                // Get message template, or if not there, see if there is one in base properties
                EnsureMessageTemplate();
                // Start with base properties
                foreach (var kvp in _baseProperties)
                {
                    allProperties.Add(kvp.Key, kvp.Value);
                }
                // If have a template, extract keys
                if (_messageTemplate != null)
                {
                    var keyExtractor = new MessageTemplateKeyExtractor(_messageTemplate);
                    _messageTemplateKeys = keyExtractor.GetKeys();
                }
                else
                {
                    _messageTemplateKeys = new List<string>();
                }
                // Get properties from template values
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
                // Set the template actually used (will overwrite template values)
                if (_messageTemplate != null)
                {
                    ((IDictionary<string, object>)allProperties)[MessageTemplateProperty] = _messageTemplate;
                }
                // Set the exception (will overwrite)
                if (_exception != null)
                {
                    ((IDictionary<string, object>)allProperties)[ExceptionProperty] = _exception;
                }
                _allProperties = allProperties;
            }
        }

        private void EnsureMessageTemplate()
        {
            if (_messageTemplate == null)
            {
                object _basePropertiesMessageTemplate;
                if (_baseProperties.TryGetValue(MessageTemplateProperty, out _basePropertiesMessageTemplate))
                {
                    if (_basePropertiesMessageTemplate is string)
                    {
                        _messageTemplate = (string)_basePropertiesMessageTemplate;
                    }
                }
            }
        }

        private void EnsureToString()
        {
            if (_toString == null)
            {
                EnsureMessageTemplate();
                EnsureAllProperties();
                var writer = new StringWriter();
                var delimiter = "";
                if (_messageTemplate != null)
                {
                    var messageFromTemplate = StringTemplate.Format(_messageTemplate, GetValue);
                    writer.Write(messageFromTemplate);
                    delimiter = "; ";
                }
                var excludeKeys = _messageTemplateKeys;
                excludeKeys.Add(MessageTemplateProperty);
                StructuredPropertyFormatter.FormatProperties(_allProperties, excludeKeys, writer, ref delimiter);
                _toString = writer.GetStringBuilder().ToString();
            }
        }

        private bool GetValue(string name, out object value)
        {
            if (_allProperties.TryGetValue(name, out value))
            {
                if (name.StartsWith("@"))
                {
                    value = StructuredPropertyFormatter.DestructureObject(value);
                }
            }
            else
            {
                value = "{" + name + "}";
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
