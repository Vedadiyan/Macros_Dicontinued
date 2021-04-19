using System;

namespace Macros.Autowire
{
    public class AutowiredAttribute : Attribute
    {
        public string Name { get; set; }
        public Type UnderlyingType { get; }
        public AutowiredAttribute() {

        }
        public AutowiredAttribute(Type underlyingType) {
            UnderlyingType = underlyingType;
        }
    }
}