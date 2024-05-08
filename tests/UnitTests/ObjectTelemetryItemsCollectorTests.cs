namespace Byndyusoft.UnitTests
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Telemetry;
    using Telemetry.Abstraction;
    using Telemetry.Abstraction.Attributes;
    using Xunit;

    public class ObjectTelemetryItemsCollectorTests
    {
        [Theory]
        [MemberData(nameof(GetTestCaseData))]
        public void Collect(string parameterName,
                            object idValue,
                            string namePrefix,
                            TelemetryItem[] expectedTelemetryItems)
        {
            // Act
            var telemetryItems = ObjectTelemetryItemsCollector.Collect(parameterName, idValue, namePrefix);

            // Assert
            telemetryItems.Should().BeEquivalentTo(expectedTelemetryItems);
        }

        private static IEnumerable<object[]> GetTestCaseData()
        {
            var defaultNamePrefix = "param.";

            yield return new object[] { "id", 10, defaultNamePrefix, new[] { new TelemetryItem("param.id", 10) } };
            yield return new object[] { "id", 10, "", new[] { new TelemetryItem("id", 10) } };
            yield return new object[] { "id", 10, "http.", new[] { new TelemetryItem("http.id", 10) } };
            yield return new object[]
                             { "warehouse_id", 12, defaultNamePrefix, new[] { new TelemetryItem("param.warehouse_id", 12) } };
            yield return new object[]
                             { "stringId", "value", defaultNamePrefix, new[] { new TelemetryItem("param.stringId", "value") } };

            yield return new object[] { "name", 10, defaultNamePrefix, Array.Empty<TelemetryItem>() };
            yield return new object[] { "id", DateTime.Now, defaultNamePrefix, Array.Empty<TelemetryItem>() };

            int? nullableInt = 11;
            yield return new object[]
                             { "id", nullableInt, defaultNamePrefix, new[] { new TelemetryItem("param.id", 11) } };

            var testObject = new TestObject
                                 {
                                     ToLogInt = 13,
                                     ToLogString = "log_string",
                                     NotToLog = 14
                                 };
            var expectedTestObjectTelemetryItems = new TelemetryItem[]
                                                       {
                                                           new("param.ToLogInt", 13),
                                                           new("param.ToLogString", "log_string")
                                                       };

            yield return new object[] { "dto", testObject, defaultNamePrefix, expectedTestObjectTelemetryItems };
        }

        private class TestObject
        {
            [TelemetryItem] public int ToLogInt { get; set; }

            [TelemetryItem] public string? ToLogString { get; set; }

            public int NotToLog { get; set; }
        }
    }
}