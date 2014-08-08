using System;
using System.IO;
using System.Reflection;

namespace PhoneNumbers
{
    /// <summary>
    /// Interface for caller to specify customized phone metadata loader.
    /// </summary>
    public interface MetadataLoader
    {
        /// <summary>
        /// Returns an input stream corresponding to the metadata to load.
        /// </summary>
        /// <param name = "metadataFileName"> File name (including path) of metadata to load. File path is an
        /// absolute class path like /com/google/i18n/phonenumbers/data/PhoneNumberMetadataProto.</param>
        /// <returns>The input stream for the metadata file. The library will close this stream
        /// after it is done. Return null in case the metadata file could not be found.</returns>
        Stream LoadMetadata(String metadataFileName);
    }

    public class DefaultMetadataLoader : MetadataLoader
    {
        public Stream LoadMetadata(String metadataFileName)
        {
            var asm = Assembly.GetExecutingAssembly();
            return asm.GetManifestResourceStream(metadataFileName);
        }
    }
}
