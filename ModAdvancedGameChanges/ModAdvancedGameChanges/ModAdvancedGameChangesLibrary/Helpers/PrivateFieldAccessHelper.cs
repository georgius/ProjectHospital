using System;
using System.Reflection;

namespace ModAdvancedGameChanges.Helpers
{
    public sealed class PrivateFieldAccessHelper<TType, TFieldType>
    {
        private FieldInfo m_fieldInfo;
        private TType instance;

        public PrivateFieldAccessHelper(string fieldName, TType instance)
        {
            this.instance = instance;

            // get the gype of the class
            Type type = typeof(TType);

            // get the private field using BindingFlags
            this.m_fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public TFieldType Field
        {
            get
            {
                return (TFieldType)this.m_fieldInfo.GetValue(instance);
            }
            set
            {
                this.m_fieldInfo.SetValue(instance, value);
            }
        }
    }
}
