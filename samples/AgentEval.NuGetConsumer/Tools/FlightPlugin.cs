// SPDX-License-Identifier: MIT
// Copyright (c) 2026 AgentEval Contributors

using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;

namespace AgentEval.NuGetConsumer.Tools;

/// <summary>
/// Semantic Kernel native plugin for flight search and booking.
/// Uses [KernelFunction] + [Description] — the real SK plugin pattern.
/// Tools are deterministic stubs returning test data; the LLM reasoning is real.
/// </summary>
public class FlightPlugin
{
    [KernelFunction("SearchFlights")]
    [Description("Search for available flights between airports on a given date.")]
    public string SearchFlights(
        [Description("Departure city or airport code")] string origin,
        [Description("Destination city or airport code")] string destination,
        [Description("Travel date (YYYY-MM-DD or natural language)")] string date)
    {
        Console.WriteLine($"      🔧 [SK] SearchFlights({origin} → {destination}, {date})");

        var results = new[]
        {
            new { FlightId = "FL-123", Price = 450, Airline = "AirEval", Departs = "08:00", Arrives = "14:30" },
            new { FlightId = "FL-456", Price = 380, Airline = "SkyNet", Departs = "11:00", Arrives = "17:30" },
            new { FlightId = "FL-789", Price = 520, Airline = "TestAir", Departs = "15:00", Arrives = "21:30" },
        };
        return JsonSerializer.Serialize(results);
    }

    [KernelFunction("BookFlight")]
    [Description("Book a specific flight for the given number of passengers.")]
    public string BookFlight(
        [Description("Flight ID from search results")] string flightId,
        [Description("Number of passengers")] int passengers)
    {
        Console.WriteLine($"      🔧 [SK] BookFlight({flightId}, {passengers} pax)");

        return JsonSerializer.Serialize(new
        {
            ConfirmationCode = $"CONF-{flightId}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            FlightId = flightId,
            Passengers = passengers,
            Status = "Confirmed",
            TotalPrice = passengers * 380
        });
    }
}
