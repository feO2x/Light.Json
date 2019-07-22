using System;
using BenchmarkDotNet.Attributes;
using Light.GuardClauses;
using Light.Json.Tokenization;
using Light.Json.Tokenization.Utf16;
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
            var tokenizer = new JsonUtf16Tokenizer(Json);
            string firstName = null;
            string lastName = null;
            var age = 0;

            tokenizer.GetNextToken().Type.MustBe(JsonTokenType.BeginOfObject);
            JsonUtf16Token currentToken;
            do
            {
                currentToken = tokenizer.GetNextToken();
                currentToken.Type.MustBe(JsonTokenType.String);
                var dependencyName = currentToken.Text;
                tokenizer.GetNextToken().Type.MustBe(JsonTokenType.NameValueSeparator);
                currentToken = tokenizer.GetNextToken();
                if (dependencyName.Equals(nameof(firstName), StringComparison.Ordinal))
                {
                    currentToken.Type.MustBe(JsonTokenType.String);
                    firstName = currentToken.Text.ToString();
                }
                else if (dependencyName.Equals(nameof(lastName), StringComparison.Ordinal))
                {
                    currentToken.Type.MustBe(JsonTokenType.String);
                    lastName = currentToken.Text.ToString();
                }
                else if (dependencyName.Equals(nameof(age), StringComparison.Ordinal))
                {
                    currentToken.Type.MustBe(JsonTokenType.IntegerNumber);
                    age = int.Parse(currentToken.Text);
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