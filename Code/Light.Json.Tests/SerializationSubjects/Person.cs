namespace Light.Json.Tests.SerializationSubjects
{
    // A simple mutable DTO
    public sealed class Person
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public int Age { get; set; }

        public static Person CreateDefaultInstance()
        {
            return new Person
            {
                FirstName = "Kenny",
                LastName = "Pflug",
                Age = 33
            };
        }

        public const string DefaultMinifiedJson = "{\"firstName\":\"Kenny\",\"lastName\":\"Pflug\",\"age\":33}";
    }
}