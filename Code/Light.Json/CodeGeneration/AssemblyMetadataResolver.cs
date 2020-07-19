using System;
using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;
using Microsoft.CodeAnalysis;

namespace Light.Json.CodeGeneration
{
    public static class AssemblyMetadataResolver
    {
        public static void AddMetadataReferencesFromType(this Dictionary<string, MetadataReference> metadataReferences, Type type)
        {
            type.MustNotBeNull(nameof(type));
            metadataReferences.MustNotBeNull(nameof(metadataReferences));

            metadataReferences.AddMetadataReferencesRecursively(type.Assembly);
        }

        private static void AddMetadataReferencesRecursively(this Dictionary<string, MetadataReference> metadataReferences, Assembly assembly)
        {
            if (metadataReferences.ContainsKey(assembly.FullName!))
                return;

            metadataReferences.Add(assembly.FullName!, MetadataReference.CreateFromFile(assembly.Location));
            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                var referencedAssembly = Assembly.Load(assemblyName.FullName);
                metadataReferences.AddMetadataReferencesRecursively(referencedAssembly);
            }
        }
    }
}
