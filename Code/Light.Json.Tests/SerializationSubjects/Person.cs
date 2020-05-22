namespace Light.Json.Tests.SerializationSubjects
{
    // A simple mutable DTO
    public sealed class Person
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public int Age { get; set; }
    }
}