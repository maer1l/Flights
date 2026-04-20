using System;
using System.Collections.Generic;

namespace Flights.Models;

public partial class Flight
{
    public int FlightId { get; set; }

    public string? FlightNumber { get; set; }

    public string? Direction { get; set; }

    public string? GateNumber { get; set; }

    public int? PeopleCount { get; set; }
}
