using System;
using BenchmarkDotNet.Attributes;
using Light.GuardClauses;
using Newtonsoft.Json;

namespace Light.Json.Performance
{
    public class JsonNetVsImperativeBenchmark
    {
        public string Json = @"
{
    ""firstName"": ""John"",
    ""lastName"": ""Doe"",
    ""age"": 42
}";

        [Benchmark(Baseline = true)]
        public Contact JsonNet() => JsonConvert.DeserializeObject<Contact>(Json);

        [Benchmark]
        public Contact Imperative()
        {
            var tokenizer = new MemoryTextTokenizer(Json.AsMemory());
            string firstName = null;
            string lastName = null;
            var age = 0;

            tokenizer.GetNextToken().Type.MustBe(JsonTokenType.BeginOfObject);
            JsonTextToken currentToken;
            do
            {
                currentToken = tokenizer.GetNextToken();
                currentToken.Type.MustBe(JsonTokenType.String);
                var dependencyName = currentToken.Text;
                tokenizer.GetNextToken().Type.MustBe(JsonTokenType.NameValueSeparator);
                currentToken = tokenizer.GetNextToken();
                if (dependencyName.EqualsSpan(nameof(firstName)))
                {
                    currentToken.Type.MustBe(JsonTokenType.String);
                    firstName = currentToken.Text.ToString();
                }
                else if (dependencyName.EqualsSpan(nameof(lastName)))
                {
                    currentToken.Type.MustBe(JsonTokenType.String);
                    lastName = currentToken.Text.ToString();
                }
                else if (dependencyName.EqualsSpan(nameof(age)))
                {
                    currentToken.Type.MustBe(JsonTokenType.IntegerNumber);
                    age = int.Parse(currentToken.Text.First.Span);
                }
                else
                {
                    throw new DeserializationException($"Found unexpected token \"{dependencyName.ToString()}\".");
                }

                currentToken = tokenizer.GetNextToken();
            } while (currentToken.Type == JsonTokenType.EntrySeparator);

            currentToken.Type.MustBe(JsonTokenType.EndOfObject);

            return new Contact(firstName, lastName, age);
        }
    }

    public class Contact
    {
        public Contact(string firstName, string lastName, int age)
        {
            FirstName = firstName.MustNotBeNull(nameof(firstName));
            LastName = lastName.MustNotBeNull(nameof(lastName));
            Age = age.MustNotBeLessThan(0, nameof(age));
        }

        public string FirstName { get; }
        public string LastName { get; }
        public int Age { get; }
    }
}