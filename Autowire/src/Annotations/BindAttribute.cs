using System;

namespace Macros.Autowire
{
    public class BindAttribute : Attribute
    {
        public BindingMethods BindingMethod { get; set; }
        public string Name { get; set; }
        public Type UnderlyingType { get; }
        public BindAttribute(Type underlyingType) {
            UnderlyingType = underlyingType;
        }
    }
    public enum BindingMethods
    {
        SINGLETON,
        RELATIVE
    }
}