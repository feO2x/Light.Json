using System;
using System.Collections.Generic;
using System.Reflection;
using Light.GuardClauses;
using Microsoft.CodeAnalysis;

namespace Light.Json.CodeGeneration
{
    public static class AssemblyMetadataResolver
    {
        public static void AddMetadataReferencesFromType(Type type, Dictionary<string, MetadataReference> metadataReferences)
        {
            type.MustNotBeNull(nameof(type));
            metadataReferences.MustNotBeNull(nameof(metadataReferences));

            AddMetadataReferencesRecursively(type.Assembly, metadataReferences);
        }

        private static void AddMetadataReferencesRecursively(Assembly assembly, Dictionary<string, MetadataReference> metadataReferences)
        {
            if (metadataReferences.ContainsKey(assembly.FullName!))
                return;

            metadataReferences.Add(assembly.FullName!, MetadataReference.CreateFromFile(assembly.Location));
            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                var referencedAssembly = Assembly.Load(assemblyName.FullName);
                AddMetadataReferencesRecursively(referencedAssembly, metadataReferences);
            }
        }
    }
}
