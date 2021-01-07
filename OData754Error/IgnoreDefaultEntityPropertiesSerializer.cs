using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OData_754_Error
{
    public class IgnoreDefaultEntityPropertiesSerializer : ODataResourceSerializer
    {
        private static readonly Double d = 0;
        private static readonly Decimal dec = 0;
        private static readonly Int16 sh = 0;
        private static readonly Int32 i = 0;
        private static readonly Int64 l = 0;
        private static readonly Byte b = 0;

        ILogger Logger;

        public IgnoreDefaultEntityPropertiesSerializer(ODataSerializerProvider provider, ILogger logger) 
            : base(provider) 
        { 
            Logger = logger; 
        }

        /// <summary>
        /// Only return properties that are not null and non-default
        /// </summary>
        /// <param name="structuralProperty">The EDM structural property being written.</param>
        /// <param name="resourceContext">The context for the entity instance being written.</param>
        /// <returns>The property to be written by the serilizer, a null response will effectively skip this property.</returns>
        public override ODataProperty CreateStructuralProperty(IEdmStructuralProperty structuralProperty, ResourceContext resourceContext)
        {
            try
            {
                var property = base.CreateStructuralProperty(structuralProperty, resourceContext);
                if (property.Value == null)
                    return null;

                if (property.Value is string && string.IsNullOrWhiteSpace((string)property.Value))
                    return null;

                if (Equals(property.Value, i))
                    return null;

                if (Equals(property.Value, dec))
                    return null;

                if (Equals(property.Value, sh))
                    return null;

                if (Equals(property.Value, l))
                    return null;

                if (Equals(property.Value, b))
                    return null;

                if (Equals(property.Value, d))
                    return null;

                return property;
            }
            catch(InvalidOperationException e)
            {
                Logger.LogError(e.Message);
                return null;
            }
        }
    }

    /// <summary>
    /// Provider that selects the IngoreNullEntityPropertiesSerializer that omits null properties on resources from the response
    /// </summary>
    public class IgnoreDefaultEntityPropertiesSerializerProvider : DefaultODataSerializerProvider
    {
        private readonly IgnoreDefaultEntityPropertiesSerializer _entityTypeSerializer;

        public IgnoreDefaultEntityPropertiesSerializerProvider(IServiceProvider rootContainer)
            : base(rootContainer)
        {
            _entityTypeSerializer = new IgnoreDefaultEntityPropertiesSerializer(this, (ILogger)rootContainer.GetService(typeof(ILogger)));
        }

        public override ODataEdmTypeSerializer GetEdmTypeSerializer(Microsoft.OData.Edm.IEdmTypeReference edmType)
        {
            // Support for Entity types AND Complex types
            if (edmType.Definition.TypeKind == EdmTypeKind.Entity || edmType.Definition.TypeKind == EdmTypeKind.Complex)
                return _entityTypeSerializer;
            else
                return base.GetEdmTypeSerializer(edmType);
        }
    }

}
